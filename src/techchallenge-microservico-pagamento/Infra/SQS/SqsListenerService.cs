using Amazon.SQS.Model;
using Amazon.SQS;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Amazon.S3;
using Microsoft.Extensions.Configuration;
using techchallenge_microservico_pagamento.Repositories.Interfaces;
using techchallenge_microservico_pagamento.Models;
using Amazon.SQS.ExtendedClient;
using techchallenge_microservico_pagamento.Enums;

namespace Infra.SQS
{
    public class SqsListenerService : BackgroundService
    {
        private readonly ILogger<SqsListenerService> _logger;
        private readonly IPedidoRepository _pedidoRepository;
        private readonly bool _useLocalStack;
        private AmazonSQSClient _sqsClient;
        private readonly string _queueUrlRead;
        private readonly string _queueUrlRead2;
        private readonly List<string> _queueUrls;
        private readonly string _queueName;
        private readonly string _queueListenerName;
        private readonly string _queueListenerName2;
        private readonly bool _criarFila;
        private readonly bool _enviarMensagem;
        private readonly string _bucketName;
        private IAmazonSQS _amazonSQS;
        private IAmazonS3 _amazonS3;
        private readonly ISQSConfiguration _sqsConfiguration;
        private readonly string _queueUrlSend;

        public SqsListenerService(ILogger<SqsListenerService> logger, IPedidoRepository pedidoRepository, IConfiguration config, IAmazonS3 s3, IAmazonSQS sqs, ISQSConfiguration sqsConfiguration)
        {
            var useLocalStack = config.GetSection("SQSConfig").GetSection("useLocalStack").Value;
            var criarFila = config.GetSection("SQSConfig").GetSection("CreateTestQueue").Value;
            var enviarMensagem = config.GetSection("SQSConfig").GetSection("SendTestMessage").Value;

            _logger = logger;
            _pedidoRepository = pedidoRepository;
            _useLocalStack = Convert.ToBoolean(useLocalStack);
            _criarFila = Convert.ToBoolean(criarFila);
            _enviarMensagem = Convert.ToBoolean(enviarMensagem);
            _bucketName = config.GetSection("SQSExtendedClient").GetSection("S3Bucket").Value;
            _amazonS3 = s3;
            _queueUrlRead = config.GetSection("QueueUrlRead").Value;
            _queueUrlRead2 = config.GetSection("QueueUrlRead2").Value;
            _queueUrls = new List<string>() { _queueUrlRead, _queueUrlRead2 };
            _queueUrlSend = config.GetSection("QueueUrlSend").Value;
            _queueName = config.GetSection("SQSConfig").GetSection("TestQueueName").Value;
            _queueListenerName = config.GetSection("SQSConfig").GetSection("TestListenerQueueName").Value;
            _queueListenerName2 = config.GetSection("SQSConfig").GetSection("TestListenerQueueName2").Value;
            _amazonSQS = sqs;
            _sqsConfiguration = sqsConfiguration;
            _sqsClient = new AmazonSQSClient(Amazon.RegionEndpoint.GetBySystemName(
                    string.IsNullOrEmpty(Environment.GetEnvironmentVariable("MY_SECRET")) || Environment.GetEnvironmentVariable("MY_SECRET").Equals("{MY_SECRET}")
                    ? "us-east-1"
                    : Environment.GetEnvironmentVariable("MY_SECRET")));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                if (_useLocalStack)
                {
                    var configSQS = new AmazonSQSExtendedClient(_amazonSQS, new ExtendedClientConfiguration().WithLargePayloadSupportEnabled(_amazonS3, _bucketName));

                    if (_criarFila)
                        await _sqsConfiguration.CreateMessageInQueueWithStatusASyncLocalStack(configSQS, _queueListenerName);

                    if (_criarFila)
                        await _sqsConfiguration.CreateMessageInQueueWithStatusASyncLocalStack(configSQS, _queueListenerName2);

                    if (_enviarMensagem)
                        await _sqsConfiguration.SendTestMessageAsyncLocalStack(_queueUrlRead, configSQS);

                    if (_enviarMensagem)
                        await _sqsConfiguration.SendTestMessageAsyncLocalStack(_queueUrlRead2, configSQS);
                }

                while (!stoppingToken.IsCancellationRequested)
                {
                    var tasks = new List<Task>();

                    foreach (var queueUrl in _queueUrls)
                    {
                        tasks.Add(ProcessQueueAsync(queueUrl, stoppingToken));
                    }

                    await Task.WhenAll(tasks);
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
            }
            catch
            {
                throw;
            }
        }

        private async Task ProcessQueueAsync(string queueUrl, CancellationToken stoppingToken)
        {
            var receiveMessageRequest = new ReceiveMessageRequest
            {
                QueueUrl = queueUrl,
                WaitTimeSeconds = 10,
                AttributeNames = new List<string> { "ApproximateReceiveCount" },
                MessageAttributeNames = new List<string> { "All" },
                MaxNumberOfMessages = 10
            };

            var receiveMessageResponse = await _amazonSQS.ReceiveMessageAsync(receiveMessageRequest, stoppingToken);

            foreach (var message in receiveMessageResponse.Messages)
            {
                var pedido = new Pedido();
                var pedidoOrigem = new Pedido();

                try
                {
                    _logger.LogInformation($"Received message: {message.Body}");

                    if (queueUrl.Contains("pagamento"))
                    {
                        var obj = _sqsConfiguration.TratarMessageTransacaoPagamento(message.Body);

                        if (obj == null)
                            continue;

                        pedidoOrigem = await _pedidoRepository.GetPedidoByOrdemDePagamento(obj.OrderDePagamento);

                        if (pedidoOrigem.Status == EPedidoStatus.PendentePagamento)
                        {
                            pedidoOrigem.Status = EPedidoStatus.Recebido;
                            await _pedidoRepository.UpdatePedido(pedidoOrigem.IdPedidoOrigem, pedidoOrigem);

                            var messageJson = Newtonsoft.Json.JsonConvert.SerializeObject(pedidoOrigem);
                            await EnviarMessageSQS(messageJson);
                        };
                    }
                    else
                    {
                        var obj = _sqsConfiguration.TratarMessage(message.Body);

                        if (obj == null)
                            continue;

                        pedido = await CreatePedido(obj);
                        Console.WriteLine(message.Body);
                        _logger.LogInformation(message.Body);
                    }

                    var deleteMessageRequest = new DeleteMessageRequest
                    {
                        QueueUrl = queueUrl,
                        ReceiptHandle = message.ReceiptHandle
                    };

                    await _amazonSQS.DeleteMessageAsync(deleteMessageRequest, stoppingToken);
                }
                catch (Exception ex)
                {
                    if (pedido.Id != null)
                        await _pedidoRepository.DeletePedido(pedido.Id);

                    if (pedidoOrigem.Id != null && pedidoOrigem.Status == EPedidoStatus.Recebido)
                    {
                        pedidoOrigem.Status = EPedidoStatus.PendentePagamento;
                        await _pedidoRepository.UpdatePedido(pedidoOrigem.Id, pedidoOrigem);
                    }

                    _logger.LogError(ex.Message);
                }
            }
        }

        public async Task<Pedido> CreatePedido(Pedido pedido)
        {
            try
            {
                var numeroPedido = await _pedidoRepository.GetAllPedidos();

                var novoPedido = new Pedido
                {
                    Produtos = pedido.Produtos,
                    Total = pedido.Produtos.Sum(x => x.Preco),
                    Status = 0,
                    DataCriacao = DateTime.Now,
                    Numero = numeroPedido.Count + 1,
                    Usuario = pedido.Usuario,
                    IdCarrinho = pedido.IdCarrinho,
                    IdPedidoOrigem = pedido.Id
                };

                await _pedidoRepository.CreatePedido(novoPedido);

                return novoPedido;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new Exception(ex.Message);
            }
        }

        public async Task EnviarMessageSQS(string messageJson)
        {
            if (!_useLocalStack)
            {
                _sqsClient = await _sqsConfiguration.ConfigurarSQS();

                await _sqsConfiguration.EnviarParaSQS(messageJson, _sqsClient, _queueUrlSend);
            }
            else
            {
                var configSQS = new AmazonSQSExtendedClient(_amazonSQS, new ExtendedClientConfiguration().WithLargePayloadSupportEnabled(_amazonS3, _bucketName));

                if (_criarFila)
                    await _sqsConfiguration.CreateMessageInQueueWithStatusASyncLocalStack(configSQS, _queueName);

                if (_enviarMensagem)
                    await _sqsConfiguration.SendTestMessageAsyncLocalStack(_queueUrlRead, configSQS);

                await configSQS.SendMessageAsync(_queueUrlSend, messageJson);
            }
        }
    }
}

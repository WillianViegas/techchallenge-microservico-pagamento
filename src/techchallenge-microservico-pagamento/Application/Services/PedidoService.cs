using techchallenge_microservico_pagamento.Enums;
using techchallenge_microservico_pagamento.Models;
using techchallenge_microservico_pagamento.Repositories.Interfaces;
using techchallenge_microservico_pagamento.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Amazon.S3;
using Amazon.SQS;
using Microsoft.Extensions.Configuration;
using Amazon.SQS.Model;
using System.Net;
using Infra.SQS;
using Amazon.SQS.ExtendedClient;
using techchallenge_microservico_pedido.Models;

namespace techchallenge_microservico_pagamento.Services
{
    public class PedidoService : IPedidoService
    {

        private readonly ILogger<PedidoService> _logger;
        private readonly IPedidoRepository _pedidoRepository;
        private readonly ICarrinhoRepository _carrinhoRepository;
        private readonly bool _useLocalStack;
        private AmazonSQSClient _sqs;
        private readonly string _queueUrlRead;
        private readonly string _queueUrlSend;
        private readonly bool _criarFila;
        private readonly bool _enviarMensagem;
        private readonly string _queueName;
        private readonly string _bucketName;
        private IAmazonSQS _amazonSQS;
        private IAmazonS3 _amazonS3;
        private readonly ISQSConfiguration _sQSConfiguration;

        public PedidoService(ILogger<PedidoService> logger, IPedidoRepository pedidoRepository, ICarrinhoRepository carrinhoRepository, IConfiguration config, IAmazonS3 s3, IAmazonSQS sqs, ISQSConfiguration sqsConfiguration)
        {
            var criarFila = config.GetSection("SQSConfig").GetSection("CreateTestQueue").Value;
            var enviarMensagem = config.GetSection("SQSConfig").GetSection("SendTestMessage").Value;
            var useLocalStack = config.GetSection("SQSConfig").GetSection("useLocalStack").Value;

            _sQSConfiguration = sqsConfiguration;
            _logger = logger;
            _pedidoRepository = pedidoRepository;
            _carrinhoRepository = carrinhoRepository;
            _useLocalStack = Convert.ToBoolean(useLocalStack);
            _criarFila = Convert.ToBoolean(criarFila);
            _queueUrlRead = config.GetSection("QueueUrlRead").Value;
            _queueUrlSend = config.GetSection("QueueUrlSend").Value;
            _enviarMensagem = Convert.ToBoolean(enviarMensagem);
            _queueName = config.GetSection("SQSConfig").GetSection("TestQueueName").Value;
            _bucketName = config.GetSection("SQSExtendedClient").GetSection("S3Bucket").Value;
            _amazonS3 = s3;
            _amazonSQS = sqs;
            _sqs = new AmazonSQSClient(Amazon.RegionEndpoint.GetBySystemName(
                    string.IsNullOrEmpty(Environment.GetEnvironmentVariable("MY_SECRET")) || Environment.GetEnvironmentVariable("MY_SECRET").Equals("{MY_SECRET}")
                    ? "us-east-1"
                    : Environment.GetEnvironmentVariable("MY_SECRET")));

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

        public async Task<Pedido> FinalizarPedido(string id)
        {
            try
            {
                var pedido = await GetPedidoByIdOrigem(id);
                if (pedido is null)
                {
                    _logger.LogError("Pedido não encontrado");
                    throw new Exception("Pedido não existe");
                }

                if (pedido.Status != EPedidoStatus.Novo) throw new Exception($"Status do pedido não é válido para confirmação. Status: {pedido.Status}, NumeroPedido: {pedido.Numero}");

                pedido.Status = EPedidoStatus.PendentePagamento;
                pedido.Pagamento = new Pagamento()
                {
                    Tipo = ETipoPagamento.QRCode,
                    QRCodeUrl = "www.usdfhosdfsdhfosdfhsdofhdsfds.com.br",
                    OrdemDePagamento = Guid.NewGuid().ToString()
                };

                await _pedidoRepository.UpdatePedido(id, pedido);
                _logger.LogInformation($"Pedido atualizado id: {id}, status: {pedido.Status.ToString()}");

                //desativa o carrinho (pensar se futuramente n é melhor excluir)
                var carrinho = await _carrinhoRepository.GetCarrinhoById(id);
                if (carrinho != null)
                {
                    carrinho.Ativo = false;
                    await _carrinhoRepository.UpdateCarrinho(carrinho.Id, carrinho);
                }

                return pedido;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new Exception(ex.Message);
            }
        }

        public async Task<Pedido> GetPedidoByIdOrigem(string id)
        {
            try
            {
                return await _pedidoRepository.GetPedidoByIdOrigem(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new Exception(ex.Message);
            }
        }

        async Task EnviarMessageSQS(string messageJson)
        {
            if (!_useLocalStack)
            {
                _sqs = await _sQSConfiguration.ConfigurarSQS();

                await _sQSConfiguration.EnviarParaSQS(messageJson, _sqs, _queueUrlSend);
            }
            else
            {
                var configSQS = new AmazonSQSExtendedClient(_amazonSQS, new ExtendedClientConfiguration().WithLargePayloadSupportEnabled(_amazonS3, _bucketName));

                if (_criarFila)
                    await _sQSConfiguration.CreateMessageInQueueWithStatusASyncLocalStack(configSQS, _queueName);

                if (_enviarMensagem)
                    await _sQSConfiguration.SendTestMessageAsyncLocalStack(_queueUrlRead, configSQS);

                await configSQS.SendMessageAsync(_queueUrlSend, messageJson);
            }
        }

        public async Task RegistrarPedidos()
        {
            try
            {
                _logger.LogDebug("Reading queue...");

                var response = await _amazonSQS.ReceiveMessageAsync(new ReceiveMessageRequest
                {
                    QueueUrl = _queueUrlRead,
                    WaitTimeSeconds = 10,
                    AttributeNames = new List<string> { "ApproximateReceiveCount" },
                    MessageAttributeNames = new List<string> { "All" },
                    MaxNumberOfMessages = 10
                });

                if (response.HttpStatusCode != HttpStatusCode.OK)
                {
                    _logger.LogError($"Error reading the queue: {_queueUrlRead}!");
                    throw new AmazonSQSException($"Failed to GetMessages for queue {_queueUrlRead}. Response: {response.HttpStatusCode}");
                }

                foreach (var message in response.Messages)
                {
                    var pedido = new Pedido();

                    try
                    {
                        var obj = _sQSConfiguration.TratarMessage(message.Body);

                        if (obj == null)
                            continue;

                        //chamar confirmar pedido
                        pedido = await CreatePedido(obj);
                        Console.WriteLine(message.Body);
                        _logger.LogInformation(message.Body);

                        await _amazonSQS.DeleteMessageAsync(_queueUrlRead, message.ReceiptHandle);
                        _logger.LogInformation($"Message deleted");
                    }
                    catch (Exception ex)
                    {
                        if (pedido.Id != null)
                            await _pedidoRepository.DeletePedido(pedido.Id);

                        _logger.LogError(ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing messages for queue {_queueUrlRead}!");
            }
        }
    }
}

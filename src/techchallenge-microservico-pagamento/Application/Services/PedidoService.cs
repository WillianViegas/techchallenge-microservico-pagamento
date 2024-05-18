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
        private readonly string _queueUrl;
        private readonly bool _criarFila;
        private readonly bool _enviarMensagem;
        private readonly string _queueName;
        private readonly string _bucketName;
        private IAmazonSQS _amazonSQS;
        private IAmazonS3 _amazonS3;

        public PedidoService(ILogger<PedidoService> logger, IPedidoRepository pedidoRepository, ICarrinhoRepository carrinhoRepository, IConfiguration config, IAmazonS3 s3, IAmazonSQS sqs)
        {
            var criarFila = config.GetSection("SQSConfig").GetSection("CreateTestQueue").Value;
            var enviarMensagem = config.GetSection("SQSConfig").GetSection("SendTestMessage").Value;
            var useLocalStack = config.GetSection("SQSConfig").GetSection("useLocalStack").Value;

            _logger = logger;
            _pedidoRepository = pedidoRepository;
            _carrinhoRepository = carrinhoRepository;
            _useLocalStack = Convert.ToBoolean(useLocalStack);
            _criarFila = Convert.ToBoolean(criarFila);
            _queueUrl = config.GetSection("QueueUrl").Value;
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

                var messageJson = Newtonsoft.Json.JsonConvert.SerializeObject(pedido);
                await EnviarMessageSQS(messageJson);

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
                var sqsConfiguration = new SQSConfiguration();
                _sqs = await sqsConfiguration.ConfigurarSQS();

                await sqsConfiguration.EnviarParaSQS(messageJson, _sqs);
            }
            else
            {
                var configSQS = new AmazonSQSExtendedClient(_amazonSQS, new ExtendedClientConfiguration().WithLargePayloadSupportEnabled(_amazonS3, _bucketName));

                if (_criarFila)
                    await CreateMessageInQueueWithStatusASyncLocalStack(configSQS);

                if (_enviarMensagem)
                    await SendTestMessageAsyncLocalStack(_queueUrl, configSQS);

                await configSQS.SendMessageAsync(_queueUrl, messageJson);
            }
        }

        async Task SendTestMessageAsyncLocalStack(string queue, AmazonSQSExtendedClient sqs)
        {
            var messageBody = new MessageBody();
            messageBody.IdTransacao = Guid.NewGuid().ToString();
            messageBody.idPedido = "65a315fadb1f522d916d9361";
            messageBody.Status = "OK";
            messageBody.DataTransacao = DateTime.Now;

            var jsonObj = Newtonsoft.Json.JsonConvert.SerializeObject(messageBody);

            await sqs.SendMessageAsync(queue, jsonObj);
        }

        async Task CreateMessageInQueueWithStatusASyncLocalStack(AmazonSQSExtendedClient sqs)
        {
            var responseQueue = await sqs.CreateQueueAsync(new CreateQueueRequest(_queueName));

            if (responseQueue.HttpStatusCode != HttpStatusCode.OK)
            {
                var erro = $"Failed to CreateQueue for queue {_queueName}. Response: {responseQueue.HttpStatusCode}";
                //log.LogError(erro);
                throw new AmazonSQSException(erro);
            }
        }

        public async Task RegistrarPedidos()
        {
            try
            {
                _logger.LogDebug("Reading queue...");

                var response = await _amazonSQS.ReceiveMessageAsync(new ReceiveMessageRequest
                {
                    QueueUrl = _queueUrl,
                    WaitTimeSeconds = 10,
                    AttributeNames = new List<string> { "ApproximateReceiveCount" },
                    MessageAttributeNames = new List<string> { "All" },
                    MaxNumberOfMessages = 10
                });

                if (response.HttpStatusCode != HttpStatusCode.OK)
                {
                    _logger.LogError($"Error creating the queue: {_queueUrl}!");
                    throw new AmazonSQSException($"Failed to GetMessages for queue {_queueUrl}. Response: {response.HttpStatusCode}");
                }

                foreach (var message in response.Messages)
                {

                    var obj = TratarMessage(message.Body);

                    if (obj == null)
                        continue;

                    //chamar confirmar pedido
                    var pedido = await CreatePedido(obj);
                    Console.WriteLine(message.Body);
                    _logger.LogInformation(message.Body);

                    await _amazonSQS.DeleteMessageAsync(_queueUrl, message.ReceiptHandle);
                    _logger.LogInformation($"Message deleted");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing messages for queue {_queueUrl}!");
            }
        }

        private Pedido? TratarMessage(string body)
        {
            var obj = new Pedido();
            try
            {
                obj = Newtonsoft.Json.JsonConvert.DeserializeObject<Pedido>(body);

                if (string.IsNullOrEmpty(obj.Id))
                {
                    _logger.LogWarning($"TratarMessage: objeto não tinha id. Body: {body}");
                    return null;
                }

            }
            catch
            {
                _logger.LogError($"TratarMessage: erro ao deserializar json. Body: {body}");
                return null;
            }

            return obj;
        }

    }
}

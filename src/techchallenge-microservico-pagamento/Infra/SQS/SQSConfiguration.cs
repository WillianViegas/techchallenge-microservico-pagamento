using Amazon.Runtime.Internal.Util;
using Amazon.S3;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Amazon.SQS;
using Amazon.SQS.ExtendedClient;
using Amazon.SQS.Model;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net;
using techchallenge_microservico_pagamento.Models;
using techchallenge_microservico_pedido.Models;

namespace Infra.SQS
{
    public class SQSConfiguration : ISQSConfiguration
    {
        private readonly ILogger<SQSConfiguration> _logger;

        public SQSConfiguration(ILogger<SQSConfiguration> log)
        {
            _logger = log;
        }

        public async Task<AmazonSQSClient> ConfigurarSQS()
        {
            using (var secretsManagerClient = new AmazonSecretsManagerClient())
            {
                var secretName = Environment.GetEnvironmentVariable("MY_SECRET");
                var getSecretValueRequest = new GetSecretValueRequest
                {
                    SecretId = secretName
                };

                var getSecretValueResponse = await secretsManagerClient.GetSecretValueAsync(getSecretValueRequest);
                var secretString = getSecretValueResponse.SecretString;

                var sqsConnectionDetails = ParseSecretString(secretString);

                var sqsConfig = new AmazonSQSConfig
                {
                    RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(sqsConnectionDetails.Region)
                };

                var sqsClient = new AmazonSQSClient(sqsConnectionDetails.AccessKeyId, sqsConnectionDetails.SecretAccessKey, sqsConfig);
                
                return sqsClient;
            }
        }

        private SqsConnectionDetails ParseSecretString(string secretString)
        {
            return JsonConvert.DeserializeObject<SqsConnectionDetails>(secretString);
        }

        public async Task EnviarParaSQS(string jsonMessage, AmazonSQSClient sqsclient, string queueUrl)
        {
            try
            {
                Console.WriteLine($"SQS Queue URL: {queueUrl}");
                await sqsclient.SendMessageAsync(queueUrl, jsonMessage);
            }
            catch
            {
                //logar
            }
          
        }

        public Pedido? TratarMessage(string body)
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

        public async Task CreateMessageInQueueWithStatusASyncLocalStack(AmazonSQSExtendedClient sqs, string queueName)
        {
            var responseQueue = await sqs.CreateQueueAsync(new CreateQueueRequest(queueName));

            if (responseQueue.HttpStatusCode != HttpStatusCode.OK)
            {
                var erro = $"Failed to CreateQueue for queue {queueName}. Response: {responseQueue.HttpStatusCode}";
                //log.LogError(erro);
                throw new AmazonSQSException(erro);
            }
        }

        public async Task SendTestMessageAsyncLocalStack(string queue, AmazonSQSExtendedClient sqs)
        {
            var messageBody = new MessageBody();
            messageBody.IdTransacao = Guid.NewGuid().ToString();
            messageBody.idPedido = "65a315fadb1f522d916d9361";
            messageBody.Status = "OK";
            messageBody.DataTransacao = DateTime.Now;

            var jsonObj = Newtonsoft.Json.JsonConvert.SerializeObject(messageBody);

            await sqs.SendMessageAsync(queue, jsonObj);
        }

        class SqsConnectionDetails
        {
            public string AccessKeyId { get; set; }
            public string SecretAccessKey { get; set; }
            public string Region { get; set; }
        }
    }
}

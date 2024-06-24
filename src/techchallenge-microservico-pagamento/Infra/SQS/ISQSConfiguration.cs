using Amazon.SQS;
using Amazon.SQS.ExtendedClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using techchallenge_microservico_pagamento.Models;

namespace Infra.SQS
{
    public interface ISQSConfiguration
    {
        public Task<AmazonSQSClient> ConfigurarSQS();
        public Task EnviarParaSQS(string jsonMessage, AmazonSQSClient sqsclient, string queueUrl);
        public Pedido? TratarMessage(string body);
        public Task CreateMessageInQueueWithStatusASyncLocalStack(AmazonSQSExtendedClient sqs, string queueName);
        public Task SendTestMessageAsyncLocalStack(string queue, AmazonSQSExtendedClient sqs);
    }
}

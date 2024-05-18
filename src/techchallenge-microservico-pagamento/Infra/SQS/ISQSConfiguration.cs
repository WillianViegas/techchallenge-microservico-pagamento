using Amazon.SQS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infra.SQS
{
    public interface ISQSConfiguration
    {
        public Task<AmazonSQSClient> ConfigurarSQS();
        public Task EnviarParaSQS(string jsonMessage, AmazonSQSClient sqsclient);
    }
}

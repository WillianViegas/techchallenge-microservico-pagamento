using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Amazon.SQS;
using Amazon.SQS.Model;
using Newtonsoft.Json;

namespace Infra.SQS
{
    public class SQSConfiguration : ISQSConfiguration
    {
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

        class SqsConnectionDetails
        {
            public string AccessKeyId { get; set; }
            public string SecretAccessKey { get; set; }
            public string Region { get; set; }
        }
    }
}

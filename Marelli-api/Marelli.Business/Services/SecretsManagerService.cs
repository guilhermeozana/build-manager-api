using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Marelli.Business.IClients;
using Marelli.Business.IServices;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace Marelli.Business.Services
{
    public class SecretsManagerService : ISecretsManagerService
    {
        private readonly IConfiguration _configuration;
        private readonly IAmazonSecretsManager _secretsManagerClient;

        public SecretsManagerService(IConfiguration configuration, IAwsClient awsService)
        {
            _configuration = configuration;
            _secretsManagerClient = awsService.GetAmazonSecretsManagerClient();
        }

        public async Task<string> GetDbConnectionString()
        {

            GetSecretValueRequest request = new GetSecretValueRequest
            {
                SecretId = _configuration["AWS:SecretName"],
                VersionStage = "AWSCURRENT", // VersionStage defaults to AWSCURRENT if unspecified.
            };

            GetSecretValueResponse response;

            try
            {
                response = await _secretsManagerClient.GetSecretValueAsync(request);
            }
            catch (Exception e)
            {
                // For a list of the exceptions thrown, see
                // https://docs.aws.amazon.com/secretsmanager/latest/apireference/API_GetSecretValue.html
                throw new Exception(e.Message);
            }

            var dbConnectionString = "";

            if (response.SecretString != null)
            {
                var secretJson = JObject.Parse(response.SecretString);
                var username = secretJson["username"] != null ? secretJson["username"].ToString() : "";
                var password = secretJson["password"] != null ? secretJson["password"].ToString() : "";
                var engine = secretJson["engine"] != null ? secretJson["engine"].ToString() : "";
                var host = secretJson["host"] != null ? secretJson["host"].ToString() : "";
                var port = secretJson["port"] != null ? secretJson["port"].ToString() : "";
                var dbname = secretJson["dbname"] != null ? secretJson["dbname"].ToString() : "";

                dbConnectionString = $"Host={host};Port={port};Pooling=true;Database={dbname};User Id={username};Password={password};";
            }

            return dbConnectionString;
        }
    }
}

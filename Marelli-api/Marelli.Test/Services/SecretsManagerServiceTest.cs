using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Marelli.Business.IClients;
using Marelli.Business.Services;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace Marelli.Test.Services
{
    public class SecretsManagerServiceTests
    {
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<IAmazonSecretsManager> _amazonSecretsManagerMock;
        private readonly Mock<IAwsClient> _awsServiceMock;
        private readonly SecretsManagerService _secretsManagerService;

        public SecretsManagerServiceTests()
        {
            _configurationMock = new Mock<IConfiguration>();

            _configurationMock.Setup(c => c["AWS:AccessKeyId"]).Returns("fake-access-key");
            _configurationMock.Setup(c => c["AWS:SecretAccessKey"]).Returns("fake-secret-key");
            _configurationMock.Setup(c => c["AWS:Region"]).Returns("us-east-1");
            _configurationMock.Setup(c => c["AWS:SecretName"]).Returns("fake-secret-name");

            _amazonSecretsManagerMock = new Mock<IAmazonSecretsManager>();
            _awsServiceMock = new Mock<IAwsClient>();

            _awsServiceMock.Setup(a => a.GetAmazonSecretsManagerClient()).Returns(_amazonSecretsManagerMock.Object);

            _secretsManagerService = new SecretsManagerService(_configurationMock.Object, _awsServiceMock.Object);


        }

        [Fact]
        public async Task GetDbConnectionString_ShouldReturnCorrectConnectionString()
        {
            var secretString = @"
            {
                ""username"": ""testuser"",
                ""password"": ""testpassword"",
                ""engine"": ""postgres"",
                ""host"": ""localhost"",
                ""port"": ""5432"",
                ""dbname"": ""testdb""
            }";

            var secretResponse = new GetSecretValueResponse
            {
                SecretString = secretString
            };

            _amazonSecretsManagerMock
                .Setup(a => a.GetSecretValueAsync(It.IsAny<GetSecretValueRequest>(), default))
                .ReturnsAsync(secretResponse);

            var result = await _secretsManagerService.GetDbConnectionString();

            var expectedConnectionString = "Host=localhost;Port=5432;Pooling=true;Database=testdb;User Id=testuser;Password=testpassword;";
            Assert.Equal(expectedConnectionString, result);
        }

        [Fact]
        public async Task GetDbConnectionString_ShouldThrowException()
        {

            _amazonSecretsManagerMock
                .Setup(a => a.GetSecretValueAsync(It.IsAny<GetSecretValueRequest>(), default))
                .ThrowsAsync(new Exception());

            await Assert.ThrowsAsync<Exception>(async () => await _secretsManagerService.GetDbConnectionString());
        }
    }
}

using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Amazon.SecretsManager;
using Marelli.Business.Clients;
using Marelli.Business.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace Marelli.Tests.Clients
{
    public class AwsClientTests
    {
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<IHubContext<UploadProgressHub>> _hubContextMock;
        private readonly Mock<IClientProxy> _clientProxyMock;
        private readonly AwsClient _awsClient;

        public AwsClientTests()
        {
            _configurationMock = new Mock<IConfiguration>();
            _configurationMock.Setup(config => config["AWS:AccessKeyId"]).Returns("fakeAccessKeyId");
            _configurationMock.Setup(config => config["AWS:SecretAccessKey"]).Returns("fakeSecretAccessKey");
            _configurationMock.Setup(config => config["AWS:Region"]).Returns("us-west-2");

            _hubContextMock = new Mock<IHubContext<UploadProgressHub>>();
            _clientProxyMock = new Mock<IClientProxy>();

            _hubContextMock.Setup(h => h.Clients.All).Returns(_clientProxyMock.Object);

            _awsClient = new AwsClient(_configurationMock.Object, _hubContextMock.Object);
        }

        [Fact]
        public void GetAmazonS3Client_ShouldReturnS3ClientWithCorrectConfiguration()
        {
            // Act
            var s3Client = _awsClient.GetAmazonS3Client();

            // Assert
            Assert.NotNull(s3Client);
            var amazonS3Client = s3Client as AmazonS3Client;
            Assert.NotNull(amazonS3Client);
        }

        [Fact]
        public void GetTransferUtility_ShouldReturnTransferUtilityInstance()
        {
            // Arrange
            var amazonS3Mock = new Mock<IAmazonS3>();
            var awsClientMock = new Mock<AwsClient>(_configurationMock.Object, _hubContextMock.Object)
            {
                CallBase = true
            };

            awsClientMock.Setup(ac => ac.GetAmazonS3Client()).Returns(amazonS3Mock.Object);

            // Act
            var transferUtility = awsClientMock.Object.GetTransferUtility();

            // Assert
            Assert.NotNull(transferUtility);
            Assert.IsType<TransferUtility>(transferUtility);

            awsClientMock.Verify(ac => ac.GetAmazonS3Client(), Times.Once);
        }

        [Fact]
        public async Task DoesS3BucketExistAsync_ShouldReturnTrue_WhenBucketExists()
        {
            // Arrange
            var mockS3Client = new Mock<IAmazonS3>();
            mockS3Client.Setup(client => client.ListBucketsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ListBucketsResponse
                {
                    Buckets = new List<S3Bucket>
                    {
                    new S3Bucket { BucketName = "test-bucket" }
                    }
                });

            // Act
            var result = await _awsClient.DoesS3BucketExistAsync(mockS3Client.Object, "test-bucket");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task UploadS3File_ShouldUploadFile()
        {
            // Arrange
            var mockTransferUtility = new Mock<ITransferUtility>();
            var mockStream = new MemoryStream();
            var bucketName = "test-bucket";
            var key = "test-key";

            mockTransferUtility.Setup(tu => tu.UploadAsync(It.IsAny<TransferUtilityUploadRequest>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            await _awsClient.UploadS3File(mockTransferUtility.Object, mockStream, bucketName, key);

            // Assert
            mockTransferUtility.Verify(tu => tu.UploadAsync(It.IsAny<TransferUtilityUploadRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public void GetAmazonSecretsManagerClient_ShouldReturnSecretsManagerClientWithCorrectConfiguration()
        {
            // Act
            var secretsManagerClient = _awsClient.GetAmazonSecretsManagerClient();

            // Assert
            Assert.NotNull(secretsManagerClient);
            var amazonSecretsManagerClient = secretsManagerClient as AmazonSecretsManagerClient;
            Assert.NotNull(amazonSecretsManagerClient);
        }
    }
}

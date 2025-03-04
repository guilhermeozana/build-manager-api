using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Marelli.Domain.Dtos;
using Marelli.Domain.Entities;
using Marelli.Infra.Context;
using Marelli.Test.Integration.Configuration;
using Marelli.Test.Utils.Factories;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Newtonsoft.Json;
using System.Net;
using Xunit;


namespace Marelli.Test.Integration
{
    [Collection("Integration collection")]
    public class FileIntegrationTest
    {
        private readonly IntegrationSetupFixture _fixture;
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _httpClient;

        public FileIntegrationTest(IntegrationSetupFixture fixture)
        {
            _fixture = fixture;
            _factory = fixture.Factory;
            _httpClient = fixture.HttpClient;
        }

        [Fact]
        public async Task UploadZip_ShouldReturnBuildTableRowInstance()
        {
            //Arrange
            var user = UserFactory.GetUserWithoutGroup();
            var project = ProjectFactory.GetProject();

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();
                context.User.Add(user);
                context.Project.Add(project);

                await context.SaveChangesAsync();
            }

            _fixture.AwsClientMock.Reset();

            _fixture.AwsClientMock.Setup(a => a.DoesS3BucketExistAsync(It.IsAny<IAmazonS3>(), It.IsAny<string>())).ReturnsAsync(true);
            _fixture.AwsClientMock.Setup(a => a.UploadS3File(It.IsAny<TransferUtility>(), It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()));

            var formData = FileFactory.GetMultipartFormDataContent();

            //Act
            var response = await _httpClient.PostAsync($"/api/File/AWS/UploadZip/{user.Id}/{project.Id}", formData);

            //Assert
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<BuildTableRow>(responseContent);

            Assert.NotNull(result);
            Assert.Equal(user.Id, result.UserId);
            Assert.Equal(project.Id, result.ProjectId);


            _fixture.AwsClientMock.Verify(a => a.UploadS3File(It.IsAny<TransferUtility>(), It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }


        [Fact]
        public async Task UploadZip_ShouldReturnInternalServerErrorResult_WhenFileIsEmpty()
        {
            //Arrange
            var user = UserFactory.GetUserWithoutGroup();
            var project = ProjectFactory.GetProject();

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();
                context.User.Add(user);
                context.Project.Add(project);

                await context.SaveChangesAsync();
            }

            _fixture.AwsClientMock.Setup(a => a.DoesS3BucketExistAsync(It.IsAny<IAmazonS3>(), It.IsAny<string>())).ReturnsAsync(true);
            _fixture.AwsClientMock.Setup(a => a.UploadS3File(It.IsAny<TransferUtility>(), It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()));

            var formData = FileFactory.GetEmptyMultipartFormDataContent();

            //Act
            var response = await _httpClient.PostAsync($"/api/File/AWS/UploadZip/{user.Id}/{project.Id}", formData);

            //Assert
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.Contains("File is empty or null.", responseContent);

        }

        [Fact]
        public async Task UploadZip_ShouldReturnInternalServerErrorResult_WhenBucketDoesNotExist()
        {
            //Arrange
            var user = UserFactory.GetUserWithoutGroup();
            var project = ProjectFactory.GetProject();

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();
                context.User.Add(user);
                context.Project.Add(project);

                await context.SaveChangesAsync();
            }

            _fixture.AwsClientMock.Setup(a => a.DoesS3BucketExistAsync(It.IsAny<IAmazonS3>(), It.IsAny<string>())).ReturnsAsync(false);

            var formData = FileFactory.GetMultipartFormDataContent();

            //Act
            var response = await _httpClient.PostAsync($"/api/File/AWS/UploadZip/{user.Id}/{project.Id}", formData);

            //Assert
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.Contains("S3 bucket does not exist.", responseContent);

        }

        [Fact]
        public async Task UploadZip_ShouldReturnInternalServerErrorResult_WhenUserNotFound()
        {
            //Arrange
            var project = ProjectFactory.GetProject();

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();
                context.Project.Add(project);

                await context.SaveChangesAsync();
            }

            _fixture.AwsClientMock.Setup(a => a.DoesS3BucketExistAsync(It.IsAny<IAmazonS3>(), It.IsAny<string>())).ReturnsAsync(true);

            var formData = FileFactory.GetMultipartFormDataContent();

            var unexistentId = -1;

            //Act
            var response = await _httpClient.PostAsync($"/api/File/AWS/UploadZip/{unexistentId}/{project.Id}", formData);

            //Assert
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.Contains("not found", responseContent);

        }

        [Fact]
        public async Task UploadZip_ShouldReturnInternalServerErrorResult_WhenProjectNotFound()
        {
            //Arrange
            var user = UserFactory.GetUserWithoutGroup();

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();
                context.User.Add(user);

                await context.SaveChangesAsync();
            }

            _fixture.AwsClientMock.Setup(a => a.DoesS3BucketExistAsync(It.IsAny<IAmazonS3>(), It.IsAny<string>())).ReturnsAsync(true);

            var formData = FileFactory.GetMultipartFormDataContent();

            var unexistentId = -1;

            //Act
            var response = await _httpClient.PostAsync($"/api/File/AWS/UploadZip/{user.Id}/{unexistentId}", formData);

            //Assert
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.Contains("not found", responseContent);

        }

        [Fact]
        public async Task DownloadZip_ShouldReturnFileResponseInstance()
        {
            //Arrange
            var logAndArtifact = JenkinsFactory.GetLogAndArtifact();

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();
                context.LogAndArtifact.Add(logAndArtifact);

                await context.SaveChangesAsync();
            }

            _fixture.AwsClientMock.Setup(a => a.GetAmazonS3Client()).Returns(_fixture.AmazonS3ClientMock.Object);
            _fixture.AmazonS3ClientMock.Setup(a => a.GetPreSignedURL(It.IsAny<GetPreSignedUrlRequest>())).Returns("http://test.com");

            //Act
            var response = await _httpClient.GetAsync($"/api/File/AWS/DownloadZip/{logAndArtifact.BuildId}");

            //Assert
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<FileResponse>(responseContent);

            Assert.NotNull(result);
            Assert.Equal("application/zip", result.MimeType);
        }

        [Fact]
        public async Task DownloadZip_ShouldReturnNotFoundResult_WhenLogAndArtifactNotFound()
        {
            //Arrange

            var unexistentId = -1;

            //Act
            var response = await _httpClient.GetAsync($"/api/File/AWS/DownloadZip/{unexistentId}");

            //Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.Contains("not available", responseContent);

        }

        [Fact]
        public async Task RemoveZip_ShouldReturnOkResultMessage()
        {
            //Arrange
            var buildTable = BuildTableRowFactory.GetBuildTable();

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();
                context.BuildTableRow.Add(buildTable);

                await context.SaveChangesAsync();
            }

            _fixture.AwsClientMock.Setup(a => a.DoesS3BucketExistAsync(It.IsAny<IAmazonS3>(), It.IsAny<string>())).ReturnsAsync(true);
            _fixture.AwsClientMock.Setup(a => a.GetAmazonS3Client()).Returns(_fixture.AmazonS3ClientMock.Object);
            _fixture.AmazonS3ClientMock.Setup(a => a.DeleteObjectAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()));

            var formData = FileFactory.GetMultipartFormDataContent();

            //Act
            var response = await _httpClient.DeleteAsync($"/api/File/AWS/RemoveZip/{buildTable.UserId}/{buildTable.ProjectId}/{buildTable.Id}");

            //Assert
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();

            Assert.Equal("File deleted successfully.", responseContent);
        }

        [Fact]
        public async Task RemoveZip_ShouldReturnNotFoundResult_WhenBuildTableNotFound()
        {
            //Arrange
            var unexistentId = -1;

            //Act
            var response = await _httpClient.DeleteAsync($"/api/File/AWS/RemoveZip/1/1/{unexistentId}");

            //Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.Contains("not found", responseContent);

        }


        [Fact]
        public async Task RemoveZip_ShouldReturnInternalServerErrorResult_WhenS3BucketDoesNotExist()
        {
            //Arrange
            var buildTable = BuildTableRowFactory.GetBuildTable();

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();
                context.BuildTableRow.Add(buildTable);

                await context.SaveChangesAsync();
            }

            _fixture.AwsClientMock.Setup(a => a.DoesS3BucketExistAsync(It.IsAny<IAmazonS3>(), It.IsAny<string>())).ReturnsAsync(false);

            var formData = FileFactory.GetEmptyMultipartFormDataContent();

            //Act
            var response = await _httpClient.DeleteAsync($"/api/File/AWS/RemoveZip/{buildTable.UserId}/{buildTable.ProjectId}/{buildTable.Id}");

            //Assert
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.Contains("S3 bucket does not exist.", responseContent);

        }

        [Fact]
        public async Task UploadBaseline_ShouldReturnOkResultMessage()
        {
            //Arrange
            var project = ProjectFactory.GetProject();

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();
                context.Project.Add(project);

                await context.SaveChangesAsync();
            }

            _fixture.AwsClientMock.Reset();
            _fixture.AwsClientMock.Setup(a => a.DoesS3BucketExistAsync(It.IsAny<IAmazonS3>(), It.IsAny<string>())).ReturnsAsync(true);
            _fixture.AwsClientMock.Setup(a => a.UploadS3File(It.IsAny<TransferUtility>(), It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()));

            var formData = FileFactory.GetMultipartFormDataContent();

            //Act
            var response = await _httpClient.PostAsync($"/api/File/AWS/UploadBaseline/{project.Id}/test", formData);

            //Assert
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<Baseline>(responseContent);

            Assert.IsType<Baseline>(result);
            Assert.NotNull(result);

            _fixture.AwsClientMock.Verify(a => a.UploadS3File(It.IsAny<TransferUtility>(), It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }


        [Fact]
        public async Task UploadBaseline_ShouldReturnInternalServerErrorResult_WhenFileIsEmpty()
        {
            //Arrange
            _fixture.AwsClientMock.Setup(a => a.DoesS3BucketExistAsync(It.IsAny<IAmazonS3>(), It.IsAny<string>())).ReturnsAsync(true);
            _fixture.AwsClientMock.Setup(a => a.UploadS3File(It.IsAny<TransferUtility>(), It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()));

            var formData = FileFactory.GetEmptyMultipartFormDataContent();

            //Act
            var response = await _httpClient.PostAsync($"/api/File/AWS/UploadBaseline/1/test", formData);

            //Assert
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.Contains("File is empty or null.", responseContent);

        }

        [Fact]
        public async Task UploadBaseline_ShouldReturnInternalServerErrorResult_WhenBucketDoesNotExist()
        {
            //Arrange
            _fixture.AwsClientMock.Setup(a => a.DoesS3BucketExistAsync(It.IsAny<IAmazonS3>(), It.IsAny<string>())).ReturnsAsync(false);

            var formData = FileFactory.GetMultipartFormDataContent();

            //Act
            var response = await _httpClient.PostAsync($"/api/File/AWS/UploadBaseline/1/test", formData);

            //Assert
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.Contains("S3 bucket does not exist.", responseContent);

        }


        [Fact]
        public async Task DownloadBaseline_ShouldReturnFileResponseInstance()
        {
            //Arrange
            var baseline = BaselineFactory.GetBaseline();
            var project = ProjectFactory.GetProject();
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();

                context.Project.Add(project);
                await context.SaveChangesAsync();

                baseline.ProjectId = project.Id;
                context.Baseline.Add(baseline);
                await context.SaveChangesAsync();
            }

            _fixture.AwsClientMock.Setup(a => a.GetAmazonS3Client()).Returns(_fixture.AmazonS3ClientMock.Object);
            _fixture.AmazonS3ClientMock.Setup(a => a.GetPreSignedURL(It.IsAny<GetPreSignedUrlRequest>())).Returns("http://test.com");

            //Act
            var response = await _httpClient.GetAsync($"/api/File/AWS/DownloadBaseline/{baseline.Id}");

            //Assert
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<FileResponse>(responseContent);

            Assert.NotNull(result);
            Assert.Equal("application/zip", result.MimeType);
        }

        [Fact]
        public async Task DownloadBaseline_ShouldReturnNotFoundResult_WhenBaselineNotFound()
        {
            //Arrange

            var unexistentId = -1;

            //Act
            var response = await _httpClient.GetAsync($"/api/File/AWS/DownloadBaseline/{unexistentId}");

            //Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.Contains("not found", responseContent);

        }

        [Fact]
        public async Task RemoveBaseline_ShouldReturnOkResultMessage()
        {
            //Arrange
            var baseline = BaselineFactory.GetBaseline();
            var project = ProjectFactory.GetProject();
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();

                context.Project.Add(project);
                await context.SaveChangesAsync();

                baseline.ProjectId = project.Id;
                context.Baseline.Add(baseline);
                await context.SaveChangesAsync();
            }

            _fixture.AwsClientMock.Setup(a => a.DoesS3BucketExistAsync(It.IsAny<IAmazonS3>(), It.IsAny<string>())).ReturnsAsync(true);
            _fixture.AwsClientMock.Setup(a => a.GetAmazonS3Client()).Returns(_fixture.AmazonS3ClientMock.Object);
            _fixture.AmazonS3ClientMock.Setup(a => a.DeleteObjectAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()));

            var formData = FileFactory.GetMultipartFormDataContent();

            //Act
            var response = await _httpClient.DeleteAsync($"/api/File/AWS/RemoveBaseline/{baseline.Id}");

            //Assert
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();

            Assert.Equal("Baseline deleted successfully.", responseContent);
        }

        [Fact]
        public async Task RemoveBaseline_ShouldReturnNotFoundResult_WhenBaselineNotFound()
        {
            //Arrange

            var unexistentId = -1;

            //Act
            var response = await _httpClient.DeleteAsync($"/api/File/AWS/RemoveBaseline/{unexistentId}");

            //Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.Contains("not found", responseContent);

        }


        [Fact]
        public async Task RemoveBaseline_ShouldReturnInternalServerErrorResult_WhenS3BucketDoesNotExist()
        {
            //Arrange
            var baseline = BaselineFactory.GetBaseline();
            var project = ProjectFactory.GetProject();
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();

                context.Project.Add(project);
                await context.SaveChangesAsync();

                baseline.ProjectId = project.Id;
                context.Baseline.Add(baseline);
                await context.SaveChangesAsync();
            }

            _fixture.AwsClientMock.Setup(a => a.DoesS3BucketExistAsync(It.IsAny<IAmazonS3>(), It.IsAny<string>())).ReturnsAsync(false);

            var formData = FileFactory.GetEmptyMultipartFormDataContent();

            //Act
            var response = await _httpClient.DeleteAsync($"/api/File/AWS/RemoveBaseline/{baseline.Id}");

            //Assert
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.Contains("S3 bucket does not exist.", responseContent);

        }


        [Fact]
        public async Task DownloadLogs_ShouldReturnFileResponseInstance()
        {
            //Arrange
            var logAndArtifact = JenkinsFactory.GetLogAndArtifact();

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();
                context.LogAndArtifact.Add(logAndArtifact);

                await context.SaveChangesAsync();
            }

            _fixture.AwsClientMock.Setup(a => a.GetAmazonS3Client()).Returns(_fixture.AmazonS3ClientMock.Object);
            _fixture.AmazonS3ClientMock.Setup(a => a.GetPreSignedURL(It.IsAny<GetPreSignedUrlRequest>())).Returns("http://test.com");

            //Act
            var response = await _httpClient.GetAsync($"/api/File/AWS/DownloadLogs/{logAndArtifact.BuildId}");

            //Assert
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<FileResponse>(responseContent);

            Assert.NotNull(result);
            Assert.Equal("application/text", result.MimeType);
        }

        [Fact]
        public async Task DownloadLogs_ShouldReturnNotFoundResult_WhenLogAndArtifactNotFound()
        {
            //Arrange
            var unexistentId = -1;

            //Act
            var response = await _httpClient.GetAsync($"/api/File/AWS/DownloadLogs/{unexistentId}");

            //Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.Contains("not available", responseContent);

        }
    }
}

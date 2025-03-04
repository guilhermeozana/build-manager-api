using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Marelli.Business.Exceptions;
using Marelli.Business.IClients;
using Marelli.Business.IServices;
using Marelli.Business.Services;
using Marelli.Domain.Entities;
using Marelli.Infra.IRepositories;
using Marelli.Test.Utils.Factories;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace Marelli.Test.Services
{
    public class FileServiceTest
    {
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<IWebHostEnvironment> _hostingEnvironmentMock;
        private readonly Mock<IJenkinsRepository> _jenkinsRepositoryMock;
        private readonly Mock<IBuildTableRowRepository> _buildTableRowRepositoryMock;
        private readonly Mock<IUserService> _usuarioServiceMock;
        private readonly Mock<IProjectService> _projectServiceMock;
        private readonly Mock<IAmazonS3> _amazonS3ClientMock;
        private readonly Mock<IAwsClient> _awsClientMock;
        private readonly Mock<IBaselineService> _baselineServiceMock;

        private readonly FileService _fileService;

        public FileServiceTest()
        {
            // Criar mocks das dependências
            _configurationMock = new Mock<IConfiguration>();
            _hostingEnvironmentMock = new Mock<IWebHostEnvironment>();
            _jenkinsRepositoryMock = new Mock<IJenkinsRepository>();
            _buildTableRowRepositoryMock = new Mock<IBuildTableRowRepository>();
            _usuarioServiceMock = new Mock<IUserService>();
            _projectServiceMock = new Mock<IProjectService>();
            _amazonS3ClientMock = new Mock<IAmazonS3>();
            _awsClientMock = new Mock<IAwsClient>();
            _baselineServiceMock = new Mock<IBaselineService>();


            _configurationMock.Setup(config => config["AWS:AccessKeyId"]).Returns("fake-access-key");
            _configurationMock.Setup(config => config["AWS:SecretAccessKey"]).Returns("fake-secret-key");
            _configurationMock.Setup(config => config["AWS:Region"]).Returns("us-east-1");

            _awsClientMock.Setup(a => a.GetAmazonS3Client()).Returns(_amazonS3ClientMock.Object);

            _fileService = new FileService(
                _configurationMock.Object,
                _hostingEnvironmentMock.Object,
                _jenkinsRepositoryMock.Object,
                _buildTableRowRepositoryMock.Object,
                _usuarioServiceMock.Object,
                _projectServiceMock.Object,
                _awsClientMock.Object,
                _baselineServiceMock.Object

            );

        }

        [Fact]
        public async Task UploadZip_ShouldReturnBuildTableRowInstance()
        {
            var buildTableExpected = BuildTableRowFactory.GetBuildTable();

            _awsClientMock.Setup(a => a.DoesS3BucketExistAsync(It.IsAny<IAmazonS3>(), It.IsAny<string>())).ReturnsAsync(true);
            _awsClientMock.Setup(a => a.UploadS3File(It.IsAny<TransferUtility>(), It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()));
            _usuarioServiceMock.Setup(u => u.GetUserById(It.IsAny<int>())).ReturnsAsync(UserFactory.GetUser());
            _projectServiceMock.Setup(p => p.GetProjectById(It.IsAny<int>())).ReturnsAsync(ProjectFactory.GetProject());
            _buildTableRowRepositoryMock.Setup(b => b.GetLastUploadedByUserAsync(It.IsAny<int>())).ReturnsAsync(BuildTableRowFactory.GetBuildTable());
            _buildTableRowRepositoryMock.Setup(b => b.SaveBuildTableAsync(It.IsAny<BuildTableRow>())).ReturnsAsync(buildTableExpected);

            var res = await _fileService.UploadZip(FileFactory.GetFormFile(), 1, 1);

            Assert.NotNull(res);
            Assert.Equal(buildTableExpected, res);
            Assert.Equal(buildTableExpected.Developer, res.Developer);
        }

        [Fact]
        public async Task UploadZip_ShouldThrowFileNotFoundException()
        {
            await Assert.ThrowsAsync<FileNotFoundException>(async () => await _fileService.UploadZip(FileFactory.GetEmptyFormFile(), 1, 1));
        }

        [Fact]
        public async Task UploadZip_ShouldThrowNoSuchBucketException()
        {
            _awsClientMock.Setup(a => a.DoesS3BucketExistAsync(It.IsAny<IAmazonS3>(), It.IsAny<string>())).ReturnsAsync(false);

            await Assert.ThrowsAsync<NoSuchBucketException>(async () => await _fileService.UploadZip(FileFactory.GetFormFile(), 1, 1));
        }

        [Fact]
        public async Task UploadZip_ShouldThrowAmazonS3Exception()
        {
            _awsClientMock.Setup(a => a.DoesS3BucketExistAsync(It.IsAny<IAmazonS3>(), It.IsAny<string>())).ReturnsAsync(true);
            _usuarioServiceMock.Setup(u => u.GetUserById(It.IsAny<int>())).ReturnsAsync(UserFactory.GetUser());
            _projectServiceMock.Setup(p => p.GetProjectById(It.IsAny<int>())).ReturnsAsync(ProjectFactory.GetProject());
            _buildTableRowRepositoryMock.Setup(b => b.GetLastUploadedByUserAsync(It.IsAny<int>())).ReturnsAsync(BuildTableRowFactory.GetBuildTable());
            _buildTableRowRepositoryMock.Setup(b => b.SaveBuildTableAsync(It.IsAny<BuildTableRow>())).ReturnsAsync(BuildTableRowFactory.GetBuildTable());
            _awsClientMock.Setup(a => a.UploadS3File(It.IsAny<TransferUtility>(), It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ThrowsAsync(new AmazonS3Exception("Error Test"));

            await Assert.ThrowsAsync<AmazonS3Exception>(async () => await _fileService.UploadZip(FileFactory.GetFormFile(), 1, 1));
        }

        [Fact]
        public async Task UploadZip_ShouldThrowException_WhenUserNotFound()
        {
            _awsClientMock.Setup(a => a.DoesS3BucketExistAsync(It.IsAny<IAmazonS3>(), It.IsAny<string>())).ReturnsAsync(true);
            _usuarioServiceMock.Setup(u => u.GetUserById(It.IsAny<int>())).ThrowsAsync(new NotFoundException());
            _buildTableRowRepositoryMock.Setup(b => b.SaveBuildTableAsync(It.IsAny<BuildTableRow>())).ReturnsAsync(BuildTableRowFactory.GetBuildTable());
            _awsClientMock.Setup(a => a.UploadS3File(It.IsAny<TransferUtility>(), It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()));

            await Assert.ThrowsAsync<Exception>(async () => await _fileService.UploadZip(FileFactory.GetFormFile(), 1, 1));
        }

        [Fact]
        public async Task UploadZip_ShouldThrowException_WhenProjectNotFound()
        {
            _awsClientMock.Setup(a => a.DoesS3BucketExistAsync(It.IsAny<IAmazonS3>(), It.IsAny<string>())).ReturnsAsync(true);
            _awsClientMock.Setup(a => a.UploadS3File(It.IsAny<TransferUtility>(), It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()));
            _usuarioServiceMock.Setup(u => u.GetUserById(It.IsAny<int>())).ReturnsAsync(UserFactory.GetUser());
            _projectServiceMock.Setup(p => p.GetProjectById(It.IsAny<int>())).ThrowsAsync(new NotFoundException());

            await Assert.ThrowsAsync<Exception>(async () => await _fileService.UploadZip(FileFactory.GetFormFile(), 1, 1));
        }

        [Fact]
        public async Task DownloadZip_ShouldReturnFileResponseInstance()
        {
            var fileResponse = FileFactory.GetFileResponse();

            _jenkinsRepositoryMock.Setup(j => j.GetLogAndArtifactByBuildId(It.IsAny<int>())).ReturnsAsync(JenkinsFactory.GetLogAndArtifact());

            _projectServiceMock.Setup(p => p.GetProjectById(It.IsAny<int>())).ReturnsAsync(ProjectFactory.GetProject());
            _amazonS3ClientMock.Setup(a => a.GetPreSignedURL(It.IsAny<GetPreSignedUrlRequest>())).Returns("http://test.com");

            var res = await _fileService.DownloadZip(1);

            Assert.NotNull(res);
            Assert.Equal(fileResponse.MimeType, res.MimeType);
            Assert.Equal(fileResponse.Url, res.Url);
        }

        [Fact]
        public async Task DownloadZip_ShouldThrowNotFoundException()
        {
            _jenkinsRepositoryMock.Setup(j => j.GetLogAndArtifactByBuildId(It.IsAny<int>())).ThrowsAsync(new NotFoundException());

            await Assert.ThrowsAsync<NotFoundException>(async () => await _fileService.DownloadZip(1));
        }

        [Fact]
        public async Task RemoveZip_ShouldRemoveZipFromS3Bucket()
        {
            var buildTableExpected = BuildTableRowFactory.GetBuildTable();

            _buildTableRowRepositoryMock.Setup(b => b.GetBuildTableRowAsync(It.IsAny<int>())).ReturnsAsync(BuildTableRowFactory.GetBuildTable());
            _awsClientMock.Setup(a => a.DoesS3BucketExistAsync(It.IsAny<IAmazonS3>(), It.IsAny<string>())).ReturnsAsync(true);
            _amazonS3ClientMock.Setup(a => a.DeleteObjectAsync(It.IsAny<DeleteObjectRequest>(), It.IsAny<CancellationToken>()));
            _buildTableRowRepositoryMock.Setup(b => b.DeleteBuildTableAsync(It.IsAny<BuildTableRow>()));

            await _fileService.RemoveZip(1, 1, 1);

            _amazonS3ClientMock.Verify(a => a.DeleteObjectAsync(It.IsAny<DeleteObjectRequest>(), It.IsAny<CancellationToken>()), Times.Once);
            _buildTableRowRepositoryMock.Verify(b => b.DeleteBuildTableAsync(It.IsAny<BuildTableRow>()), Times.Once);
        }

        [Fact]
        public async Task RemoveZip_ShouldThrowNotFoundException()
        {

            _buildTableRowRepositoryMock.Setup(b => b.GetBuildTableRowAsync(It.IsAny<int>())).ThrowsAsync(new NotFoundException());

            await Assert.ThrowsAsync<NotFoundException>(async () => await _fileService.RemoveZip(1, 1, 1));
        }

        [Fact]
        public async Task RemoveZip_ShouldThrowNoSuchBucketException()
        {
            _buildTableRowRepositoryMock.Setup(b => b.GetBuildTableRowAsync(It.IsAny<int>())).ReturnsAsync(BuildTableRowFactory.GetBuildTable());
            _awsClientMock.Setup(a => a.DoesS3BucketExistAsync(It.IsAny<IAmazonS3>(), It.IsAny<string>())).ReturnsAsync(false);

            await Assert.ThrowsAsync<NoSuchBucketException>(async () => await _fileService.RemoveZip(1, 1, 1));
        }

        [Fact]
        public async Task UploadBaseline_ShouldUploadBaselineFile()
        {

            _awsClientMock.Setup(a => a.DoesS3BucketExistAsync(It.IsAny<IAmazonS3>(), It.IsAny<string>())).ReturnsAsync(true);
            _projectServiceMock.Setup(p => p.GetProjectById(It.IsAny<int>())).ReturnsAsync(ProjectFactory.GetProject());
            _baselineServiceMock.Setup(b => b.SaveBaseline(It.IsAny<Baseline>())).ReturnsAsync(BaselineFactory.GetBaseline());
            _awsClientMock.Setup(a => a.UploadS3File(It.IsAny<TransferUtility>(), It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()));

            await _fileService.UploadBaseline(FileFactory.GetFormFile(), 1, "test");

            _awsClientMock.Verify(a => a.UploadS3File(It.IsAny<TransferUtility>(), It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);

        }

        [Fact]
        public async Task UploadBaseline_ShouldThrowFileNotFoundException()
        {
            await Assert.ThrowsAsync<FileNotFoundException>(async () => await _fileService.UploadBaseline(FileFactory.GetEmptyFormFile(), 1, "test"));
        }

        [Fact]
        public async Task UploadBaseline_ShouldThrowNoSuchBucketException()
        {
            _awsClientMock.Setup(a => a.DoesS3BucketExistAsync(It.IsAny<IAmazonS3>(), It.IsAny<string>())).ReturnsAsync(false);

            await Assert.ThrowsAsync<NoSuchBucketException>(async () => await _fileService.UploadBaseline(FileFactory.GetFormFile(), 1, "test"));
        }

        [Fact]
        public async Task UploadBaseline_ShouldThrowException_WhenProjectNotFound()
        {
            _awsClientMock.Setup(a => a.DoesS3BucketExistAsync(It.IsAny<IAmazonS3>(), It.IsAny<string>())).ReturnsAsync(true);
            _projectServiceMock.Setup(p => p.GetProjectById(It.IsAny<int>())).ThrowsAsync(new NotFoundException());

            await Assert.ThrowsAsync<Exception>(async () => await _fileService.UploadBaseline(FileFactory.GetFormFile(), 1, "test"));
        }

        [Fact]
        public async Task UploadBaseline_ShouldThrowAmazonS3Exception()
        {
            _awsClientMock.Setup(a => a.DoesS3BucketExistAsync(It.IsAny<IAmazonS3>(), It.IsAny<string>())).ReturnsAsync(true);
            _projectServiceMock.Setup(p => p.GetProjectById(It.IsAny<int>())).ReturnsAsync(ProjectFactory.GetProject());
            _baselineServiceMock.Setup(b => b.SaveBaseline(It.IsAny<Baseline>())).ReturnsAsync(BaselineFactory.GetBaseline());
            _awsClientMock.Setup(a => a.UploadS3File(It.IsAny<TransferUtility>(), It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ThrowsAsync(new AmazonS3Exception("Error Test"));

            await Assert.ThrowsAsync<AmazonS3Exception>(async () => await _fileService.UploadBaseline(FileFactory.GetFormFile(), 1, "test"));
        }

        [Fact]
        public async Task DownloadBaseline_ShouldReturnFileResponseInstance()
        {
            var fileResponse = FileFactory.GetFileResponse();

            _baselineServiceMock.Setup(b => b.GetBaseline(It.IsAny<int>())).ReturnsAsync(BaselineFactory.GetBaseline());

            _projectServiceMock.Setup(p => p.GetProjectById(It.IsAny<int>())).ReturnsAsync(ProjectFactory.GetProject());
            _amazonS3ClientMock.Setup(a => a.GetPreSignedURL(It.IsAny<GetPreSignedUrlRequest>())).Returns("http://test.com");

            var res = await _fileService.DownloadBaseline(1);

            Assert.NotNull(res);
            Assert.Equal(fileResponse.MimeType, res.MimeType);
            Assert.Equal(fileResponse.Url, res.Url);
        }

        [Fact]
        public async Task DownloadBaseline_ShouldThrowNotFoundException()
        {
            _baselineServiceMock.Setup(b => b.GetBaseline(It.IsAny<int>())).ThrowsAsync(new NotFoundException());

            await Assert.ThrowsAsync<NotFoundException>(async () => await _fileService.DownloadBaseline(1));
        }

        [Fact]
        public async Task RemoveBaseline_ShouldRemoveZipFromS3Bucket()
        {
            var buildTableExpected = BuildTableRowFactory.GetBuildTable();

            _buildTableRowRepositoryMock.Setup(b => b.GetBuildTableRowAsync(It.IsAny<int>())).ReturnsAsync(BuildTableRowFactory.GetBuildTable());
            _awsClientMock.Setup(a => a.DoesS3BucketExistAsync(It.IsAny<IAmazonS3>(), It.IsAny<string>())).ReturnsAsync(true);
            _amazonS3ClientMock.Setup(a => a.DeleteObjectAsync(It.IsAny<DeleteObjectRequest>(), It.IsAny<CancellationToken>()));
            _buildTableRowRepositoryMock.Setup(b => b.DeleteBuildTableAsync(It.IsAny<BuildTableRow>()));

            await _fileService.RemoveZip(1, 1, 1);

            _amazonS3ClientMock.Verify(a => a.DeleteObjectAsync(It.IsAny<DeleteObjectRequest>(), It.IsAny<CancellationToken>()), Times.Once);
            _buildTableRowRepositoryMock.Verify(b => b.DeleteBuildTableAsync(It.IsAny<BuildTableRow>()), Times.Once);
        }

        [Fact]
        public async Task RemoveBaseline_ShouldThrowNotFoundException()
        {

            _buildTableRowRepositoryMock.Setup(b => b.GetBuildTableRowAsync(It.IsAny<int>())).ThrowsAsync(new NotFoundException());

            await Assert.ThrowsAsync<NotFoundException>(async () => await _fileService.RemoveZip(1, 1, 1));
        }

        [Fact]
        public async Task RemoveBaseline_ShouldThrowNoSuchBucketException()
        {
            _baselineServiceMock.Setup(b => b.GetBaseline(It.IsAny<int>())).ReturnsAsync(BaselineFactory.GetBaseline());
            _awsClientMock.Setup(a => a.DoesS3BucketExistAsync(It.IsAny<IAmazonS3>(), It.IsAny<string>())).ReturnsAsync(false);

            await Assert.ThrowsAsync<NoSuchBucketException>(async () => await _fileService.RemoveBaseline(1));
        }

        [Fact]
        public async Task DownloadLogs_ShouldReturnFileResponseInstance()
        {
            var fileResponse = FileFactory.GetFileResponse();

            _jenkinsRepositoryMock.Setup(j => j.GetLogAndArtifactByBuildId(It.IsAny<int>())).ReturnsAsync(JenkinsFactory.GetLogAndArtifact());
            _amazonS3ClientMock.Setup(a => a.GetPreSignedURL(It.IsAny<GetPreSignedUrlRequest>())).Returns("http://test.com");

            var res = await _fileService.DownloadLogs(1);

            Assert.NotNull(res);
            Assert.Equal(fileResponse.Url, res.Url);
        }

        [Fact]
        public async Task DownloadLogs_ShouldThrowNotFoundException()
        {
            _jenkinsRepositoryMock.Setup(j => j.GetLogAndArtifactByBuildId(It.IsAny<int>())).ThrowsAsync(new NotFoundException());

            await Assert.ThrowsAsync<NotFoundException>(async () => await _fileService.DownloadLogs(1));
        }



    }
}
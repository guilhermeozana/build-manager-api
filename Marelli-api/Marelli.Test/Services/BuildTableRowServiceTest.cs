using Marelli.Business.Exceptions;
using Marelli.Business.Factories;
using Marelli.Business.Hubs;
using Marelli.Business.IServices;
using Marelli.Business.Services;
using Marelli.Domain.Entities;
using Marelli.Infra.IRepositories;
using Marelli.Test.Utils.Factories;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using System.Net;
using Xunit;

namespace Marelli.Test.Services
{
    public class BuildTableRowServiceTest
    {
        private readonly Mock<IBuildTableRowRepository> _buildTableRowRepositoryMock;
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly Mock<IUserService> _userServiceMock;
        private readonly Mock<IProjectService> _projectServiceMock;
        private readonly Mock<IHubContext<BuildStateHub>> _hubContextMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<IJenkinsRepository> _jenkinsRepositoryMock;
        private readonly Mock<IFileService> _fileServiceMock;
        private readonly WebApplicationFactory<Program> _factory;
        private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private readonly Mock<ICustomHttpClientFactory> _httpClientFactoryMock;

        private readonly BuildTableRowService _buildTableRowService;

        public BuildTableRowServiceTest()
        {
            _buildTableRowRepositoryMock = new Mock<IBuildTableRowRepository>();
            _emailServiceMock = new Mock<IEmailService>();
            _userServiceMock = new Mock<IUserService>();
            _projectServiceMock = new Mock<IProjectService>();
            _hubContextMock = new Mock<IHubContext<BuildStateHub>>();
            _configurationMock = new Mock<IConfiguration>();
            _jenkinsRepositoryMock = new Mock<IJenkinsRepository>();
            _fileServiceMock = new Mock<IFileService>();
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            _httpClientFactoryMock = new Mock<ICustomHttpClientFactory>();

            _httpClientFactoryMock.Setup(h => h.GetHttpClient()).Returns(new HttpClient(_httpMessageHandlerMock.Object));

            _buildTableRowService = new BuildTableRowService(_buildTableRowRepositoryMock.Object, _emailServiceMock.Object, _userServiceMock.Object, _projectServiceMock.Object, _hubContextMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object, _jenkinsRepositoryMock.Object, _fileServiceMock.Object);

            _configurationMock.Setup(c => c["Jenkins:UrlHost"]).Returns("http://test.com");
            _configurationMock.Setup(c => c["Jenkins:Token"]).Returns("my-token");
        }


        [Fact]
        public async Task SaveBuildTable_ShouldReturnBuildTableRowInstance()
        {
            var buildTableExpected = BuildTableRowFactory.GetBuildTable();

            var mockClientProxy = new Mock<IClientProxy>();

            _hubContextMock.Setup(h => h.Clients.All).Returns(mockClientProxy.Object);
            _buildTableRowRepositoryMock.Setup(b => b.ListBuildTableAsync(It.IsAny<int>())).ReturnsAsync(new List<BuildTableRow>() { buildTableExpected });
            _buildTableRowRepositoryMock.Setup(b => b.SaveBuildTableAsync(It.IsAny<BuildTableRow>())).ReturnsAsync(buildTableExpected);

            var res = await _buildTableRowService.SaveBuildTable(buildTableExpected);

            Assert.NotNull(res);
            Assert.Equal(buildTableExpected, res);
        }

        [Fact]
        public async Task ListBuildTable_ShouldReturnBuildTableRowList()
        {
            var buildTableList = new List<BuildTableRow>() { BuildTableRowFactory.GetBuildTable() };

            _buildTableRowRepositoryMock.Setup(b => b.ListBuildTableAsync(It.IsAny<int>())).ReturnsAsync(buildTableList);

            var res = await _buildTableRowService.ListBuildTable(1);

            Assert.NotEmpty(res);
            Assert.Equal(buildTableList, res);
        }

        [Fact]
        public async Task ListBuildTableInProgress_ShouldReturnBuildTableRowList()
        {
            var buildTableList = new List<BuildTableRow>() { BuildTableRowFactory.GetBuildTable() };

            _buildTableRowRepositoryMock.Setup(b => b.ListBuildTableInProgressAsync(It.IsAny<int>())).ReturnsAsync(buildTableList);

            var res = await _buildTableRowService.ListBuildTableInProgress(1);

            Assert.NotEmpty(res);
            Assert.Equal(buildTableList, res);
        }

        public async Task ListBuildTableInQueue_ShouldReturnBuildTableRowList()
        {
            var buildTableList = new List<BuildTableRow>() { BuildTableRowFactory.GetBuildTable() };

            _buildTableRowRepositoryMock.Setup(b => b.ListBuildTableInQueueAsync()).ReturnsAsync(buildTableList);

            var res = await _buildTableRowService.ListBuildTableInProgress(1);

            Assert.NotEmpty(res);
            Assert.Equal(buildTableList, res);
        }

        [Fact]
        public async Task ListBuildTableByProject_ShouldReturnBuildTableRowList()
        {
            var buildTableList = new List<BuildTableRow>() { BuildTableRowFactory.GetBuildTable() };

            _buildTableRowRepositoryMock.Setup(b => b.ListBuildTableByProjectAsync(It.IsAny<int>())).ReturnsAsync(buildTableList);

            var res = await _buildTableRowService.ListBuildTableByProject(1);

            Assert.NotEmpty(res);
            Assert.Equal(buildTableList, res);
        }

        [Fact]
        public async Task GetBuildTable_ShouldReturnBuildTableRowInstance()
        {
            var buildTableExpected = BuildTableRowFactory.GetBuildTable();

            _buildTableRowRepositoryMock.Setup(b => b.GetBuildTableRowAsync(It.IsAny<int>())).ReturnsAsync(buildTableExpected);

            var res = await _buildTableRowService.GetBuildTable(1);

            Assert.NotNull(res);
            Assert.Equal(buildTableExpected, res);
        }

        [Fact]
        public async Task GetBuildTable_ShouldThrowNotFoundException()
        {
            _buildTableRowRepositoryMock.Setup(b => b.GetBuildTableRowAsync(It.IsAny<int>())).ReturnsAsync((BuildTableRow)null);

            await Assert.ThrowsAsync<NotFoundException>(async () => await _buildTableRowService.GetBuildTable(1));
        }

        [Fact]
        public async Task GetLastUploadedByUser_ShouldReturnBuildTableRowInstance()
        {
            var buildTableExpected = BuildTableRowFactory.GetBuildTable();

            _buildTableRowRepositoryMock.Setup(b => b.GetLastUploadedByUserAsync(It.IsAny<int>())).ReturnsAsync(buildTableExpected);

            var res = await _buildTableRowService.GetLastUploadedByUser(1);

            Assert.NotNull(res);
            Assert.Equal(buildTableExpected, res);
        }

        [Fact]
        public async Task GetLastUploadedByUserProjects_ShouldReturnBuildTableRowInstance()
        {
            var buildTableExpected = BuildTableRowFactory.GetBuildTable();

            _buildTableRowRepositoryMock.Setup(b => b.GetLastUploadedByUserProjectsAsync(It.IsAny<int>())).ReturnsAsync(buildTableExpected);

            var res = await _buildTableRowService.GetLastUploadedByUserProjects(1);

            Assert.NotNull(res);
            Assert.Equal(buildTableExpected, res);
        }

        [Fact]
        public async Task GetFirstInQueue_ShouldReturnBuildTableRowInstance()
        {
            var buildTableExpected = BuildTableRowFactory.GetBuildTable();

            _buildTableRowRepositoryMock.Setup(b => b.GetFirstInQueueAsync(It.IsAny<int>())).ReturnsAsync(buildTableExpected);

            var res = await _buildTableRowService.GetFirstInQueue(1);

            Assert.NotNull(res);
            Assert.Equal(buildTableExpected, res);
        }

        [Fact]
        public async Task UpdateBuildTable_ShouldReturnGreaterThanZero()
        {
            var buildTable = BuildTableRowFactory.GetBuildTable();
            var mockClientProxy = new Mock<IClientProxy>();

            _hubContextMock.Setup(h => h.Clients.All).Returns(mockClientProxy.Object);
            _buildTableRowRepositoryMock.Setup(b => b.GetBuildTableRowAsync(It.IsAny<int>())).ReturnsAsync(buildTable);
            _buildTableRowRepositoryMock.Setup(b => b.UpdateBuildTableAsync(It.IsAny<int>(), It.IsAny<BuildTableRow>(), It.IsAny<BuildTableRow>())).ReturnsAsync(1);
            _userServiceMock.Setup(u => u.GetUserById(It.IsAny<int>())).ReturnsAsync(UserFactory.GetUser());
            _projectServiceMock.Setup(p => p.GetProjectById(It.IsAny<int>())).ReturnsAsync(ProjectFactory.GetProject());
            _emailServiceMock.Setup(e => e.SendEmail(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));

            _buildTableRowRepositoryMock.Setup(b => b.ListBuildTableAsync(It.IsAny<int>())).ReturnsAsync(new List<BuildTableRow>() { buildTable });

            var buildTableStatusFinished = BuildTableRowFactory.GetBuildTable();
            buildTableStatusFinished.Status = "Finished";

            var res = await _buildTableRowService.UpdateBuildTable(1, buildTableStatusFinished);

            Assert.Equal(1, res);
        }

        [Fact]
        public async Task UpdateBuildTable_ShouldThrowNotFoundException()
        {
            _buildTableRowRepositoryMock.Setup(b => b.GetBuildTableRowAsync(It.IsAny<int>())).ReturnsAsync((BuildTableRow)null);

            await Assert.ThrowsAsync<NotFoundException>(async () => await _buildTableRowService.UpdateBuildTable(1, BuildTableRowFactory.GetBuildTable()));
        }

        [Fact]
        public async Task DeleteBuildTableInstance_ShouldReturnGreaterThanZero()
        {
            _buildTableRowRepositoryMock.Setup(b => b.GetBuildTableRowAsync(It.IsAny<int>())).ReturnsAsync(BuildTableRowFactory.GetBuildTable());
            _buildTableRowRepositoryMock.Setup(b => b.DeleteBuildTableAsync(It.IsAny<BuildTableRow>())).ReturnsAsync(1);

            var res = await _buildTableRowService.DeleteBuildTable(BuildTableRowFactory.GetBuildTable());

            Assert.Equal(1, res);
        }

        [Fact]
        public async Task DeleteBuildTable_ShouldReturnGreaterThanZero()
        {

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Post &&
                        req.RequestUri.ToString().Contains("/doDelete")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK
                });

            var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
            _httpClientFactoryMock.Setup(f => f.GetHttpClient()).Returns(httpClient);

            var buildTable = BuildTableRowFactory.GetBuildTable();

            _buildTableRowRepositoryMock.Setup(b => b.GetBuildTableRowAsync(It.IsAny<int>()))
                .ReturnsAsync(buildTable);

            _buildTableRowRepositoryMock.Setup(b => b.DeleteBuildTableAsync(It.IsAny<BuildTableRow>()))
                .ReturnsAsync(1);

            var result = await _buildTableRowService.DeleteBuildTable(1);

            Assert.Equal(1, result);

            _httpMessageHandlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri.ToString().Contains("/doDelete")),
                ItExpr.IsAny<CancellationToken>()
            );
        }


        [Fact]
        public async Task DeleteBuildTable_ShouldThrowNotFoundException()
        {
            _buildTableRowRepositoryMock.Setup(b => b.GetBuildTableRowAsync(It.IsAny<int>())).ReturnsAsync((BuildTableRow)null);

            await Assert.ThrowsAsync<NotFoundException>(async () => await _buildTableRowService.DeleteBuildTable(1));
        }

    }
}
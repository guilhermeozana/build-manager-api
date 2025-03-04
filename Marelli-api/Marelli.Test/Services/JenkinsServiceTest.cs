using Marelli.Business.Exceptions;
using Marelli.Business.Factories;
using Marelli.Business.Hubs;
using Marelli.Business.IServices;
using Marelli.Business.Services;
using Marelli.Domain.Entities;
using Marelli.Infra.IRepositories;
using Marelli.Test.Utils.Factories;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Text;
using Xunit;

namespace Marelli.Test.Services
{
    public class JenkinsServiceTest
    {
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<IJenkinsRepository> _jenkinsRepositoryMock;
        private readonly Mock<IHubContext<BuildStateHub>> _hubContextMock;
        private readonly Mock<IUserService> _userServiceMock;
        private readonly Mock<IProjectService> _projectServiceMock;
        private readonly Mock<IBuildTableRowService> _buildTableRowServiceMock;
        private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private readonly Mock<ICustomHttpClientFactory> _httpClientFactoryMock;
        private readonly Mock<IClientProxy> _clientProxyMock;
        private readonly Mock<IBaselineService> _baselineServiceMock;
        private readonly Mock<IFileService> _fileServiceMock;
        private readonly Mock<IBuildLogService> _buildLogServiceMock;

        private readonly JenkinsService _jenkinsService;

        public JenkinsServiceTest()
        {
            _configurationMock = new Mock<IConfiguration>();
            _jenkinsRepositoryMock = new Mock<IJenkinsRepository>();
            _hubContextMock = new Mock<IHubContext<BuildStateHub>>();
            _userServiceMock = new Mock<IUserService>();
            _projectServiceMock = new Mock<IProjectService>();
            _buildTableRowServiceMock = new Mock<IBuildTableRowService>();
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            _httpClientFactoryMock = new Mock<ICustomHttpClientFactory>();
            _clientProxyMock = new Mock<IClientProxy>();
            _baselineServiceMock = new Mock<IBaselineService>();
            _fileServiceMock = new Mock<IFileService>();
            _buildLogServiceMock = new Mock<IBuildLogService>();

            _httpClientFactoryMock.Setup(h => h.GetHttpClient()).Returns(new HttpClient(_httpMessageHandlerMock.Object));

            _jenkinsService = new JenkinsService(
                _configurationMock.Object,
                _jenkinsRepositoryMock.Object,
                _hubContextMock.Object,
                _userServiceMock.Object,
                _projectServiceMock.Object,
                _buildTableRowServiceMock.Object,
                _baselineServiceMock.Object,
                _httpClientFactoryMock.Object,
                _fileServiceMock.Object,
                _buildLogServiceMock.Object
            );

            _configurationMock.Setup(c => c["Jenkins:UrlHost"]).Returns("http://test.com");
            _configurationMock.Setup(c => c["Jenkins:Token"]).Returns("my-token");
        }

        [Fact]
        public async Task Invoke_ShouldReturnBuildingStateInstance()
        {
            //Simulates All Http Messages

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK
                });

            // Simulates inQueue request

            var inQueueJsonResponse = new JObject
            {
                { "inQueue", false }
            }.ToString();

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri == new Uri($"http://test.com/job/create_jobs/api/json")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(inQueueJsonResponse)
                });


            // Simulates lastBuildJob request

            var lastBuildJsonResponse = new JObject
            {
                { "inProgress", false },
                { "result", "SUCCESS" }
            }.ToString();

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri == new Uri($"http://test.com/job/create_jobs/lastBuild/api/json")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(lastBuildJsonResponse)
                });

            var expectedBuildingState = JenkinsFactory.GetBuildingState();

            _hubContextMock.Setup(h => h.Clients.All).Returns(_clientProxyMock.Object);
            _buildTableRowServiceMock.Setup(b => b.GetBuildTable(It.IsAny<int>())).ReturnsAsync(BuildTableRowFactory.GetBuildTable());
            _buildTableRowServiceMock.Setup(b => b.ListBuildTableByProject(It.IsAny<int>())).ReturnsAsync(new List<BuildTableRow>() { BuildTableRowFactory.GetBuildTable() });
            _buildTableRowServiceMock.Setup(b => b.ListBuildTableInProgress(It.IsAny<int>())).ReturnsAsync(new List<BuildTableRow>() { });
            _baselineServiceMock.Setup(b => b.GetBaselineByProject(It.IsAny<int>())).ReturnsAsync(BaselineFactory.GetBaseline());
            _jenkinsRepositoryMock.Setup(j => j.GetBuildingStateByBuildId(It.IsAny<int>())).ReturnsAsync(expectedBuildingState);
            _jenkinsRepositoryMock.Setup(j => j.UpdateBuildingState(It.IsAny<int>(), It.IsAny<BuildingState>(), It.IsAny<BuildingState>())).ReturnsAsync(expectedBuildingState);

            _jenkinsRepositoryMock.Setup(j => j.SaveBuildingState(It.IsAny<BuildingState>()))
                .ReturnsAsync(expectedBuildingState);

            var result = await _jenkinsService.Invoke(1, 1, 1, true, false);

            Assert.NotNull(result);
            Assert.Equal(expectedBuildingState.Date, result.Date);
        }

        [Fact]
        public async Task Invoke_ShouldReturnBuildingStateInstance_WhenRebuild()
        {
            //Simulates All Http Messages

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK
                });

            // Simulates inQueue request

            var inQueueJsonResponse = new JObject
            {
                { "inQueue", false }
            }.ToString();

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri == new Uri($"http://test.com/job/create_jobs/api/json")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(inQueueJsonResponse)
                });


            // Simulates lastBuildJob request

            var lastBuildJsonResponse = new JObject
            {
                { "inProgress", false },
                { "result", "SUCCESS" }
            }.ToString();

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri == new Uri($"http://test.com/job/create_jobs/lastBuild/api/json")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(lastBuildJsonResponse)
                });

            var expectedBuildingState = JenkinsFactory.GetBuildingState();

            _hubContextMock.Setup(h => h.Clients.All).Returns(_clientProxyMock.Object);
            _buildTableRowServiceMock.Setup(b => b.GetBuildTable(It.IsAny<int>())).ReturnsAsync(BuildTableRowFactory.GetBuildTable());
            _buildTableRowServiceMock.Setup(b => b.ListBuildTableByProject(It.IsAny<int>())).ReturnsAsync(new List<BuildTableRow>() { BuildTableRowFactory.GetBuildTable() });
            _buildTableRowServiceMock.Setup(b => b.ListBuildTableInProgress(It.IsAny<int>())).ReturnsAsync(new List<BuildTableRow>() { });
            _baselineServiceMock.Setup(b => b.GetBaselineByProject(It.IsAny<int>())).ReturnsAsync(BaselineFactory.GetBaseline());
            _jenkinsRepositoryMock.Setup(j => j.GetBuildingStateByBuildId(It.IsAny<int>())).ReturnsAsync(expectedBuildingState);
            _jenkinsRepositoryMock.Setup(j => j.GetBuildingStateById(It.IsAny<int>())).ReturnsAsync(expectedBuildingState);
            _jenkinsRepositoryMock.Setup(j => j.UpdateBuildingState(It.IsAny<int>(), It.IsAny<BuildingState>(), It.IsAny<BuildingState>())).ReturnsAsync(expectedBuildingState);

            _jenkinsRepositoryMock.Setup(j => j.SaveBuildingState(It.IsAny<BuildingState>()))
                .ReturnsAsync(expectedBuildingState);

            var result = await _jenkinsService.Invoke(1, 1, 1, true, true);

            Assert.NotNull(result);
            Assert.Equal(expectedBuildingState.Date, result.Date);
        }

        [Fact]
        public async Task Invoke_ShouldThrowException_WhenBuildTableNotFound()
        {
            _buildTableRowServiceMock.Setup(b => b.GetBuildTable(It.IsAny<int>())).ThrowsAsync(new NotFoundException());

            await Assert.ThrowsAsync<Exception>(async () => await _jenkinsService.Invoke(1, 1, 1, true, false));
        }

        [Fact]
        public async Task Invoke_ShouldThrowInvalidOperationException_WhenBuildIsAlreadyInProgress()
        {
            var buildTable = BuildTableRowFactory.GetBuildTable();
            buildTable.Status = "Compiling";

            _buildTableRowServiceMock.Setup(b => b.GetBuildTable(It.IsAny<int>())).ReturnsAsync(buildTable);

            await Assert.ThrowsAsync<InvalidOperationException>(async () => await _jenkinsService.Invoke(1, 1, 1, true, false));
        }

        [Fact]
        public async Task Invoke_ShouldThrowInvalidOperationException_WhenBuildIsAlreadyInQueue()
        {
            var buildTable = BuildTableRowFactory.GetBuildTable();
            buildTable.Status = "In Queue";

            _buildTableRowServiceMock.Setup(b => b.GetBuildTable(It.IsAny<int>())).ReturnsAsync(buildTable);

            await Assert.ThrowsAsync<InvalidOperationException>(async () => await _jenkinsService.Invoke(1, 1, 1, true, false));
        }

        [Fact]
        public async Task Invoke_ShouldThrowInvalidOperationException_WhenThereIsNoBaseline()
        {
            var buildTable = BuildTableRowFactory.GetBuildTable();

            _buildTableRowServiceMock.Setup(b => b.GetBuildTable(It.IsAny<int>())).ReturnsAsync(buildTable);
            _buildTableRowServiceMock.Setup(b => b.ListBuildTableByProject(It.IsAny<int>())).ReturnsAsync(new List<BuildTableRow>() { });
            _baselineServiceMock.Setup(b => b.GetBaselineByProject(It.IsAny<int>())).ReturnsAsync((Baseline)null);

            await Assert.ThrowsAsync<InvalidOperationException>(async () => await _jenkinsService.Invoke(1, 1, 1, true, false));
        }

        [Fact]
        public async Task Invoke_ShouldThrowInvalidOperationException_WhenBuildHasAlreadyBeenStarted()
        {
            var buildTable = BuildTableRowFactory.GetBuildTable();
            buildTable.Status = "Starting";

            _buildTableRowServiceMock.Setup(b => b.GetBuildTable(It.IsAny<int>())).ReturnsAsync(buildTable);

            await Assert.ThrowsAsync<InvalidOperationException>(async () => await _jenkinsService.Invoke(1, 1, 1, true, false));
        }

        [Fact]
        public async Task Invoke_ShouldThrowException_WhenInvokingJobCreationFails()
        {
            //Simulates All Http Messages

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadGateway
                });

            _buildTableRowServiceMock.Setup(b => b.GetBuildTable(It.IsAny<int>())).ReturnsAsync(BuildTableRowFactory.GetBuildTable());
            _baselineServiceMock.Setup(b => b.GetBaselineByProject(It.IsAny<int>())).ReturnsAsync(BaselineFactory.GetBaseline());
            _buildTableRowServiceMock.Setup(b => b.ListBuildTableInProgress(It.IsAny<int>())).ReturnsAsync(new List<BuildTableRow>() { });

            await Assert.ThrowsAsync<Exception>(async () => await _jenkinsService.Invoke(1, 1, 1, true, false));
        }

        [Fact]
        public async Task Invoke_ShouldThrowException_WhenJobCreationFails()
        {
            //Simulates All Http Messages

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK
                });

            // Simulates inQueue request

            var inQueueJsonResponse = new JObject
            {
                { "inQueue", false }
            }.ToString();

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri == new Uri($"http://test.com/job/create_jobs/api/json")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(inQueueJsonResponse)
                });


            // Simulates lastBuildJob request with failure

            var lastBuildJsonResponse = new JObject
            {
                { "inProgress", false },
                { "result", "FAILURE" }
            }.ToString();

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri == new Uri($"http://test.com/job/create_jobs/lastBuild/api/json")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(lastBuildJsonResponse)
                });

            _buildTableRowServiceMock.Setup(b => b.GetBuildTable(It.IsAny<int>())).ReturnsAsync(BuildTableRowFactory.GetBuildTable());
            _baselineServiceMock.Setup(b => b.GetBaselineByProject(It.IsAny<int>())).ReturnsAsync(BaselineFactory.GetBaseline());
            _buildTableRowServiceMock.Setup(b => b.ListBuildTableInProgress(It.IsAny<int>())).ReturnsAsync(new List<BuildTableRow>() { });

            await Assert.ThrowsAsync<Exception>(async () => await _jenkinsService.Invoke(1, 1, 1, true, false));
        }

        [Fact]
        public async Task Invoke_ShouldThrowException_WhenStartingBuildFails()
        {
            var buildTable = BuildTableRowFactory.GetBuildTable();
            var fileName = $"1_1_{buildTable.Date.ToString("yyyyMMddHHmmssffff")}";

            //Simulates All Http Messages

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK
                });

            // Simulates inQueue request

            var inQueueJsonResponse = new JObject
            {
                { "inQueue", false }
            }.ToString();

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri == new Uri($"http://test.com/job/create_jobs/api/json")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(inQueueJsonResponse)
                });


            // Simulates lastBuildJob request

            var lastBuildJsonResponse = new JObject
            {
                { "inProgress", false },
                { "result", "SUCCESS" }
            }.ToString();

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri == new Uri($"http://test.com/job/create_jobs/lastBuild/api/json")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(lastBuildJsonResponse)
                });

            // Simulates Start Build request with Failure

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Post &&
                        req.RequestUri == new Uri($"http://test.com/job/{fileName}/buildWithParameters")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Content = new StringContent(inQueueJsonResponse)
                });

            _buildTableRowServiceMock.Setup(b => b.GetBuildTable(It.IsAny<int>())).ReturnsAsync(buildTable);
            _baselineServiceMock.Setup(b => b.GetBaselineByProject(It.IsAny<int>())).ReturnsAsync(BaselineFactory.GetBaseline());
            _buildTableRowServiceMock.Setup(b => b.ListBuildTableInProgress(It.IsAny<int>())).ReturnsAsync(new List<BuildTableRow>() { });

            await Assert.ThrowsAsync<Exception>(async () => await _jenkinsService.Invoke(1, 1, 1, true, false));
        }

        [Fact]
        public async Task StopBuild_ShouldStopBuildInProgress()
        {
            var buildTable = BuildTableRowFactory.GetBuildTable();
            buildTable.Status = "Compiling";

            var buildJobName = $"{buildTable.ProjectId}_{buildTable.UserId}_{buildTable.TagName}";

            //Simulates All Http Messages

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK
                });

            // Simulates get job by name request with sequential responses

            var responseQueue = new Queue<HttpResponseMessage>();

            responseQueue.Enqueue(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{ \"status\": \"job exists\" }", Encoding.UTF8, "application/json")
            });

            responseQueue.Enqueue(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound,
                Content = new StringContent("{ \"error\": \"job not found\" }", Encoding.UTF8, "application/json")
            });

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri == new Uri($"http://test.com/job/{buildJobName}/api/json")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(() => responseQueue.Dequeue());


            //Simulates delete job request

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Post &&
                        req.RequestUri == new Uri($"http://test.com/job/job/{buildJobName}/doDelete")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK
                });


            _hubContextMock.Setup(h => h.Clients.All).Returns(_clientProxyMock.Object);
            _buildTableRowServiceMock.Setup(b => b.GetBuildTable(It.IsAny<int>())).ReturnsAsync(buildTable);
            _buildTableRowServiceMock.Setup(b => b.UpdateBuildTable(It.IsAny<int>(), It.IsAny<BuildTableRow>())).ReturnsAsync(1);
            _jenkinsRepositoryMock.Setup(j => j.GetBuildingStateByBuildId(It.IsAny<int>())).ReturnsAsync(JenkinsFactory.GetBuildingState());
            _jenkinsRepositoryMock.Setup(j => j.GetBuildingStateById(It.IsAny<int>())).ReturnsAsync(JenkinsFactory.GetBuildingState());
            _jenkinsRepositoryMock.Setup(j => j.GetLogAndArtifactByBuildId(It.IsAny<int>())).ReturnsAsync(JenkinsFactory.GetLogAndArtifact());
            _jenkinsRepositoryMock.Setup(j => j.GetLogAndArtifactById(It.IsAny<int>())).ReturnsAsync(JenkinsFactory.GetLogAndArtifact());

            await _jenkinsService.StopBuild(1);
        }

        [Fact]
        public async Task StopBuild_ShouldThrowNotFoundException_WhenBuildNotFound()
        {
            _buildTableRowServiceMock.Setup(b => b.GetBuildTable(It.IsAny<int>())).ThrowsAsync(new NotFoundException());

            await Assert.ThrowsAsync<NotFoundException>(async () => await _jenkinsService.StopBuild(1));

        }

        [Fact]
        public async Task StopBuild_ShouldThrowInvalidOperationException_WhenBuildIsNotInProgress()
        {
            var buildTable = BuildTableRowFactory.GetBuildTable();

            _buildTableRowServiceMock.Setup(b => b.GetBuildTable(It.IsAny<int>())).ReturnsAsync(buildTable);

            await Assert.ThrowsAsync<InvalidOperationException>(async () => await _jenkinsService.StopBuild(1));

        }

        [Fact]
        public async Task StopBuild_ShouldThrowException_WhenJenkinsJobNotFound()
        {
            var buildTable = BuildTableRowFactory.GetBuildTable();
            buildTable.Status = "Compiling";

            var buildJobName = $"{buildTable.ProjectId}_{buildTable.UserId}_{buildTable.TagName}";

            // Simulates get job by name request with sequential responses

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri == new Uri($"http://test.com/job/{buildJobName}/api/json")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.NotFound
                });

            _buildTableRowServiceMock.Setup(b => b.GetBuildTable(It.IsAny<int>())).ReturnsAsync(buildTable);

            await Assert.ThrowsAsync<Exception>(async () => await _jenkinsService.StopBuild(1));

        }

        [Fact]
        public async Task GetAllData_ShouldReturnAllJenkinsData()
        {
            var jenkinsAllData = JenkinsFactory.GetJenkinsAllDataResponse();

            _jenkinsRepositoryMock.Setup(j => j.GetAllData(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(jenkinsAllData);

            var res = await _jenkinsService.GetAllData(1, 1);

            Assert.NotNull(res);
            Assert.Equal(jenkinsAllData, res);
        }

        //Building State

        [Fact]
        public async Task SaveBuildingState_ShouldReturnBuildingStateInstance()
        {
            var buildingStateExpected = JenkinsFactory.GetBuildingState();

            _hubContextMock.Setup(h => h.Clients.All).Returns(_clientProxyMock.Object);

            _jenkinsRepositoryMock.Setup(j => j.SaveBuildingState(It.IsAny<BuildingState>()))
                .ReturnsAsync(buildingStateExpected);

            var result = await _jenkinsService.SaveBuildingState(buildingStateExpected);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task ListBuildingState_ShouldReturnBuildingStateList()
        {
            var buildingStateList = new List<BuildingState>() { JenkinsFactory.GetBuildingState() };

            _jenkinsRepositoryMock.Setup(j => j.ListBuildingState(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(buildingStateList);

            var res = await _jenkinsService.ListBuildingState(1, 1);

            Assert.NotEmpty(res);
            Assert.Equal(buildingStateList, res);
        }

        [Fact]
        public async Task GetBuildingStateById_ShouldReturnBuildingStateInstance()
        {
            var expectedBuildingState = JenkinsFactory.GetBuildingState();

            _jenkinsRepositoryMock.Setup(j => j.GetBuildingStateById(It.IsAny<int>())).ReturnsAsync(expectedBuildingState);

            var res = await _jenkinsService.GetBuildingStateById(1);

            Assert.NotNull(res);
            Assert.Equal(expectedBuildingState.Date, res.Date);
        }

        [Fact]
        public async Task GetBuildingStateById_ShouldThrowNotFoundException()
        {
            _jenkinsRepositoryMock.Setup(j => j.GetBuildingStateById(It.IsAny<int>())).ReturnsAsync((BuildingState)null);

            await Assert.ThrowsAsync<NotFoundException>(async () => await _jenkinsService.GetBuildingStateById(1));
        }

        [Fact]
        public async Task GetBuildingStateByBuildId_ShouldReturnBuildingStateInstance()
        {
            var expectedBuildingState = JenkinsFactory.GetBuildingState();

            _jenkinsRepositoryMock.Setup(j => j.GetBuildingStateByBuildId(It.IsAny<int>())).ReturnsAsync(expectedBuildingState);

            var res = await _jenkinsService.GetBuildingStateByBuildId(1);

            Assert.NotNull(res);
            Assert.Equal(expectedBuildingState.Date, res.Date);
        }

        [Fact]
        public async Task UpdateBuildingState_ShouldReturnGreaterThanZero()
        {
            var expectedBuildingState = JenkinsFactory.GetBuildingState();

            _hubContextMock.Setup(h => h.Clients.All).Returns(_clientProxyMock.Object);

            _jenkinsRepositoryMock.Setup(j => j.GetBuildingStateById(It.IsAny<int>())).ReturnsAsync(expectedBuildingState);
            _jenkinsRepositoryMock.Setup(j => j.UpdateBuildingState(It.IsAny<int>(), It.IsAny<BuildingState>(), It.IsAny<BuildingState>())).ReturnsAsync(expectedBuildingState);

            var res = await _jenkinsService.UpdateBuildingState(1, expectedBuildingState);

            Assert.NotNull(res);
            Assert.Equal(expectedBuildingState.Date, res.Date);
        }

        [Fact]
        public async Task UpdateBuildingState_ShouldThrowNotFoundException()
        {

            _jenkinsRepositoryMock.Setup(j => j.GetBuildingStateById(It.IsAny<int>())).ReturnsAsync((BuildingState)null);

            await Assert.ThrowsAsync<NotFoundException>(async () => await _jenkinsService.UpdateBuildingState(1, JenkinsFactory.GetBuildingState()));
        }

        [Fact]
        public async Task DeleteBuildingState_ShouldReturnGreaterThanZero()
        {
            _jenkinsRepositoryMock.Setup(j => j.GetBuildingStateById(It.IsAny<int>())).ReturnsAsync(JenkinsFactory.GetBuildingState());
            _jenkinsRepositoryMock.Setup(j => j.DeleteBuildingState(It.IsAny<BuildingState>())).ReturnsAsync(1);

            var res = await _jenkinsService.DeleteBuildingState(1);

            Assert.Equal(1, res);
        }

        [Fact]
        public async Task DeleteBuildingState_ShouldThrowNotFoundException()
        {
            _jenkinsRepositoryMock.Setup(j => j.GetBuildingStateById(It.IsAny<int>())).ReturnsAsync((BuildingState)null);

            await Assert.ThrowsAsync<NotFoundException>(async () => await _jenkinsService.DeleteBuildingState(1));
        }

        //Log And Artifact

        [Fact]
        public async Task SaveLogAndArtifact_ShouldReturnLogAndArtifactInstance()
        {
            var logAndArtifactExpected = JenkinsFactory.GetLogAndArtifact();

            _jenkinsRepositoryMock.Setup(j => j.SaveLogAndArtifact(It.IsAny<LogAndArtifact>()))
                .ReturnsAsync(1);

            var result = await _jenkinsService.SaveLogAndArtifact(logAndArtifactExpected);

            Assert.Equal(1, result);
        }

        [Fact]
        public async Task ListLogAndArtifact_ShouldReturnLogAndArtifactList()
        {
            var logAndArtifactList = new List<LogAndArtifact>() { JenkinsFactory.GetLogAndArtifact() };

            _jenkinsRepositoryMock.Setup(j => j.ListLogAndArtifact(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(logAndArtifactList);

            var res = await _jenkinsService.ListLogAndArtifact(1, 1);

            Assert.NotEmpty(res);
            Assert.Equal(logAndArtifactList, res);
        }

        [Fact]
        public async Task GetLogAndArtifactById_ShouldReturnLogAndArtifactInstance()
        {
            var expectedLogAndArtifact = JenkinsFactory.GetLogAndArtifact();

            _jenkinsRepositoryMock.Setup(j => j.GetLogAndArtifactById(It.IsAny<int>())).ReturnsAsync(expectedLogAndArtifact);

            var res = await _jenkinsService.GetLogAndArtifactById(1);

            Assert.NotNull(res);
            Assert.Equal(expectedLogAndArtifact.FileOutS3BucketLocation, res.FileOutS3BucketLocation);
            Assert.Equal(expectedLogAndArtifact.FileLogS3BucketLocation, res.FileLogS3BucketLocation);
        }

        [Fact]
        public async Task GetLogAndArtifactById_ShouldThrowNotFoundException()
        {
            _jenkinsRepositoryMock.Setup(j => j.GetLogAndArtifactById(It.IsAny<int>())).ReturnsAsync((LogAndArtifact)null);

            await Assert.ThrowsAsync<NotFoundException>(async () => await _jenkinsService.GetLogAndArtifactById(1));
        }

        [Fact]
        public async Task GetLogAndArtifactByBuildId_ShouldReturnLogAndArtifactInstance()
        {
            var expectedLogAndArtifact = JenkinsFactory.GetLogAndArtifact();

            _jenkinsRepositoryMock.Setup(j => j.GetLogAndArtifactByBuildId(It.IsAny<int>())).ReturnsAsync(expectedLogAndArtifact);

            var res = await _jenkinsService.GetLogAndArtifactByBuildId(1);

            Assert.NotNull(res);
            Assert.Equal(expectedLogAndArtifact.FileOutS3BucketLocation, res.FileOutS3BucketLocation);
            Assert.Equal(expectedLogAndArtifact.FileLogS3BucketLocation, res.FileLogS3BucketLocation);
        }

        [Fact]
        public async Task UpdateLogAndArtifact_ShouldReturnGreaterThanZero()
        {
            var expectedLogAndArtifact = JenkinsFactory.GetLogAndArtifact();

            _jenkinsRepositoryMock.Setup(j => j.GetLogAndArtifactById(It.IsAny<int>())).ReturnsAsync(expectedLogAndArtifact);
            _jenkinsRepositoryMock.Setup(j => j.UpdateLogAndArtifact(It.IsAny<int>(), It.IsAny<LogAndArtifact>(), It.IsAny<LogAndArtifact>())).ReturnsAsync(1);

            var res = await _jenkinsService.UpdateLogAndArtifact(1, expectedLogAndArtifact);

            Assert.Equal(1, res);
        }

        [Fact]
        public async Task UpdateLogAndArtifact_ShouldThrowNotFoundException()
        {

            _jenkinsRepositoryMock.Setup(j => j.GetLogAndArtifactById(It.IsAny<int>())).ReturnsAsync((LogAndArtifact)null);

            await Assert.ThrowsAsync<NotFoundException>(async () => await _jenkinsService.UpdateLogAndArtifact(1, JenkinsFactory.GetLogAndArtifact()));
        }

        [Fact]
        public async Task DeleteLogAndArtifact_ShouldReturnGreaterThanZero()
        {
            _jenkinsRepositoryMock.Setup(j => j.GetLogAndArtifactById(It.IsAny<int>())).ReturnsAsync(JenkinsFactory.GetLogAndArtifact());
            _jenkinsRepositoryMock.Setup(j => j.DeleteLogAndArtifact(It.IsAny<LogAndArtifact>())).ReturnsAsync(1);

            var res = await _jenkinsService.DeleteLogAndArtifact(1);

            Assert.Equal(1, res);
        }

        [Fact]
        public async Task DeleteLogAndArtifact_ShouldThrowNotFoundException()
        {
            _jenkinsRepositoryMock.Setup(j => j.GetLogAndArtifactById(It.IsAny<int>())).ReturnsAsync((LogAndArtifact)null);

            await Assert.ThrowsAsync<NotFoundException>(async () => await _jenkinsService.DeleteLogAndArtifact(1));
        }

        //File Verify

        [Fact]
        public async Task SaveFileVerify_ShouldReturnFileVerifyInstance()
        {
            var fileVerifyExpected = JenkinsFactory.GetFileVerify();

            _jenkinsRepositoryMock.Setup(j => j.SaveFileVerify(It.IsAny<FileVerify>()))
                .ReturnsAsync(1);

            var result = await _jenkinsService.SaveFileVerify(fileVerifyExpected);

            Assert.Equal(1, result);
        }

        [Fact]
        public async Task ListFileVerify_ShouldReturnFileVerifyList()
        {
            var fileVerifyList = new List<FileVerify>() { JenkinsFactory.GetFileVerify() };

            _jenkinsRepositoryMock.Setup(j => j.ListFileVerify(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(fileVerifyList);

            var res = await _jenkinsService.ListFileVerify(1, 1);

            Assert.NotEmpty(res);
            Assert.Equal(fileVerifyList, res);
        }

        [Fact]
        public async Task GetFileVerifyById_ShouldReturnFileVerifyInstance()
        {
            var expectedFileVerify = JenkinsFactory.GetFileVerify();

            _jenkinsRepositoryMock.Setup(j => j.GetFileVerifyById(It.IsAny<int>())).ReturnsAsync(expectedFileVerify);

            var res = await _jenkinsService.GetFileVerifyById(1);

            Assert.NotNull(res);
            Assert.Equal(expectedFileVerify.Filename, res.Filename);
            Assert.Equal(expectedFileVerify.FileZipS3BucketLocation, res.FileZipS3BucketLocation);
        }

        [Fact]
        public async Task GetFileVerifyById_ShouldThrowNotFoundException()
        {
            _jenkinsRepositoryMock.Setup(j => j.GetFileVerifyById(It.IsAny<int>())).ReturnsAsync((FileVerify)null);

            await Assert.ThrowsAsync<NotFoundException>(async () => await _jenkinsService.GetFileVerifyById(1));
        }

        [Fact]
        public async Task GetFileVerifyByBuildId_ShouldReturnFileVerifyInstance()
        {
            var expectedFileVerify = JenkinsFactory.GetFileVerify();

            _jenkinsRepositoryMock.Setup(j => j.GetFileVerifyByBuildId(It.IsAny<int>())).ReturnsAsync(expectedFileVerify);

            var res = await _jenkinsService.GetFileVerifyByBuildId(1);

            Assert.NotNull(res);
            Assert.Equal(expectedFileVerify.Filename, res.Filename);
        }

        [Fact]
        public async Task UpdateFileVerify_ShouldReturnGreaterThanZero()
        {
            var expectedFileVerify = JenkinsFactory.GetFileVerify();

            _jenkinsRepositoryMock.Setup(j => j.GetFileVerifyById(It.IsAny<int>())).ReturnsAsync(expectedFileVerify);
            _jenkinsRepositoryMock.Setup(j => j.UpdateFileVerify(It.IsAny<int>(), It.IsAny<FileVerify>(), It.IsAny<FileVerify>())).ReturnsAsync(1);

            var res = await _jenkinsService.UpdateFileVerify(1, expectedFileVerify);

            Assert.Equal(1, res);
        }

        [Fact]
        public async Task UpdateFileVerify_ShouldThrowNotFoundException()
        {

            _jenkinsRepositoryMock.Setup(j => j.GetFileVerifyById(It.IsAny<int>())).ReturnsAsync((FileVerify)null);

            await Assert.ThrowsAsync<NotFoundException>(async () => await _jenkinsService.UpdateFileVerify(1, JenkinsFactory.GetFileVerify()));
        }

        [Fact]
        public async Task DeleteFileVerify_ShouldReturnGreaterThanZero()
        {
            _jenkinsRepositoryMock.Setup(j => j.GetFileVerifyById(It.IsAny<int>())).ReturnsAsync(JenkinsFactory.GetFileVerify());
            _jenkinsRepositoryMock.Setup(j => j.DeleteFileVerify(It.IsAny<FileVerify>())).ReturnsAsync(1);

            var res = await _jenkinsService.DeleteFileVerify(1);

            Assert.Equal(1, res);
        }

        [Fact]
        public async Task DeleteFileVerify_ShouldThrowNotFoundException()
        {
            _jenkinsRepositoryMock.Setup(j => j.GetFileVerifyById(It.IsAny<int>())).ReturnsAsync((FileVerify)null);

            await Assert.ThrowsAsync<NotFoundException>(async () => await _jenkinsService.DeleteFileVerify(1));
        }
    }
}

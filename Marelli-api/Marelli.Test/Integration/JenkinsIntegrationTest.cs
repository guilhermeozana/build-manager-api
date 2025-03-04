using Marelli.Business.Factories;
using Marelli.Business.Hubs;
using Marelli.Domain.Dtos;
using Marelli.Domain.Entities;
using Marelli.Infra.Context;
using Marelli.Test.Integration.Configuration;
using Marelli.Test.Utils.Factories;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using Testcontainers.PostgreSql;
using Xunit;


namespace Marelli.Test.Integration
{
    [Collection("Integration collection")]
    public class JenkinsIntegrationTests
    {
        private readonly IntegrationSetupFixture _fixture;
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _httpClient;
        private readonly PostgreSqlContainer _postgreSqlContainer;
        private readonly Mock<IHubContext<BuildStateHub>> _hubContextMock;
        private readonly Mock<IClientProxy> _clientProxyMock;
        public JenkinsIntegrationTests(IntegrationSetupFixture fixture)
        {
            _fixture = fixture;
            _factory = fixture.Factory;
            _postgreSqlContainer = fixture.Container;
            _httpClient = fixture.HttpClient;
            _hubContextMock = new Mock<IHubContext<BuildStateHub>>();
            _clientProxyMock = new Mock<IClientProxy>();
        }

        [Fact]
        public async Task Invoke_ShouldReturnBuildingStateInstance()
        {
            //Arrange
            var httpMessageHandlerMock = new Mock<HttpMessageHandler>();

            _fixture.CustomHttpClientFactoryMock.Setup(c => c.GetHttpClient()).Returns(new HttpClient(httpMessageHandlerMock.Object));
            _fixture.AwsClientMock.Setup(a => a.GetAmazonS3Client()).Returns(_fixture.AmazonS3ClientMock.Object);

            var user = UserFactory.GetUser();
            var project = ProjectFactory.GetProject();
            var buildTable = BuildTableRowFactory.GetBuildTable();
            var baseline = BaselineFactory.GetBaseline();

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();

                context.User.Add(user);
                context.Project.Add(project);

                await context.SaveChangesAsync();

                buildTable.UserId = user.Id;
                buildTable.ProjectId = project.Id;

                context.BuildTableRow.Add(buildTable);

                baseline.ProjectId = project.Id;
                baseline.Project = project;

                context.Baseline.Add(baseline);

                await context.SaveChangesAsync();
            }

            //Simulates All Http Messages

            httpMessageHandlerMock
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

            httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                        req.RequestUri == new Uri($"{_fixture.Configuration["Jenkins:UrlHost"]}/job/create_jobs/api/json")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(inQueueJsonResponse, Encoding.UTF8, "application/json")
                });


            // Simulates lastBuildJob request

            var lastBuildJsonResponse = new JObject
            {
                { "inProgress", false },
                { "result", "SUCCESS" }
            }.ToString();

            httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri == new Uri($"{_fixture.Configuration["Jenkins:UrlHost"]}/job/create_jobs/lastBuild/api/json")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(lastBuildJsonResponse)
                });

            //Act
            var response = await _httpClient.PostAsync($"/api/Jenkins/Invoke/{user.Id}/{project.Id}/{buildTable.Id}/true/false", null);

            //Assert
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            var buildingState = JsonConvert.DeserializeObject<BuildingState>(responseContent);

            Assert.IsType<BuildingState>(buildingState);
            Assert.NotNull(buildingState);
        }

        [Fact]
        public async Task Invoke_ShouldReturnNotFoundResult_WhenBuildTableNotFound()
        {
            //Arrange
            var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            _fixture.CustomHttpClientFactoryMock = new Mock<ICustomHttpClientFactory>();

            _fixture.CustomHttpClientFactoryMock.Setup(c => c.GetHttpClient()).Returns(new HttpClient(httpMessageHandlerMock.Object));

            var nonExistentId = -1;

            //Act
            var response = await _httpClient.PostAsync($"/api/Jenkins/Invoke/1/1/{nonExistentId}/true/false", null);

            //Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.Contains("not found", responseContent);
        }

        [Fact]
        public async Task Invoke_ShouldReturnInternalServerErrorResult_WhenBuildHasAlreadyBeenStarted()
        {
            //Arrange
            var buildTable = BuildTableRowFactory.GetBuildTable();
            buildTable.Status = "Compiling";
            var user = UserFactory.GetUser();
            var project = ProjectFactory.GetProject();

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();
                context.User.Add(user);
                context.Project.Add(project);

                await context.SaveChangesAsync();

                buildTable.UserId = user.Id;
                buildTable.ProjectId = project.Id;

                context.BuildTableRow.Add(buildTable);

                await context.SaveChangesAsync();
            }

            var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            _fixture.CustomHttpClientFactoryMock = new Mock<ICustomHttpClientFactory>();

            _fixture.CustomHttpClientFactoryMock.Setup(c => c.GetHttpClient()).Returns(new HttpClient(httpMessageHandlerMock.Object));

            //Act
            var response = await _httpClient.PostAsync($"/api/Jenkins/Invoke/1/1/{buildTable.Id}/true/false", null);

            //Assert
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.Contains("build has already been started", responseContent);
        }

        [Fact]
        public async Task Invoke_ShouldReturnInternalServerErrorResult_WhenThereIsNoBaselineForProject()
        {
            //Arrange
            var buildTable = BuildTableRowFactory.GetBuildTable();
            var user = UserFactory.GetUser();
            var project = ProjectFactory.GetProject();

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();
                context.User.Add(user);
                context.Project.Add(project);

                await context.SaveChangesAsync();

                buildTable.UserId = user.Id;
                buildTable.ProjectId = project.Id;

                context.BuildTableRow.Add(buildTable);

                await context.SaveChangesAsync();
            }

            var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            _fixture.CustomHttpClientFactoryMock = new Mock<ICustomHttpClientFactory>();

            _fixture.CustomHttpClientFactoryMock.Setup(c => c.GetHttpClient()).Returns(new HttpClient(httpMessageHandlerMock.Object));

            //Act
            var response = await _httpClient.PostAsync($"/api/Jenkins/Invoke/1/1/{buildTable.Id}/true/false", null);

            //Assert
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.Contains("There is no baseline defined for this project", responseContent);
        }

        [Fact]
        public async Task Invoke_ShouldReturnInternalServerErrorResult_WhenInvokingJobCreationFails()
        {
            //Arrange
            var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            _fixture.CustomHttpClientFactoryMock = new Mock<ICustomHttpClientFactory>();

            _fixture.CustomHttpClientFactoryMock.Setup(c => c.GetHttpClient()).Returns(new HttpClient(httpMessageHandlerMock.Object));

            var user = UserFactory.GetUser();
            var project = ProjectFactory.GetProject();
            var buildTable = BuildTableRowFactory.GetBuildTable();
            var baseline = BaselineFactory.GetBaseline();

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();

                context.User.Add(user);
                context.Project.Add(project);

                await context.SaveChangesAsync();

                buildTable.UserId = user.Id;
                buildTable.ProjectId = project.Id;

                context.BuildTableRow.Add(buildTable);

                baseline.ProjectId = project.Id;
                baseline.Project = project;

                context.Baseline.Add(baseline);

                await context.SaveChangesAsync();
            }

            //Simulates All Http Messages
            httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadGateway
                });

            //Act
            var response = await _httpClient.PostAsync($"/api/Jenkins/Invoke/{user.Id}/{project.Id}/{buildTable.Id}/true/false", null);

            //Assert
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.Contains("Error while invoking Jenkins", responseContent);
        }

        [Fact]
        public async Task Invoke_ShouldReturnInternalServerErrorResult_WhenJobCreationFails()
        {
            //Arrange
            var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            _fixture.CustomHttpClientFactoryMock = new Mock<ICustomHttpClientFactory>();

            _fixture.CustomHttpClientFactoryMock.Setup(c => c.GetHttpClient()).Returns(new HttpClient(httpMessageHandlerMock.Object));

            var user = UserFactory.GetUser();
            var project = ProjectFactory.GetProject();
            var buildTable = BuildTableRowFactory.GetBuildTable();
            var baseline = BaselineFactory.GetBaseline();

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();

                context.User.Add(user);
                context.Project.Add(project);

                await context.SaveChangesAsync();

                buildTable.UserId = user.Id;
                buildTable.ProjectId = project.Id;

                context.BuildTableRow.Add(buildTable);

                baseline.ProjectId = project.Id;
                baseline.Project = project;

                context.Baseline.Add(baseline);

                await context.SaveChangesAsync();
            }

            //Simulates All Http Messages
            httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadGateway
                });

            // Simulates inQueue request

            var inQueueJsonResponse = new JObject
            {
                { "inQueue", false }
            }.ToString();

            httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri == new Uri($"{_fixture.Configuration["Jenkins:UrlHost"]}/job/create_jobs/api/json")),
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

            httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri == new Uri($"{_fixture.Configuration["Jenkins:UrlHost"]}/job/create_jobs/lastBuild/api/json")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(lastBuildJsonResponse)
                });

            //Act
            var response = await _httpClient.PostAsync($"/api/Jenkins/Invoke/{user.Id}/{project.Id}/{buildTable.Id}/true/false", null);

            //Assert
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.Contains("Error while invoking Jenkins", responseContent);
        }

        [Fact]
        public async Task Invoke_ShouldReturnInternalServerErrorResult_WhenStartingBuildFails()
        {
            //Arrange
            var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            _fixture.CustomHttpClientFactoryMock = new Mock<ICustomHttpClientFactory>();

            _fixture.CustomHttpClientFactoryMock.Setup(c => c.GetHttpClient()).Returns(new HttpClient(httpMessageHandlerMock.Object));
            _fixture.AwsClientMock.Setup(a => a.GetAmazonS3Client()).Returns(_fixture.AmazonS3ClientMock.Object);

            var user = UserFactory.GetUser();
            var project = ProjectFactory.GetProject();
            var buildTable = BuildTableRowFactory.GetBuildTable();
            var baseline = BaselineFactory.GetBaseline();

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();

                context.User.Add(user);
                context.Project.Add(project);

                await context.SaveChangesAsync();

                buildTable.UserId = user.Id;
                buildTable.ProjectId = project.Id;

                context.BuildTableRow.Add(buildTable);

                baseline.ProjectId = project.Id;
                baseline.Project = project;

                context.Baseline.Add(baseline);

                await context.SaveChangesAsync();
            }

            var fileName = $"{project.Id}_{user.Id}_{buildTable.TagName}";

            //Simulates All Http Messages

            httpMessageHandlerMock
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

            httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri == new Uri($"{_fixture.Configuration["Jenkins:UrlHost"]}/job/create_jobs/api/json")),
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

            httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri == new Uri($"{_fixture.Configuration["Jenkins:UrlHost"]}/job/create_jobs/lastBuild/api/json")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(lastBuildJsonResponse)
                });

            // Simulates Start Build request with Failure

            httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Post &&
                        req.RequestUri.ToString().Contains("buildWithParameters") &&
                        req.RequestUri != new Uri($"{_fixture.Configuration["Jenkins:UrlHost"]}/job/create_jobs/buildWithParameters")),

                        ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Content = new StringContent(inQueueJsonResponse)
                });

            //Act
            var response = await _httpClient.PostAsync($"/api/Jenkins/Invoke/{user.Id}/{project.Id}/{buildTable.Id}/true/false", null);

            //Assert
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.Contains("Error while starting build", responseContent);
        }

        [Fact]
        public async Task GetAllData_ShouldReturnAllJenkinsData()
        {
            //Arrange
            var user = UserFactory.GetUser();
            var project = ProjectFactory.GetProject();
            var buildTable = BuildTableRowFactory.GetBuildTable();
            var buildingState = JenkinsFactory.GetBuildingState();
            var logAndArtifact = JenkinsFactory.GetLogAndArtifact();
            var fileVerify = JenkinsFactory.GetFileVerify();

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();

                context.User.Add(user);
                context.Project.Add(project);
                await context.SaveChangesAsync();

                buildingState.UserId = user.Id;
                buildingState.ProjectId = project.Id;
                logAndArtifact.UserId = user.Id;
                logAndArtifact.ProjectId = project.Id;
                fileVerify.UserId = user.Id;
                fileVerify.ProjectId = project.Id;

                context.BuildingState.Add(buildingState);
                context.LogAndArtifact.Add(logAndArtifact);
                context.FileVerify.Add(fileVerify);

                await context.SaveChangesAsync();
            }

            //Act
            var response = await _httpClient.GetAsync($"/api/Jenkins/GetAllData/{user.Id}/{project.Id}");

            //Assert
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();

            var jenkinsAllDataResponses = JsonConvert.DeserializeObject<JenkinsAllDataResponse>(responseContent);
            Assert.NotNull(jenkinsAllDataResponses);
            Assert.NotNull(jenkinsAllDataResponses.BuildingStates.Where(b => b.Id == buildingState.Id).FirstOrDefault());
            Assert.NotNull(jenkinsAllDataResponses.LogAndArtifacts.Where(l => l.Id == logAndArtifact.Id).FirstOrDefault());
            Assert.NotNull(jenkinsAllDataResponses.FileVerifies.Where(f => f.Id == fileVerify.Id).FirstOrDefault());
        }

        //Building State

        [Fact]
        public async Task SaveBuildingState_ShouldReturnGreaterThanZero()
        {
            //Arrange
            var expectedBuildingState = JenkinsFactory.GetBuildingState();
            expectedBuildingState.JenkinsBuildLogFile = Guid.NewGuid().ToString();

            //Act
            var response = await _httpClient.PostAsJsonAsync("/api/Jenkins/BuildingState/Save", expectedBuildingState);

            //Assert
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<BuildingState>(responseContent);

            Assert.NotNull(result);
            Assert.Equal(expectedBuildingState.JenkinsBuildLogFile, result.JenkinsBuildLogFile);

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();
                var savedBuildingState = await context.BuildingState.FirstOrDefaultAsync(b => b.JenkinsBuildLogFile.Equals(expectedBuildingState.JenkinsBuildLogFile));
                Assert.NotNull(savedBuildingState);
                Assert.Equal(expectedBuildingState.JenkinsBuildLogFile, result.JenkinsBuildLogFile);
            }
        }

        [Fact]
        public async Task ListBuildingState_ShouldReturnBuildingStateList()
        {
            //Arrange
            var expectedBuildingState = JenkinsFactory.GetBuildingState();

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();
                var savedBuildingState = context.BuildingState.Add(expectedBuildingState);
                await context.SaveChangesAsync();
            }

            //Act
            var response = await _httpClient.GetAsync($"/api/Jenkins/BuildingState/List/{expectedBuildingState.UserId}/{expectedBuildingState.ProjectId}");

            //Assert
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();

            var buildingStateList = JsonConvert.DeserializeObject<List<BuildingState>>(responseContent);
            Assert.NotEmpty(buildingStateList);

            var buildingStateByDescription = buildingStateList.Where(b => b.Id.Equals(expectedBuildingState.Id)).FirstOrDefault();
            Assert.NotNull(buildingStateByDescription);

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();
                var savedBuildingState = await context.BuildingState.FirstOrDefaultAsync(b => b.Id.Equals(expectedBuildingState.Id));
                Assert.NotNull(savedBuildingState);
                Assert.Equal(expectedBuildingState.JenkinsBuildLogFile, savedBuildingState.JenkinsBuildLogFile);
            }
        }

        [Fact]
        public async Task GetBuildingStateById_ShouldReturnBuildingStateInstance()
        {
            //Arrange
            var expectedBuildingState = JenkinsFactory.GetBuildingState();
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();
                context.BuildingState.Add(expectedBuildingState);
                await context.SaveChangesAsync();
            }

            //Act
            var response = await _httpClient.GetAsync($"/api/Jenkins/BuildingState/Get/{expectedBuildingState.Id}");

            //Assert
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            var buildingState = JsonConvert.DeserializeObject<BuildingState>(responseContent);

            Assert.NotNull(buildingState);
            Assert.Equal(expectedBuildingState.JenkinsBuildLogFile, buildingState.JenkinsBuildLogFile);

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();
                var savedBuildingState = await context.BuildingState.FirstOrDefaultAsync(b => b.JenkinsBuildLogFile.Equals(expectedBuildingState.JenkinsBuildLogFile));
                Assert.NotNull(savedBuildingState);
                Assert.Equal(expectedBuildingState.JenkinsBuildLogFile, savedBuildingState.JenkinsBuildLogFile);
            }
        }

        [Fact]
        public async Task GetBuildingStateById_ShouldReturnNotFoundResult()
        {
            //Arrange
            var nonExistentId = -1;

            //Act
            var response = await _httpClient.GetAsync($"/api/Jenkins/BuildingState/Get/{nonExistentId}");

            //Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.Contains("not found", responseContent);
        }
        [Fact]
        public async Task GetBuildingStateByBuildId_ShouldReturnBuildingStateInstance()
        {
            //Arrange
            var expectedBuildingState = JenkinsFactory.GetBuildingState();
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();
                context.BuildingState.Add(expectedBuildingState);
                await context.SaveChangesAsync();
            }

            //Act
            var response = await _httpClient.GetAsync($"/api/Jenkins/BuildingState/GetByBuildId/{expectedBuildingState.BuildId}");

            //Assert
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            var buildingState = JsonConvert.DeserializeObject<BuildingState>(responseContent);

            Assert.NotNull(buildingState);
            Assert.Equal(expectedBuildingState.JenkinsBuildLogFile, buildingState.JenkinsBuildLogFile);

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();
                var savedBuildingState = await context.BuildingState.FirstOrDefaultAsync(b => b.JenkinsBuildLogFile.Equals(expectedBuildingState.JenkinsBuildLogFile));
                Assert.NotNull(savedBuildingState);
                Assert.Equal(expectedBuildingState.JenkinsBuildLogFile, savedBuildingState.JenkinsBuildLogFile);
            }
        }

        [Fact]
        public async Task UpdateBuildingState_ShouldReturnGreaterThanZero()
        {
            //Arrange
            var currentBuildingState = JenkinsFactory.GetBuildingState();
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();
                context.BuildingState.Add(currentBuildingState);
                await context.SaveChangesAsync();
            }

            var buildingStateRequest = JenkinsFactory.GetBuildingState();
            buildingStateRequest.JenkinsBuildLogFile = "updated buildingState build log file";

            //Act
            var response = await _httpClient.PutAsJsonAsync($"/api/Jenkins/BuildingState/Update/{currentBuildingState.Id}", buildingStateRequest);

            //Assert
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            var buildingStateSaved = JsonConvert.DeserializeObject<BuildingState>(responseContent);

            Assert.NotNull(buildingStateSaved);
            Assert.Equal(buildingStateRequest.JenkinsBuildLogFile, buildingStateSaved.JenkinsBuildLogFile);

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();
                var updatedBuildingState = await context.BuildingState.FirstOrDefaultAsync(b => b.Id.Equals(currentBuildingState.Id));
                Assert.NotNull(updatedBuildingState);
                Assert.Equal(buildingStateRequest.JenkinsBuildLogFile, updatedBuildingState.JenkinsBuildLogFile);
                Assert.NotEqual(currentBuildingState.JenkinsBuildLogFile, updatedBuildingState.JenkinsBuildLogFile);
            }
        }

        [Fact]
        public async Task UpdateBuildingState_ShouldReturnNotFoundResult()
        {
            //Arrange
            var nonExistentId = -1;

            //Act
            var response = await _httpClient.PutAsJsonAsync($"/api/Jenkins/BuildingState/Update/{nonExistentId}", JenkinsFactory.GetBuildingState());

            //Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.Contains("not found", responseContent);
        }

        [Fact]
        public async Task DeleteBuildingState_ShouldReturnGreaterThanZero()
        {
            //Arrange
            var buildingState = JenkinsFactory.GetBuildingState();
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();
                context.BuildingState.Add(buildingState);
                await context.SaveChangesAsync();
            }

            //Act
            var response = await _httpClient.DeleteAsync($"/api/Jenkins/BuildingState/Delete/{buildingState.Id}");

            //Assert
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            var updated = JsonConvert.DeserializeObject<int>(responseContent);

            Assert.NotEqual(0, updated);

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();
                var savedBuildingState = await context.BuildingState.FirstOrDefaultAsync(n => n.Id.Equals(buildingState.Id));
                Assert.Null(savedBuildingState);
            }
        }

        [Fact]
        public async Task DeleteBuildingState_ShouldReturnNotFoundResult()
        {
            //Arrange
            var nonExistentId = 99;

            //Act
            var response = await _httpClient.DeleteAsync($"/api/Jenkins/BuildingState/Delete/{nonExistentId}");

            //Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.Contains("not found", responseContent);
        }

        // Log and Artifact

        [Fact]
        public async Task SaveLogAndArtifact_ShouldReturnGreaterThanZero()
        {
            //Arrange
            var expectedLogAndArtifact = JenkinsFactory.GetLogAndArtifact();
            expectedLogAndArtifact.FileLogS3BucketLocation = Guid.NewGuid().ToString();
            _hubContextMock.Setup(h => h.Clients.All).Returns(_clientProxyMock.Object);

            //Act
            var response = await _httpClient.PostAsJsonAsync("/api/Jenkins/LogAndArtifact/Save", expectedLogAndArtifact);

            //Assert
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<int>(responseContent);

            Assert.NotEqual(0, result);

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();
                var savedLogAndArtifact = await context.LogAndArtifact.FirstOrDefaultAsync(l => l.FileLogS3BucketLocation.Equals(expectedLogAndArtifact.FileLogS3BucketLocation));
                Assert.NotNull(savedLogAndArtifact);
                Assert.Equal(expectedLogAndArtifact.FileLogS3BucketLocation, savedLogAndArtifact.FileLogS3BucketLocation);
            }
        }

        [Fact]
        public async Task ListLogAndArtifact_ShouldReturnLogAndArtifactList()
        {
            //Arrange
            var expectedLogAndArtifact = JenkinsFactory.GetLogAndArtifact();

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();
                var savedLogAndArtifact = context.LogAndArtifact.Add(expectedLogAndArtifact);
                await context.SaveChangesAsync();
            }

            //Act
            var response = await _httpClient.GetAsync($"/api/Jenkins/LogAndArtifact/List/{expectedLogAndArtifact.UserId}/{expectedLogAndArtifact.ProjectId}");

            //Assert
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();

            var logAndArtifactList = JsonConvert.DeserializeObject<List<LogAndArtifact>>(responseContent);
            Assert.NotEmpty(logAndArtifactList);

            var logAndArtifactByDescription = logAndArtifactList.Where(b => b.Id.Equals(expectedLogAndArtifact.Id)).FirstOrDefault();
            Assert.NotNull(logAndArtifactByDescription);

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();
                var savedLogAndArtifact = await context.LogAndArtifact.FirstOrDefaultAsync(b => b.Id.Equals(expectedLogAndArtifact.Id));
                Assert.NotNull(savedLogAndArtifact);
                Assert.Equal(expectedLogAndArtifact.FileLogS3BucketLocation, savedLogAndArtifact.FileLogS3BucketLocation);
            }
        }

        [Fact]
        public async Task GetLogAndArtifactById_ShouldReturnLogAndArtifactInstance()
        {
            //Arrange
            var expectedLogAndArtifact = JenkinsFactory.GetLogAndArtifact();
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();
                context.LogAndArtifact.Add(expectedLogAndArtifact);
                await context.SaveChangesAsync();
            }

            //Act
            var response = await _httpClient.GetAsync($"/api/Jenkins/LogAndArtifact/Get/{expectedLogAndArtifact.Id}");

            //Assert
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            var logAndArtifact = JsonConvert.DeserializeObject<LogAndArtifact>(responseContent);

            Assert.NotNull(logAndArtifact);
            Assert.Equal(expectedLogAndArtifact.FileLogS3BucketLocation, logAndArtifact.FileLogS3BucketLocation);

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();
                var savedLogAndArtifact = await context.LogAndArtifact.FirstOrDefaultAsync(l => l.FileLogS3BucketLocation.Equals(expectedLogAndArtifact.FileLogS3BucketLocation));
                Assert.NotNull(savedLogAndArtifact);
                Assert.Equal(expectedLogAndArtifact.FileLogS3BucketLocation, savedLogAndArtifact.FileLogS3BucketLocation);
            }
        }

        [Fact]
        public async Task GetLogAndArtifactById_ShouldReturnNotFoundResult()
        {
            //Arrange
            var nonExistentId = -1;

            //Act
            var response = await _httpClient.GetAsync($"/api/Jenkins/LogAndArtifact/Get/{nonExistentId}");

            //Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.Contains("not found", responseContent);
        }

        [Fact]
        public async Task UpdateLogAndArtifact_ShouldReturnGreaterThanZero()
        {
            //Arrange
            var currentLogAndArtifact = JenkinsFactory.GetLogAndArtifact();
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();
                context.LogAndArtifact.Add(currentLogAndArtifact);
                await context.SaveChangesAsync();
            }

            var logAndArtifactRequest = JenkinsFactory.GetLogAndArtifact();
            logAndArtifactRequest.FileLogS3BucketLocation = "updated logAndArtifact build log file";

            //Act
            var response = await _httpClient.PutAsJsonAsync($"/api/Jenkins/LogAndArtifact/Update/{currentLogAndArtifact.Id}", logAndArtifactRequest);

            //Assert
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<int>(responseContent);

            Assert.NotEqual(0, result);

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();
                var updatedLogAndArtifact = await context.LogAndArtifact.FirstOrDefaultAsync(b => b.Id.Equals(currentLogAndArtifact.Id));
                Assert.NotNull(updatedLogAndArtifact);
                Assert.Equal(logAndArtifactRequest.FileLogS3BucketLocation, updatedLogAndArtifact.FileLogS3BucketLocation);
                Assert.NotEqual(currentLogAndArtifact.FileLogS3BucketLocation, updatedLogAndArtifact.FileLogS3BucketLocation);
            }
        }

        [Fact]
        public async Task UpdateLogAndArtifact_ShouldReturnNotFoundResult()
        {
            //Arrange
            var nonExistentId = -1;

            //Act
            var response = await _httpClient.PutAsJsonAsync($"/api/Jenkins/LogAndArtifact/Update/{nonExistentId}", JenkinsFactory.GetLogAndArtifact());

            //Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.Contains("not found", responseContent);
        }

        [Fact]
        public async Task DeleteLogAndArtifact_ShouldReturnGreaterThanZero()
        {
            //Arrange
            var logAndArtifact = JenkinsFactory.GetLogAndArtifact();
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();
                context.LogAndArtifact.Add(logAndArtifact);
                await context.SaveChangesAsync();
            }

            //Act
            var response = await _httpClient.DeleteAsync($"/api/Jenkins/LogAndArtifact/Delete/{logAndArtifact.Id}");

            //Assert
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            var updated = JsonConvert.DeserializeObject<int>(responseContent);

            Assert.NotEqual(0, updated);

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();
                var savedLogAndArtifact = await context.LogAndArtifact.FirstOrDefaultAsync(n => n.Id.Equals(logAndArtifact.Id));
                Assert.Null(savedLogAndArtifact);
            }
        }

        [Fact]
        public async Task DeleteLogAndArtifact_ShouldReturnNotFoundResult()
        {
            //Arrange
            var nonExistentId = 99;

            //Act
            var response = await _httpClient.DeleteAsync($"/api/Jenkins/LogAndArtifact/Delete/{nonExistentId}");

            //Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.Contains("not found", responseContent);
        }

        // File Verify

        [Fact]
        public async Task SaveFileVerify_ShouldReturnGreaterThanZero()
        {
            //Arrange
            var expectedFileVerify = JenkinsFactory.GetFileVerify();
            expectedFileVerify.Filename = Guid.NewGuid().ToString();
            _hubContextMock.Setup(h => h.Clients.All).Returns(_clientProxyMock.Object);

            //Act
            var response = await _httpClient.PostAsJsonAsync("/api/Jenkins/FileVerify/Save", expectedFileVerify);

            //Assert
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<int>(responseContent);

            Assert.NotEqual(0, result);

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();
                var savedFileVerify = await context.FileVerify.FirstOrDefaultAsync(f => f.Filename.Equals(expectedFileVerify.Filename));
                Assert.NotNull(savedFileVerify);
                Assert.Equal(expectedFileVerify.Filename, savedFileVerify.Filename);
            }
        }

        [Fact]
        public async Task ListFileVerify_ShouldReturnFileVerifyList()
        {
            //Arrange
            var expectedFileVerify = JenkinsFactory.GetFileVerify();

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();
                var savedFileVerify = context.FileVerify.Add(expectedFileVerify);
                await context.SaveChangesAsync();
            }

            //Act
            var response = await _httpClient.GetAsync($"/api/Jenkins/FileVerify/List/{expectedFileVerify.UserId}/{expectedFileVerify.ProjectId}");

            //Assert
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();

            var fileVerifyList = JsonConvert.DeserializeObject<List<FileVerify>>(responseContent);
            Assert.NotEmpty(fileVerifyList);

            var fileVerifyByDescription = fileVerifyList.Where(b => b.Id.Equals(expectedFileVerify.Id)).FirstOrDefault();
            Assert.NotNull(fileVerifyByDescription);

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();
                var savedFileVerify = await context.FileVerify.FirstOrDefaultAsync(b => b.Id.Equals(expectedFileVerify.Id));
                Assert.NotNull(savedFileVerify);
                Assert.Equal(expectedFileVerify.Filename, savedFileVerify.Filename);
            }
        }

        [Fact]
        public async Task GetFileVerifyById_ShouldReturnFileVerifyInstance()
        {
            //Arrange
            var expectedFileVerify = JenkinsFactory.GetFileVerify();
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();
                context.FileVerify.Add(expectedFileVerify);
                await context.SaveChangesAsync();
            }

            //Act
            var response = await _httpClient.GetAsync($"/api/Jenkins/FileVerify/Get/{expectedFileVerify.Id}");

            //Assert
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            var fileVerify = JsonConvert.DeserializeObject<FileVerify>(responseContent);

            Assert.NotNull(fileVerify);
            Assert.Equal(expectedFileVerify.Filename, fileVerify.Filename);

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();
                var savedFileVerify = await context.FileVerify.FirstOrDefaultAsync(f => f.Filename.Equals(expectedFileVerify.Filename));
                Assert.NotNull(savedFileVerify);
                Assert.Equal(expectedFileVerify.Filename, savedFileVerify.Filename);
            }
        }

        [Fact]
        public async Task GetFileVerifyById_ShouldReturnNotFoundResult()
        {
            //Arrange
            var nonExistentId = -1;

            //Act
            var response = await _httpClient.GetAsync($"/api/Jenkins/FileVerify/Get/{nonExistentId}");

            //Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.Contains("not found", responseContent);
        }

        [Fact]
        public async Task UpdateFileVerify_ShouldReturnGreaterThanZero()
        {
            //Arrange
            var currentFileVerify = JenkinsFactory.GetFileVerify();
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();
                context.FileVerify.Add(currentFileVerify);
                await context.SaveChangesAsync();
            }

            var fileVerifyRequest = JenkinsFactory.GetFileVerify();
            fileVerifyRequest.Filename = "updated fileVerify build log file";

            //Act
            var response = await _httpClient.PutAsJsonAsync($"/api/Jenkins/FileVerify/Update/{currentFileVerify.Id}", fileVerifyRequest);

            //Assert
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<int>(responseContent);

            Assert.NotEqual(0, result);

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();
                var updatedFileVerify = await context.FileVerify.FirstOrDefaultAsync(b => b.Id.Equals(currentFileVerify.Id));
                Assert.NotNull(updatedFileVerify);
                Assert.Equal(fileVerifyRequest.Filename, updatedFileVerify.Filename);
                Assert.NotEqual(currentFileVerify.Filename, updatedFileVerify.Filename);
            }
        }

        [Fact]
        public async Task UpdateFileVerify_ShouldReturnNotFoundResult()
        {
            //Arrange
            var nonExistentId = -1;

            //Act
            var response = await _httpClient.PutAsJsonAsync($"/api/Jenkins/FileVerify/Update/{nonExistentId}", JenkinsFactory.GetFileVerify());

            //Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.Contains("not found", responseContent);
        }

        [Fact]
        public async Task DeleteFileVerify_ShouldReturnGreaterThanZero()
        {
            //Arrange
            var fileVerify = JenkinsFactory.GetFileVerify();
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();
                context.FileVerify.Add(fileVerify);
                await context.SaveChangesAsync();
            }

            //Act
            var response = await _httpClient.DeleteAsync($"/api/Jenkins/FileVerify/Delete/{fileVerify.Id}");

            //Assert
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            var updated = JsonConvert.DeserializeObject<int>(responseContent);

            Assert.NotEqual(0, updated);

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();
                var savedFileVerify = await context.FileVerify.FirstOrDefaultAsync(n => n.Id.Equals(fileVerify.Id));
                Assert.Null(savedFileVerify);
            }
        }

        [Fact]
        public async Task DeleteFileVerify_ShouldReturnNotFoundResult()
        {
            //Arrange
            var nonExistentId = 99;

            //Act
            var response = await _httpClient.DeleteAsync($"/api/Jenkins/FileVerify/Delete/{nonExistentId}");

            //Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.Contains("not found", responseContent);
        }
    }

}

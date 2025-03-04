using Marelli.Domain.Entities;
using Marelli.Infra.Context;
using Marelli.Test.Integration.Configuration;
using Marelli.Test.Utils.Factories;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Json;
using Testcontainers.PostgreSql;
using Xunit;


namespace Marelli.Test.Integration
{
    [Collection("Integration collection")]
    public class BuildTableIntegrationTests
    {
        private readonly IntegrationSetupFixture _fixture;
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _httpClient;
        private readonly PostgreSqlContainer _postgreSqlContainer;
        public BuildTableIntegrationTests(IntegrationSetupFixture fixture)
        {
            _fixture = fixture;
            _factory = fixture.Factory;
            _postgreSqlContainer = fixture.Container;
            _httpClient = fixture.HttpClient;
        }

        [Fact]
        public async Task SaveBuildTable_ShouldReturnGreaterThanZero()
        {
            //Arrange
            var expectedBuildTable = BuildTableRowFactory.GetBuildTable();
            expectedBuildTable.FileName = "save buildTable description";

            //Act
            var response = await _httpClient.PostAsJsonAsync("/api/BuildTable/Save", expectedBuildTable);

            //Assert
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<BuildTableRow>(responseContent);

            Assert.NotNull(result);
            Assert.Equal(expectedBuildTable.FileName, result.FileName);

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();
                var savedBuildTable = await context.BuildTableRow.FirstOrDefaultAsync(b => b.FileName.Equals(expectedBuildTable.FileName));
                Assert.NotNull(savedBuildTable);
                Assert.Equal(expectedBuildTable.FileName, savedBuildTable.FileName);
            }
        }

        [Fact]
        public async Task ListBuildTable_ShouldReturnBuildTableList()
        {
            //Arrange
            var expectedBuildTable = BuildTableRowFactory.GetBuildTable();
            var user = UserFactory.GetUserWithoutGroup();
            var project = ProjectFactory.GetProject();

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();

                context.User.Add(user);

                await context.SaveChangesAsync();

                project.UsersProject = new List<UserProject> { UserProjectFactory.GetUserProject(user.Id, 0) };
                context.Project.Add(project);

                await context.SaveChangesAsync();

                expectedBuildTable.UserId = user.Id;
                expectedBuildTable.ProjectId = project.Id;

                var savedBuildTable = context.BuildTableRow.Add(expectedBuildTable);
                await context.SaveChangesAsync();
            }

            //Act
            var response = await _httpClient.GetAsync($"/api/BuildTable/List/{expectedBuildTable.UserId}");

            //Assert
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();

            var buildTableList = JsonConvert.DeserializeObject<List<BuildTableRow>>(responseContent);
            Assert.NotEmpty(buildTableList);

            var buildTableByFileName = buildTableList.Where(b => b.FileName.Equals(expectedBuildTable.FileName)).FirstOrDefault();
            Assert.NotNull(buildTableByFileName);

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();
                var savedBuildTable = await context.BuildTableRow.FirstOrDefaultAsync(b => b.FileName.Equals(expectedBuildTable.FileName));
                Assert.NotNull(savedBuildTable);
                Assert.Equal(expectedBuildTable.FileName, savedBuildTable.FileName);
            }
        }

        [Fact]
        public async Task ListBuildTableInProgress_ShouldReturnBuildTableList()
        {
            //Arrange
            var expectedBuildTable = BuildTableRowFactory.GetBuildTable();
            expectedBuildTable.Status = "Compiling";
            var user = UserFactory.GetUserWithoutGroup();
            var project = ProjectFactory.GetProject();

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();

                context.User.Add(user);

                await context.SaveChangesAsync();

                project.UsersProject = new List<UserProject> { UserProjectFactory.GetUserProject(user.Id, 0) };
                context.Project.Add(project);

                await context.SaveChangesAsync();

                expectedBuildTable.UserId = user.Id;
                expectedBuildTable.ProjectId = project.Id;

                var savedBuildTable = context.BuildTableRow.Add(expectedBuildTable);
                await context.SaveChangesAsync();
            }

            //Act
            var response = await _httpClient.GetAsync($"/api/BuildTable/ListInProgress/{expectedBuildTable.UserId}");

            //Assert
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();

            var buildTableList = JsonConvert.DeserializeObject<List<BuildTableRow>>(responseContent);
            Assert.NotEmpty(buildTableList);

            var buildTableByFileName = buildTableList.Where(b => b.FileName.Equals(expectedBuildTable.FileName)).FirstOrDefault();
            Assert.NotNull(buildTableByFileName);

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();
                var savedBuildTable = await context.BuildTableRow.FirstOrDefaultAsync(b => b.FileName.Equals(expectedBuildTable.FileName));
                Assert.NotNull(savedBuildTable);
                Assert.Equal(expectedBuildTable.FileName, savedBuildTable.FileName);
            }
        }

        [Fact]
        public async Task GetBuildTable_ShouldReturnBuildTableInstance()
        {
            //Arrange
            var expectedBuildTable = BuildTableRowFactory.GetBuildTable();
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();
                context.BuildTableRow.Add(expectedBuildTable);
                await context.SaveChangesAsync();
            }

            //Act
            var response = await _httpClient.GetAsync($"/api/BuildTable/Get/{expectedBuildTable.Id}");

            //Assert
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            var buildTable = JsonConvert.DeserializeObject<BuildTableRow>(responseContent);

            Assert.NotNull(buildTable);
            Assert.Equal(expectedBuildTable.FileName, buildTable.FileName);

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();
                var savedBuildTable = await context.BuildTableRow.FirstOrDefaultAsync(b => b.FileName.Equals(expectedBuildTable.FileName));
                Assert.NotNull(savedBuildTable);
                Assert.Equal(expectedBuildTable.FileName, savedBuildTable.FileName);
            }
        }

        [Fact]
        public async Task GetBuildTable_ShouldReturnNotFoundResult()
        {
            //Arrange
            var nonExistentId = -1;

            //Act
            var response = await _httpClient.GetAsync($"/api/BuildTable/Get/{nonExistentId}");

            //Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.Contains("not found", responseContent);
        }

        [Fact]
        public async Task UpdateBuildTable_ShouldReturnGreaterThanZero()
        {
            //Arrange
            var currentBuildTable = BuildTableRowFactory.GetBuildTable();
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();
                context.BuildTableRow.Add(currentBuildTable);
                await context.SaveChangesAsync();
            }

            var buildTableRequest = BuildTableRowFactory.GetBuildTable();
            buildTableRequest.FileName = "updated buildTable description";

            //Act
            var response = await _httpClient.PutAsJsonAsync($"/api/BuildTable/Update/{currentBuildTable.Id}", buildTableRequest);

            //Assert
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            var updated = JsonConvert.DeserializeObject<int>(responseContent);

            Assert.NotEqual(0, updated);

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();
                var updatedBuildTable = await context.BuildTableRow.FirstOrDefaultAsync(b => b.Id.Equals(currentBuildTable.Id));
                Assert.NotNull(updatedBuildTable);
                Assert.Equal(buildTableRequest.FileName, updatedBuildTable.FileName);
                Assert.NotEqual(currentBuildTable.FileName, updatedBuildTable.FileName);
            }
        }

        [Fact]
        public async Task UpdateBuildTable_ShouldReturnNotFoundResult()
        {
            //Arrange
            var nonExistentId = -1;

            //Act
            var response = await _httpClient.PutAsJsonAsync($"/api/BuildTable/Update/{nonExistentId}", BuildTableRowFactory.GetBuildTable());

            //Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.Contains("not found", responseContent);
        }

        [Fact]
        public async Task DeleteBuildTable_ShouldReturnGreaterThanZero()
        {
            //Arrange
            var buildTable = BuildTableRowFactory.GetBuildTable();
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();
                context.BuildTableRow.Add(buildTable);
                await context.SaveChangesAsync();
            }

            _fixture.HttpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK
                });

            //Act
            var response = await _httpClient.DeleteAsync($"/api/BuildTable/Delete/{buildTable.Id}");

            //Assert
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            var updated = JsonConvert.DeserializeObject<int>(responseContent);

            Assert.NotEqual(0, updated);

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();
                var savedBuildTable = await context.BuildTableRow.FirstOrDefaultAsync(b => b.Id.Equals(buildTable.Id));
                Assert.NotNull(savedBuildTable);
                Assert.Equal(buildTable.Status, savedBuildTable.Status);
            }
        }

        [Fact]
        public async Task DeleteBuildTable_ShouldReturnNotFoundResult()
        {
            //Arrange
            var nonExistentId = -1;

            //Act
            var response = await _httpClient.DeleteAsync($"/api/BuildTable/Delete/{nonExistentId}");

            //Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.Contains("not found", responseContent);
        }
    }


}

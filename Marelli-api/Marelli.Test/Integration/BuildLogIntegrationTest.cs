using Marelli.Domain.Entities;
using Marelli.Infra.Context;
using Marelli.Test.Integration.Configuration;
using Marelli.Test.Utils.Factories;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Json;
using Testcontainers.PostgreSql;
using Xunit;


namespace Marelli.Test.Integration
{
    [Collection("Integration collection")]
    public class BuildLogIntegrationTests
    {
        private readonly IntegrationSetupFixture _fixture;
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _httpClient;
        private readonly PostgreSqlContainer _postgreSqlContainer;
        public BuildLogIntegrationTests(IntegrationSetupFixture fixture)
        {
            _fixture = fixture;
            _factory = fixture.Factory;
            _postgreSqlContainer = fixture.Container;
            _httpClient = fixture.HttpClient;
        }

        [Fact]
        public async Task SaveBuildLog_ShouldReturnGreaterThanZero()
        {
            //Arrange
            var expectedBuildLog = BuildLogFactory.GetBuildLog();
            expectedBuildLog.Status = "save buildLog description";

            //Act
            var response = await _httpClient.PostAsJsonAsync("/api/BuildLog/Save", expectedBuildLog);

            //Assert
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<BuildLog>(responseContent);

            Assert.NotNull(result);
            Assert.Equal(expectedBuildLog.Status, result.Status);

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();
                var savedBuildLog = await context.BuildLog.FirstOrDefaultAsync(b => b.Status.Equals(expectedBuildLog.Status));
                Assert.NotNull(savedBuildLog);
                Assert.Equal(expectedBuildLog.Status, savedBuildLog.Status);
            }
        }

        [Fact]
        public async Task ListBuildLog_ShouldReturnBuildLogList()
        {
            //Arrange
            var expectedBuildLog = BuildLogFactory.GetBuildLog();

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();
                var savedBuildLog = context.BuildLog.Add(expectedBuildLog);
                await context.SaveChangesAsync();
            }

            //Act
            var response = await _httpClient.GetAsync($"/api/BuildLog/List/{expectedBuildLog.BuildId}");

            //Assert
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();

            var buildLogList = JsonConvert.DeserializeObject<List<BuildLog>>(responseContent);
            Assert.NotEmpty(buildLogList);

            var buildLogByStatus = buildLogList.Where(b => b.Status.Equals(expectedBuildLog.Status)).FirstOrDefault();
            Assert.NotNull(buildLogByStatus);

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();
                var savedBuildLog = await context.BuildLog.FirstOrDefaultAsync(b => b.Status.Equals(expectedBuildLog.Status));
                Assert.NotNull(savedBuildLog);
                Assert.Equal(expectedBuildLog.Status, savedBuildLog.Status);
            }
        }

        [Fact]
        public async Task GetBuildLog_ShouldReturnBuildLogInstance()
        {
            //Arrange
            var expectedBuildLog = BuildLogFactory.GetBuildLog();
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();
                context.BuildLog.Add(expectedBuildLog);
                await context.SaveChangesAsync();
            }

            //Act
            var response = await _httpClient.GetAsync($"/api/BuildLog/Get/{expectedBuildLog.Id}");

            //Assert
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            var buildLog = JsonConvert.DeserializeObject<BuildLog>(responseContent);

            Assert.NotNull(buildLog);
            Assert.Equal(expectedBuildLog.Status, buildLog.Status);

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();
                var savedBuildLog = await context.BuildLog.FirstOrDefaultAsync(b => b.Status.Equals(expectedBuildLog.Status));
                Assert.NotNull(savedBuildLog);
                Assert.Equal(expectedBuildLog.Status, savedBuildLog.Status);
            }
        }

        [Fact]
        public async Task GetBuildLog_ShouldReturnNotFoundResult()
        {
            //Arrange
            var nonExistentId = -1;

            //Act
            var response = await _httpClient.GetAsync($"/api/BuildLog/Get/{nonExistentId}");

            //Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.Contains("not found", responseContent);
        }

        [Fact]
        public async Task UpdateBuildLog_ShouldReturnGreaterThanZero()
        {
            //Arrange
            var currentBuildLog = BuildLogFactory.GetBuildLog();
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();
                context.BuildLog.Add(currentBuildLog);
                await context.SaveChangesAsync();
            }

            var buildLogRequest = BuildLogFactory.GetBuildLog();
            buildLogRequest.Status = "updated buildLog description";

            //Act
            var response = await _httpClient.PutAsJsonAsync($"/api/BuildLog/Update/{currentBuildLog.Id}", buildLogRequest);

            //Assert
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            var updated = JsonConvert.DeserializeObject<int>(responseContent);

            Assert.NotEqual(0, updated);

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();
                var updatedBuildLog = await context.BuildLog.FirstOrDefaultAsync(b => b.Id.Equals(currentBuildLog.Id));
                Assert.NotNull(updatedBuildLog);
                Assert.Equal(buildLogRequest.Status, updatedBuildLog.Status);
                Assert.NotEqual(currentBuildLog.Status, updatedBuildLog.Status);
            }
        }

        [Fact]
        public async Task UpdateBuildLog_ShouldReturnNotFoundResult()
        {
            //Arrange
            var nonExistentId = -1;

            //Act
            var response = await _httpClient.PutAsJsonAsync($"/api/BuildLog/Update/{nonExistentId}", BuildLogFactory.GetBuildLog());

            //Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.Contains("not found", responseContent);
        }

        [Fact]
        public async Task DeleteBuildLog_ShouldReturnGreaterThanZero()
        {
            //Arrange
            var buildLog = BuildLogFactory.GetBuildLog();
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();
                context.BuildLog.Add(buildLog);
                await context.SaveChangesAsync();
            }

            //Act
            var response = await _httpClient.DeleteAsync($"/api/BuildLog/Delete/{buildLog.Id}");

            //Assert
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            var updated = JsonConvert.DeserializeObject<int>(responseContent);

            Assert.NotEqual(0, updated);

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();
                var savedBuildLog = await context.BuildLog.FirstOrDefaultAsync(b => b.Id.Equals(buildLog.Id));
                Assert.Null(savedBuildLog);
            }
        }

        [Fact]
        public async Task DeleteBuildLog_ShouldReturnNotFoundResult()
        {
            //Arrange
            var nonExistentId = -1;

            //Act
            var response = await _httpClient.DeleteAsync($"/api/BuildLog/Delete/{nonExistentId}");

            //Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.Contains("not found", responseContent);
        }
    }


}

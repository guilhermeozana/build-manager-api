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
    public class BaselineIntegrationTests
    {
        private readonly IntegrationSetupFixture _fixture;
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _httpClient;
        private readonly PostgreSqlContainer _postgreSqlContainer;
        public BaselineIntegrationTests(IntegrationSetupFixture fixture)
        {
            _fixture = fixture;
            _factory = fixture.Factory;
            _postgreSqlContainer = fixture.Container;
            _httpClient = fixture.HttpClient;
        }

        [Fact]
        public async Task SaveBaseline_ShouldReturnGreaterThanZero()
        {
            //Arrange
            var expectedBaseline = BaselineFactory.GetBaseline();
            expectedBaseline.FileName = "save baseline description";

            var project = ProjectFactory.GetProject();

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();
                context.Project.Add(project);
                await context.SaveChangesAsync();
            }

            expectedBaseline.ProjectId = project.Id;
            expectedBaseline.Project = project;

            //Act
            var response = await _httpClient.PostAsJsonAsync("/api/Baseline/Save", expectedBaseline);

            //Assert
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<Baseline>(responseContent);

            Assert.NotNull(result);
            Assert.Equal(expectedBaseline.FileName, result.FileName);

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();
                var savedBaseline = await context.Baseline.FirstOrDefaultAsync(b => b.FileName.Equals(expectedBaseline.FileName));
                Assert.NotNull(savedBaseline);
                Assert.Equal(expectedBaseline.FileName, savedBaseline.FileName);
            }
        }

        [Fact]
        public async Task ListBaseline_ShouldReturnBaselineList()
        {
            //Arrange
            var expectedBaseline = BaselineFactory.GetBaseline();
            var project = ProjectFactory.GetProject();

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();

                context.Project.Add(project);
                await context.SaveChangesAsync();

                expectedBaseline.ProjectId = project.Id;
                context.Baseline.Add(expectedBaseline);
                await context.SaveChangesAsync();
            }

            //Act
            var response = await _httpClient.GetAsync($"/api/Baseline/List/{expectedBaseline.ProjectId}");

            //Assert
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();

            var baselineList = JsonConvert.DeserializeObject<List<Baseline>>(responseContent);
            Assert.NotEmpty(baselineList);

            var baselineByFileName = baselineList.Where(b => b.FileName.Equals(expectedBaseline.FileName)).FirstOrDefault();
            Assert.NotNull(baselineByFileName);

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();
                var savedBaseline = await context.Baseline.FirstOrDefaultAsync(b => b.FileName.Equals(expectedBaseline.FileName));
                Assert.NotNull(savedBaseline);
                Assert.Equal(expectedBaseline.FileName, savedBaseline.FileName);
            }
        }

        [Fact]
        public async Task GetBaseline_ShouldReturnBaselineInstance()
        {
            //Arrange
            var expectedBaseline = BaselineFactory.GetBaseline();
            var project = ProjectFactory.GetProject();
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();

                context.Project.Add(project);
                await context.SaveChangesAsync();

                expectedBaseline.ProjectId = project.Id;
                context.Baseline.Add(expectedBaseline);
                await context.SaveChangesAsync();
            }

            //Act
            var response = await _httpClient.GetAsync($"/api/Baseline/Get/{expectedBaseline.Id}");

            //Assert
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            var baseline = JsonConvert.DeserializeObject<Baseline>(responseContent);

            Assert.NotNull(baseline);
            Assert.Equal(expectedBaseline.FileName, baseline.FileName);

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();
                var savedBaseline = await context.Baseline.FirstOrDefaultAsync(b => b.FileName.Equals(expectedBaseline.FileName));
                Assert.NotNull(savedBaseline);
                Assert.Equal(expectedBaseline.FileName, savedBaseline.FileName);
            }
        }

        [Fact]
        public async Task GetBaseline_ShouldReturnNotFoundResult()
        {
            //Arrange
            var nonExistentId = -1;

            //Act
            var response = await _httpClient.GetAsync($"/api/Baseline/Get/{nonExistentId}");

            //Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.Contains("not found", responseContent);
        }

        [Fact]
        public async Task UpdateBaseline_ShouldReturnGreaterThanZero()
        {
            //Arrange
            var currentBaseline = BaselineFactory.GetBaseline();
            var project = ProjectFactory.GetProject();

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();

                context.Project.Add(project);
                await context.SaveChangesAsync();

                currentBaseline.ProjectId = project.Id;
                currentBaseline.Project = project;

                context.Baseline.Add(currentBaseline);
                await context.SaveChangesAsync();
            }

            var baselineRequest = BaselineFactory.GetBaseline();
            baselineRequest.FileName = "updated baseline description";
            baselineRequest.ProjectId = project.Id;
            baselineRequest.Project = project;

            //Act
            var response = await _httpClient.PutAsJsonAsync($"/api/Baseline/Update/{currentBaseline.Id}", baselineRequest);

            //Assert
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            var updated = JsonConvert.DeserializeObject<int>(responseContent);

            Assert.NotEqual(0, updated);

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();
                var updatedBaseline = await context.Baseline.FirstOrDefaultAsync(b => b.Id.Equals(currentBaseline.Id));
                Assert.NotNull(updatedBaseline);
                Assert.Equal(baselineRequest.FileName, updatedBaseline.FileName);
                Assert.NotEqual(currentBaseline.FileName, updatedBaseline.FileName);
            }
        }

        [Fact]
        public async Task UpdateBaseline_ShouldReturnNotFoundResult()
        {
            //Arrange
            var nonExistentId = -1;

            //Act
            var response = await _httpClient.PutAsJsonAsync($"/api/Baseline/Update/{nonExistentId}", BaselineFactory.GetBaseline());

            //Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.Contains("not found", responseContent);
        }

        [Fact]
        public async Task DeleteBaseline_ShouldReturnGreaterThanZero()
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

            //Act
            var response = await _httpClient.DeleteAsync($"/api/Baseline/Delete/{baseline.Id}");

            //Assert
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            var updated = JsonConvert.DeserializeObject<int>(responseContent);

            Assert.NotEqual(0, updated);

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();
                var savedBaseline = await context.Baseline.FirstOrDefaultAsync(b => b.Id.Equals(baseline.Id));
                Assert.Null(savedBaseline);
            }
        }

        [Fact]
        public async Task DeleteBaseline_ShouldReturnNotFoundResult()
        {
            //Arrange
            var nonExistentId = -1;

            //Act
            var response = await _httpClient.DeleteAsync($"/api/Baseline/Delete/{nonExistentId}");

            //Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.Contains("not found", responseContent);
        }
    }


}

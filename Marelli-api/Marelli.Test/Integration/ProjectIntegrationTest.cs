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
    public class ProjectIntegrationTests
    {
        private readonly IntegrationSetupFixture _fixture;
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _httpClient;
        private readonly PostgreSqlContainer _postgreSqlContainer;
        public ProjectIntegrationTests(IntegrationSetupFixture fixture)
        {
            _fixture = fixture;
            _factory = fixture.Factory;
            _postgreSqlContainer = fixture.Container;
            _httpClient = fixture.HttpClient;

        }

        [Fact]
        public async Task SaveProject_ShouldReturnGreaterThanZero()
        {
            //Arrange
            var expectedProject = ProjectFactory.GetProject();
            var group = GroupFactory.GetGroup();
            var user = UserFactory.GetUser();

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();
                context.Group.Add(group);

                context.User.Add(user);
                await context.SaveChangesAsync();

                var userProject = new UserProject { ProjectId = 0, UserId = user.Id };
                expectedProject.UsersProject = new List<UserProject> { userProject };

                await context.SaveChangesAsync();
            }


            //Act
            var response = await _httpClient.PostAsJsonAsync("/api/Project/Save", expectedProject);

            //Assert
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            var saved = JsonConvert.DeserializeObject<int>(responseContent);

            Assert.NotEqual(0, saved);

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();
                var savedProject = await context.Project.FirstOrDefaultAsync(p => p.Name.Equals(expectedProject.Name));
                Assert.NotNull(savedProject);
                Assert.Equal(expectedProject.Name, savedProject.Name);
            }
        }

        [Fact]
        public async Task ListProjects_ShouldReturnProjectList()
        {
            //Arrange
            var expectedProject = ProjectFactory.GetProject();
            var user = UserFactory.GetUserWithoutGroup();
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();
                context.User.Add(user);
                await context.SaveChangesAsync();
                context.Project.Add(expectedProject);
                await context.SaveChangesAsync();
            }

            //Act
            var response = await _httpClient.GetAsync("/api/Project/List/1");

            //Assert
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            var projectList = JsonConvert.DeserializeObject<List<Project>>(responseContent);
            Assert.NotEmpty(projectList);

            var projectByName = projectList.Where(p => p.Name.Equals(expectedProject.Name)).FirstOrDefault();
            Assert.NotNull(projectByName);

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();
                var savedProject = await context.Project.FirstOrDefaultAsync(p => p.Id.Equals(expectedProject.Id));
                Assert.NotNull(savedProject);
                Assert.Equal(expectedProject.Name, savedProject.Name);
            }
        }

        [Fact]
        public async Task ListProjectsByName_ShouldReturnProjectList()
        {
            //Arrange
            var expectedProject = ProjectFactory.GetProject();
            var group = GroupFactory.GetGroup();

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();
                context.Group.Add(group);
                await context.SaveChangesAsync();

                expectedProject.Group = group;
                expectedProject.GroupId = group.Id;
                context.Project.Add(expectedProject);
                await context.SaveChangesAsync();
            }

            //Act
            var response = await _httpClient.GetAsync($"/api/Project/ListByName/{expectedProject.Name}");

            //Assert
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();

            var projectList = JsonConvert.DeserializeObject<List<Project>>(responseContent);
            Assert.NotEmpty(projectList);

            var projectByName = projectList.Where(p => p.Name.Equals(expectedProject.Name)).FirstOrDefault();
            Assert.NotNull(projectByName);

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();
                var savedProject = await context.Project.FirstOrDefaultAsync(p => p.Id.Equals(expectedProject.Id));
                Assert.NotNull(savedProject);
                Assert.Equal(expectedProject.Name, savedProject.Name);
            }
        }

        [Fact]
        public async Task UpdateProject_ShouldReturnGreaterThanZero()
        {
            //Arrange
            var group = GroupFactory.GetGroup();
            var user = UserFactory.GetUser();
            var currentProject = ProjectFactory.GetProject();

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();

                context.Group.Add(group);
                context.User.Add(user);
                context.Project.Add(currentProject);

                await context.SaveChangesAsync();

                currentProject.UsersProject = new List<UserProject> { };

                await context.SaveChangesAsync();
            }

            var userProject = new UserProject { ProjectId = 0, UserId = user.Id };

            var projectRequest = ProjectFactory.GetProject();
            projectRequest.Name = "updated project name";
            projectRequest.Group = group;
            projectRequest.GroupId = group.Id;
            projectRequest.UsersProject = new List<UserProject> { userProject };

            //Act
            var response = await _httpClient.PutAsJsonAsync($"/api/Project/Update/{currentProject.Id}", projectRequest);

            //Assert
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            var updated = JsonConvert.DeserializeObject<int>(responseContent);

            Assert.NotEqual(0, updated);

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();
                var updatedProject = await context.Project.FirstOrDefaultAsync(p => p.Id.Equals(currentProject.Id));
                Assert.NotNull(updatedProject);
                Assert.Equal(projectRequest.Name, updatedProject.Name);
                Assert.NotEqual(currentProject.Name, updatedProject.Name);
            }
        }

        [Fact]
        public async Task UpdateProject_ShouldThrowNotFoundException()
        {
            //Arrange
            var nonExistentId = -1;

            //Act
            var response = await _httpClient.PutAsJsonAsync($"/api/Project/Update/{nonExistentId}", ProjectFactory.GetProject());

            //Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.Contains("not found", responseContent);
        }

        [Fact]
        public async Task DeleteProject_ShouldReturnGreaterThanZero()
        {
            //Arrange
            var project = ProjectFactory.GetProject();
            var group = GroupFactory.GetGroup();

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();
                context.Group.Add(group);
                await context.SaveChangesAsync();

                project.Group = group;
                project.GroupId = group.Id;
                context.Project.Add(project);
                await context.SaveChangesAsync();
            }

            //Act
            var response = await _httpClient.DeleteAsync($"/api/Project/Delete/{project.Id}");

            //Assert
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            var updated = JsonConvert.DeserializeObject<int>(responseContent);

            Assert.NotEqual(0, updated);

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();
                var savedProject = await context.Project.FirstOrDefaultAsync(p => p.Id.Equals(project.Id));
                Assert.Null(savedProject);
            }
        }

        [Fact]
        public async Task DeleteProject_ShouldThrowNotFoundException()
        {
            //Arrange
            var nonExistentId = -1;

            //Act
            var response = await _httpClient.DeleteAsync($"/api/Project/Delete/{nonExistentId}");

            //Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.Contains("not found", responseContent);
        }
    }


}

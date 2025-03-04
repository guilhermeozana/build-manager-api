using Marelli.Domain.Dtos;
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
    public class GroupIntegrationTests
    {
        private readonly IntegrationSetupFixture _fixture;
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _httpClient;
        private readonly PostgreSqlContainer _postgreSqlContainer;
        public GroupIntegrationTests(IntegrationSetupFixture fixture)
        {
            _fixture = fixture;
            _factory = fixture.Factory;
            _postgreSqlContainer = fixture.Container;
            _httpClient = fixture.HttpClient;

        }

        [Fact]
        public async Task SaveGroup_ShouldReturnGreaterThanZero()
        {
            //Arrange
            var expectedGroup = GroupFactory.GetGroupRequest();
            var user = UserFactory.GetUser();

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();
                context.User.Add(user);

                await context.SaveChangesAsync();
            }

            expectedGroup.UserIds = new[] { user.Id };


            //Act
            var response = await _httpClient.PostAsJsonAsync("/api/Group/Save", expectedGroup);

            //Assert
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            var saved = JsonConvert.DeserializeObject<int>(responseContent);

            Assert.NotEqual(0, saved);

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();
                var savedGroup = await context.Group.FirstOrDefaultAsync(g => g.Name.Equals(expectedGroup.Name));
                Assert.NotNull(savedGroup);
                Assert.Equal(expectedGroup.Name, savedGroup.Name);
            }
        }

        [Fact]
        public async Task ListGroups_ShouldReturnGroupList()
        {
            //Arrange
            var expectedGroup = GroupFactory.GetGroup();
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();
                context.Group.Add(expectedGroup);
                await context.SaveChangesAsync();
            }

            //Act
            var response = await _httpClient.GetAsync("/api/Group/List");

            //Assert
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            var groupList = JsonConvert.DeserializeObject<List<GroupResponse>>(responseContent);
            Assert.NotEmpty(groupList);

            var groupByName = groupList.Where(g => g.Name.Equals(expectedGroup.Name)).FirstOrDefault();
            Assert.NotNull(groupByName);

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();
                var savedGroup = await context.Group.FirstOrDefaultAsync(g => g.Id.Equals(expectedGroup.Id));
                Assert.NotNull(savedGroup);
                Assert.Equal(expectedGroup.Name, savedGroup.Name);
            }
        }

        [Fact]
        public async Task ListGroupsByName_ShouldReturnGroupList()
        {
            //Arrange
            var expectedGroup = GroupFactory.GetGroup();

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();

                context.Group.Add(expectedGroup);
                await context.SaveChangesAsync();
            }

            //Act
            var response = await _httpClient.GetAsync($"/api/Group/List/{expectedGroup.Name}");

            //Assert
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();

            var groupList = JsonConvert.DeserializeObject<List<GroupResponse>>(responseContent);
            Assert.NotEmpty(groupList);

            var groupByName = groupList.Where(g => g.Name.Equals(expectedGroup.Name)).FirstOrDefault();
            Assert.NotNull(groupByName);

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();
                var savedGroup = await context.Group.FirstOrDefaultAsync(g => g.Id.Equals(expectedGroup.Id));
                Assert.NotNull(savedGroup);
                Assert.Equal(expectedGroup.Name, savedGroup.Name);
            }
        }

        [Fact]
        public async Task UpdateGroup_ShouldReturnGreaterThanZero()
        {
            //Arrange
            var currentGroup = GroupFactory.GetGroup();

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();

                context.Group.Add(currentGroup);

                await context.SaveChangesAsync();
            }

            var groupRequest = GroupFactory.GetGroupRequest();
            groupRequest.Id = currentGroup.Id;
            groupRequest.Name = "updated group name";

            //Act
            var response = await _httpClient.PutAsJsonAsync($"/api/Group/Update/{currentGroup.Id}", groupRequest);

            //Assert
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            var updated = JsonConvert.DeserializeObject<int>(responseContent);

            Assert.NotEqual(0, updated);

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();
                var updatedGroup = await context.Group.FirstOrDefaultAsync(g => g.Id.Equals(currentGroup.Id));
                Assert.NotNull(updatedGroup);
                Assert.Equal(groupRequest.Name, updatedGroup.Name);
                Assert.NotEqual(currentGroup.Name, updatedGroup.Name);
            }
        }

        [Fact]
        public async Task UpdateGroup_ShouldThrowNotFoundException()
        {
            //Arrange
            var nonExistentId = -1;

            //Act
            var response = await _httpClient.PutAsJsonAsync($"/api/Group/Update/{nonExistentId}", GroupFactory.GetGroupRequest());

            //Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.Contains("not found", responseContent);
        }

        [Fact]
        public async Task DeleteGroup_ShouldReturnGreaterThanZero()
        {
            //Arrange
            var group = GroupFactory.GetGroup();

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();
                context.Group.Add(group);

                await context.SaveChangesAsync();
            }

            //Act
            var response = await _httpClient.DeleteAsync($"/api/Group/Delete/{group.Id}");

            //Assert
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            var updated = JsonConvert.DeserializeObject<int>(responseContent);

            Assert.NotEqual(0, updated);

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();
                var savedGroup = await context.Group.FirstOrDefaultAsync(g => g.Id.Equals(group.Id));
                Assert.Null(savedGroup);
            }
        }

        [Fact]
        public async Task DeleteGroup_ShouldReturnNotFoundResult()
        {
            //Arrange
            var nonExistentId = -1;

            //Act
            var response = await _httpClient.DeleteAsync($"/api/Group/Delete/{nonExistentId}");

            //Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.Contains("not found", responseContent);
        }
    }


}

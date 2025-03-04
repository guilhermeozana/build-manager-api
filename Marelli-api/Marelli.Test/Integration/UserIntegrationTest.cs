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
    public class UserIntegrationTests
    {
        private readonly IntegrationSetupFixture _fixture;
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _httpClient;
        private readonly PostgreSqlContainer _postgreSqlContainer;
        public UserIntegrationTests(IntegrationSetupFixture fixture)
        {
            _fixture = fixture;
            _factory = fixture.Factory;
            _postgreSqlContainer = fixture.Container;
            _httpClient = fixture.HttpClient;
        }

        [Fact]
        public async Task SaveUser_ShouldReturnGreaterThanZero()
        {
            //Arrange
            var expectedUser = UserFactory.GetUserWithoutGroup();

            //Act
            var response = await _httpClient.PostAsJsonAsync("/api/User/Save", expectedUser);

            //Assert
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            var saved = JsonConvert.DeserializeObject<int>(responseContent);

            Assert.NotEqual(0, saved);

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();
                var savedUser = await context.User.FirstOrDefaultAsync(u => u.Name.Equals(expectedUser.Name));
                Assert.NotNull(savedUser);
                Assert.Equal(expectedUser.Name, savedUser.Name);
            }
        }

        [Fact]
        public async Task ListUsers_ShouldReturnUserList()
        {
            //Arrange
            var expectedUser = UserFactory.GetUserWithoutGroup();
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();
                context.User.Add(expectedUser);
                await context.SaveChangesAsync();
            }

            //Act
            var response = await _httpClient.GetAsync("/api/User/List");

            //Assert
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            var userList = JsonConvert.DeserializeObject<List<User>>(responseContent);
            Assert.NotEmpty(userList);

            var userByName = userList.Where(u => u.Name.Equals(expectedUser.Name)).FirstOrDefault();
            Assert.NotNull(userByName);

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();
                var savedUser = await context.User.FirstOrDefaultAsync(u => u.Id.Equals(expectedUser.Id));
                Assert.NotNull(savedUser);
                Assert.Equal(expectedUser.Name, savedUser.Name);
            }
        }

        [Fact]
        public async Task GetUserByEmail_ShouldReturnUserList()
        {
            //Arrange
            var expectedUser = UserFactory.GetUserWithoutGroup();
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();
                context.User.Add(expectedUser);
                await context.SaveChangesAsync();
            }

            //Act
            var response = await _httpClient.GetAsync($"/api/User/GetByEmail/{expectedUser.Email}");

            //Assert
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();

            var user = JsonConvert.DeserializeObject<User>(responseContent);
            Assert.NotNull(user);

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();
                var savedUser = await context.User.FirstOrDefaultAsync(u => u.Id.Equals(expectedUser.Id));
                Assert.NotNull(savedUser);
                Assert.Equal(expectedUser.Name, savedUser.Name);
            }
        }

        [Fact]
        public async Task UpdateUser_ShouldReturnGreaterThanZero()
        {
            //Arrange
            var currentUser = UserFactory.GetUserWithoutGroup();
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();
                context.User.Add(currentUser);
                await context.SaveChangesAsync();
            }

            var userRequest = UserFactory.GetUserWithoutGroup();
            userRequest.Name = "updated user name";

            //Act
            var response = await _httpClient.PutAsJsonAsync($"/api/User/Update/{currentUser.Id}", userRequest);

            //Assert
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            var updated = JsonConvert.DeserializeObject<int>(responseContent);

            Assert.NotEqual(0, updated);

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();
                var updatedUser = await context.User.FirstOrDefaultAsync(u => u.Id.Equals(currentUser.Id));
                Assert.NotNull(updatedUser);
                Assert.Equal(userRequest.Name, updatedUser.Name);
                Assert.NotEqual(currentUser.Name, updatedUser.Name);
            }
        }

        [Fact]
        public async Task UpdateUser_ShouldThrowNotFoundException()
        {
            //Arrange
            var nonExistentId = -1;

            //Act
            var response = await _httpClient.PutAsJsonAsync($"/api/User/Update/{nonExistentId}", UserFactory.GetUserWithoutGroup());

            //Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.Contains("not found", responseContent);
        }

        [Fact]
        public async Task DeleteUser_ShouldReturnGreaterThanZero()
        {
            //Arrange
            var user = UserFactory.GetUserWithoutGroup();
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();
                context.User.Add(user);
                await context.SaveChangesAsync();
            }

            //Act
            var response = await _httpClient.DeleteAsync($"/api/User/Delete/{user.Id}");

            //Assert
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            var updated = JsonConvert.DeserializeObject<int>(responseContent);

            Assert.NotEqual(0, updated);

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();
                var savedUser = await context.User.FirstOrDefaultAsync(u => u.Id.Equals(user.Id));
                Assert.Null(savedUser);
            }
        }

        [Fact]
        public async Task DeleteUser_ShouldThrowNotFoundException()
        {
            //Arrange
            var nonExistentId = -1;

            //Act
            var response = await _httpClient.DeleteAsync($"/api/User/Delete/{nonExistentId}");

            //Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.Contains("not found", responseContent);
        }
    }


}

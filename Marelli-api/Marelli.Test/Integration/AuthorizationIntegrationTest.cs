using Marelli.Business.Utils;
using Marelli.Domain.Dtos;
using Marelli.Infra.Context;
using Marelli.Test.Integration.Configuration;
using Marelli.Test.Utils.Factories;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System.Net;
using Testcontainers.PostgreSql;
using Xunit;


namespace Marelli.Test.Integration
{
    [Collection("Integration collection")]
    public class AuthorizationIntegrationTest
    {
        private readonly IntegrationSetupFixture _fixture;
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _httpClient;
        private readonly PostgreSqlContainer _postgreSqlContainer;
        public AuthorizationIntegrationTest(IntegrationSetupFixture fixture)
        {
            _fixture = fixture;
            _factory = fixture.Factory;
            _postgreSqlContainer = fixture.Container;
            _httpClient = _factory.CreateClient();
        }

        [Fact]
        public async Task GenerateToken_ShouldReturnTokenResponseInstance()
        {
            //Arrange
            var user = UserFactory.GetUserWithoutGroup();
            user.Password = EncryptionUtils.HashPassword(user.Password);

            var unhashedPassword = UserFactory.GetUserWithoutGroup().Password;

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();
                var savedNews = context.User.Add(user);
                await context.SaveChangesAsync();
            }

            //Act
            var response = await _httpClient.GetAsync($"/api/Authorization/GetToken/{user.Email}/{unhashedPassword}");

            //Assert
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<TokenResponse>(responseContent);

            Assert.NotNull(result);
            Assert.IsType<TokenResponse>(result);
        }

        [Fact]
        public async Task GenerateToken_ShouldReturnUnauthorizedResult_WhenUserNotFound()
        {
            //Arrange
            var nonExistentEmail = Guid.NewGuid().ToString();

            //Act
            var response = await _httpClient.GetAsync($"/api/Authorization/GetToken/{nonExistentEmail}/test");

            //Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.Contains("Invalid email or password", responseContent);
        }

        [Fact]
        public async Task GenerateToken_ShouldReturnUnauthorizedResult_WhenPasswordIsIncorrect()
        {
            //Arrange
            var user = UserFactory.GetUserWithoutGroup();
            user.Password = EncryptionUtils.HashPassword(user.Password);

            var incorrectPassword = "incorrect-password";

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();
                var savedNews = context.User.Add(user);
                await context.SaveChangesAsync();
            }

            //Act
            var response = await _httpClient.GetAsync($"/api/Authorization/GetToken/{user.Email}/{incorrectPassword}");

            //Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.Contains("Invalid email or password", responseContent);
        }
    }


}

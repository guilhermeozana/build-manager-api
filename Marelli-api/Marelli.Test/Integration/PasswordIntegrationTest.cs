using Marelli.Infra.Context;
using Marelli.Test.Integration.Configuration;
using Marelli.Test.Utils.Factories;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using Xunit;


namespace Marelli.Test.Integration
{
    [Collection("Integration collection")]
    public class PasswordIntegrationTests
    {
        private readonly IntegrationSetupFixture _fixture;
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _httpClient;
        public PasswordIntegrationTests(IntegrationSetupFixture fixture)
        {
            _fixture = fixture;
            _factory = fixture.Factory;
            _httpClient = _factory.CreateClient();
        }

        [Fact]
        public async Task ForgotPassword_ShouldReturnOkResultMessage()
        {
            //Arrange
            var user = UserFactory.GetUserWithoutGroup();

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();
                context.User.Add(user);
                await context.SaveChangesAsync();
            }

            var forgotPasswordRequest = PasswordFactory.GetForgotPasswordRequest();
            forgotPasswordRequest.Email = user.Email;

            //Act
            var response = await _httpClient.PostAsJsonAsync("/api/Password/ForgotPassword", forgotPasswordRequest);

            //Assert
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();

            Assert.Equal("Password reset email sent.", responseContent);
        }

        [Fact]
        public async Task ResetPassword_ShouldReturnOkResultMessage()
        {
            //Arrange
            var user = UserFactory.GetUserWithoutGroup();

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();

                context.User.Add(user);
                await context.SaveChangesAsync();
            }

            var resetPasswordRequest = PasswordFactory.GetResetPasswordRequest();
            resetPasswordRequest.Token = _fixture.TestToken;
            resetPasswordRequest.Email = user.Email;

            //Act
            var response = await _httpClient.PostAsJsonAsync("/api/Password/ResetPassword", resetPasswordRequest);

            //Assert
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();

            Assert.Equal("Password redefined successfully.", responseContent);
        }

        [Fact]
        public async Task ResetPassword_ShouldReturnBadRequestResult_WhenInvalidToken()
        {
            //Arrange
            var resetPasswordRequest = PasswordFactory.GetResetPasswordRequest();
            resetPasswordRequest.Token = "invalid-token";
            resetPasswordRequest.Email = "test@test.com";

            //Act
            var response = await _httpClient.PostAsJsonAsync("/api/Password/ResetPassword", resetPasswordRequest);

            //Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.Contains("JWT", responseContent);
        }

        [Fact]
        public async Task ResetPassword_ShouldReturnInternalServerErrorResult_WhenInvalidPassword()
        {
            //Arrange
            var user = UserFactory.GetUserWithoutGroup();

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();

                context.User.Add(user);
                await context.SaveChangesAsync();
            }

            var resetPasswordRequest = PasswordFactory.GetResetPasswordRequest();
            resetPasswordRequest.Token = _fixture.TestToken;
            resetPasswordRequest.NewPassword = "invalid-password";
            resetPasswordRequest.Email = user.Email;

            //Act
            var response = await _httpClient.PostAsJsonAsync("/api/Password/ResetPassword", resetPasswordRequest);

            //Assert
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.Contains("The password must have", responseContent);

        }

        [Fact]
        public async Task ResetPassword_ShouldReturnNotFoundResult_WhenUserNotFound()
        {
            //Arrange
            var resetPasswordRequest = PasswordFactory.GetResetPasswordRequest();
            resetPasswordRequest.Token = _fixture.TestToken;
            resetPasswordRequest.Email = "incorrect-email@test.com";

            //Act
            var response = await _httpClient.PostAsJsonAsync("/api/Password/ResetPassword", resetPasswordRequest);

            //Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.Contains("not found", responseContent);

        }
    }
}

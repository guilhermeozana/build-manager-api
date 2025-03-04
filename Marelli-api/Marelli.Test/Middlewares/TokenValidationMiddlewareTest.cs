using Marelli.Api.Middlewares;
using Marelli.Business.IServices;
using Marelli.Test.Utils.Factories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Net;
using Xunit;

namespace Marelli.Test.Middlewares
{


    public class TokenValidationMiddlewareTests
    {
        [Fact]
        public async Task Invoke_ValidToken_ShouldSetUserInContext()
        {
            // Arrange
            var token = TokenFactory.GetValidToken();

            var userTokenServiceMock = new Mock<IUserTokenService>();
            userTokenServiceMock.Setup(uts => uts.VerifyUserTokenValid(token))
                .ReturnsAsync(true);

            var server = new TestServer(new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddScoped<IUserTokenService>(sp => userTokenServiceMock.Object);
                    services.AddHttpContextAccessor();
                })
                .Configure(app =>
                {
                    app.UseMiddleware<TokenValidationMiddleware>();

                    app.Run(async context =>
                    {
                        context.Response.StatusCode = 200;
                        await context.Response.WriteAsync("Request Passed!");
                    });

                }));

            var client = server.CreateClient();
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, "/");
            requestMessage.Headers.Add("Authorization", $"Bearer {token}");

            // Act
            var response = await client.SendAsync(requestMessage);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Invoke_InvalidToken_ShouldReturnUnauthorized()
        {
            // Arrange
            var token = "invalid.jwt.token";

            var userTokenServiceMock = new Mock<IUserTokenService>();
            userTokenServiceMock.Setup(uts => uts.VerifyUserTokenValid(token))
                .ReturnsAsync(false);

            var server = new TestServer(new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddScoped<IUserTokenService>(sp => userTokenServiceMock.Object);
                })
                .Configure(app =>
                {
                    app.UseMiddleware<TokenValidationMiddleware>();
                }));

            var client = server.CreateClient();
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, "/");
            requestMessage.Headers.Add("Authorization", $"Bearer {token}");

            // Act
            var response = await client.SendAsync(requestMessage);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            var responseBody = await response.Content.ReadAsStringAsync();
            Assert.Contains("Invalid token.", responseBody);
        }
    }


}

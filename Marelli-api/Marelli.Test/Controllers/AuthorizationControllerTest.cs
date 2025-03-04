using Marelli.Api.Controllers;
using Marelli.Business.IServices;
using Marelli.Domain.Dtos;
using Marelli.Test.Utils.Factories;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Marelli.Test.Controllers
{
    public class AuthorizationControllerTest
    {
        private readonly Mock<IAuthorizationService> _authorizationServiceMock;
        private readonly AuthorizationController _authorizationController;

        public AuthorizationControllerTest()
        {
            _authorizationServiceMock = new Mock<IAuthorizationService>();

            _authorizationController = new AuthorizationController(_authorizationServiceMock.Object);
        }

        [Fact]
        public async Task GenerateToken_ShouldReturnOkResult()
        {
            var tokenResponse = TokenFactory.GetTokenResponse();

            _authorizationServiceMock.Setup(a => a.GenerateToken(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(tokenResponse);

            var result = await _authorizationController.GenerateToken("test", "test");

            var okResult = Assert.IsType<OkObjectResult>(result);

            Assert.NotNull(okResult.Value);

            var okResultValue = Assert.IsType<TokenResponse>(okResult.Value);

            Assert.Equal(tokenResponse.AccessToken, okResultValue.AccessToken);
        }

    }
}

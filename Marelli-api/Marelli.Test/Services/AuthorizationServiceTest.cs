using Marelli.Business.Exceptions;
using Marelli.Business.IServices;
using Marelli.Business.Services;
using Marelli.Business.Utils;
using Marelli.Domain.Dtos;
using Marelli.Domain.Entities;
using Marelli.Test.Utils.Factories;
using Moq;
using Xunit;

namespace Marelli.Test.Services
{
    public class AuthorizationServiceTest
    {
        private readonly Mock<IUserService> _userServiceMock;
        private readonly Mock<ITokenService> _tokenServiceMock;
        private readonly AuthorizationService _authorizationService;

        public AuthorizationServiceTest()
        {
            _userServiceMock = new Mock<IUserService>();
            _tokenServiceMock = new Mock<ITokenService>();

            _authorizationService = new AuthorizationService(_userServiceMock.Object, _tokenServiceMock.Object);
        }

        [Fact]
        public async Task GenerateToken_ShouldReturnTokenResponse()
        {
            var user = UserFactory.GetUser();
            user.Password = EncryptionUtils.HashPassword(user.Password);

            _userServiceMock.Setup(u => u.GetUserWithPassword(It.IsAny<string>())).ReturnsAsync(user);
            _tokenServiceMock.Setup(t => t.GenerateAccessToken(It.IsAny<User>())).ReturnsAsync(TokenFactory.GetTokenResponse());

            var tokenResponse = await _authorizationService.GenerateToken("test@email.com", "password@123A");

            Assert.NotNull(tokenResponse);
            Assert.IsType<TokenResponse>(tokenResponse);
            Assert.Equal(tokenResponse.ExpiresIn, tokenResponse.ExpiresIn);
        }

        [Fact]
        public async Task GenerateToken_ShouldThrowUnauthorizedAccessException_WhenUserNotFound()
        {
            var user = UserFactory.GetUser();
            user.Password = EncryptionUtils.HashPassword(user.Password);

            _userServiceMock.Setup(u => u.GetUserByEmail(It.IsAny<string>())).ThrowsAsync(new NotFoundException());

            await Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await _authorizationService.GenerateToken("test@email.com", "password123A"));
        }

        [Fact]
        public async Task GenerateToken_ShouldThrowUnauthorizedAccessException_WhenPasswordIsInvalid()
        {
            var user = UserFactory.GetUser();
            user.Password = EncryptionUtils.HashPassword(user.Password);

            _userServiceMock.Setup(u => u.GetUserByEmail(It.IsAny<string>())).ReturnsAsync(user);
            _tokenServiceMock.Setup(t => t.GenerateAccessToken(It.IsAny<User>())).ReturnsAsync(TokenFactory.GetTokenResponse());

            await Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await _authorizationService.GenerateToken("test@email.com", "incorrect-password"));
        }

    }
}

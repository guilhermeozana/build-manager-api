using Marelli.Business.Services;
using Marelli.Domain.Entities;
using Marelli.Infra.IRepositories;
using Marelli.Test.Utils.Factories;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace Marelli.Test.Services
{
    public class UserTokenServiceTest
    {

        private readonly Mock<IUserTokenRepository> _userTokenRepositoryMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly UserTokenService _userTokenService;

        public UserTokenServiceTest()
        {
            _userTokenRepositoryMock = new Mock<IUserTokenRepository>();
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();

            _userTokenService = new UserTokenService(_userTokenRepositoryMock.Object, _httpContextAccessorMock.Object);
        }

        [Fact]
        public async Task SaveUserToken_ShouldReturnGreaterThanZero()
        {
            _userTokenRepositoryMock.Setup(n => n.SaveUserToken(It.IsAny<UserToken>())).ReturnsAsync(1);

            var res = await _userTokenService.SaveUserToken(UserTokenFactory.GetUserToken());

            Assert.Equal(1, res);
        }

        [Fact]
        public async Task VerifyUserTokenValid_ShouldReturnTokenIsValidBoolean()
        {
            _userTokenRepositoryMock.Setup(n => n.VerifyUserTokenValid(It.IsAny<string>())).ReturnsAsync(true);

            var res = await _userTokenService.VerifyUserTokenValid("my-token");

            Assert.True(res);
        }

        [Fact]
        public async Task RevokeUserTokens_ShouldClearTokensForUser()
        {
            _userTokenRepositoryMock.Setup(u => u.RevokeUserTokens(It.IsAny<int>())).Verifiable();

            await _userTokenService.RevokeUserTokens(1);

            _userTokenRepositoryMock.Verify(u => u.RevokeUserTokens(It.IsAny<int>()), Times.Once());
        }

        [Fact]
        public async Task RevokeUserTokensExceptCurrent_ShouldClearTokensForUser()
        {
            _httpContextAccessorMock.Setup(h => h.HttpContext.Request.Headers["Authorization"]).Returns("my-token");
            _userTokenRepositoryMock.Setup(u => u.RevokeUserTokensExceptCurrent(It.IsAny<int>(), It.IsAny<string>())).Verifiable();

            await _userTokenService.RevokeUserTokensExceptCurrent(1);

            _userTokenRepositoryMock.Verify(u => u.RevokeUserTokensExceptCurrent(It.IsAny<int>(), It.IsAny<string>()), Times.Once);
        }

    }
}
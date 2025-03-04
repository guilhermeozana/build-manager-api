using Marelli.Business.IServices;
using Marelli.Business.Services;
using Marelli.Domain.Dtos;
using Marelli.Domain.Entities;
using Marelli.Infra.Configuration;
using Marelli.Test.Utils.Factories;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Moq;
using System.Security.Claims;
using Xunit;

namespace Marelli.Test.Services
{
    public class TokenServiceTest
    {
        private readonly Mock<IOptions<AppSettings>> _appSettingsMock;
        private readonly Mock<IUserTokenService> _userTokenServiceMock;

        private readonly TokenService _tokenService;

        public TokenServiceTest()
        {
            _appSettingsMock = new Mock<IOptions<AppSettings>>();
            _userTokenServiceMock = new Mock<IUserTokenService>();

            _appSettingsMock.Setup(o => o.Value).Returns(new AppSettings
            {
                Secret = "my-fake-secret-key-asdasdasdasdadasdas",
                HoursExpiration = 24
            });

            _tokenService = new TokenService(_appSettingsMock.Object, _userTokenServiceMock.Object);
        }


        [Fact]
        public async Task GenerateAccessToken_ShouldReturnTokenResponseInstance()
        {
            _userTokenServiceMock.Setup(u => u.SaveUserToken(It.IsAny<UserToken>())).ReturnsAsync(1);

            var res = await _tokenService.GenerateAccessToken(UserFactory.GetUser());

            Assert.NotNull(res);
            Assert.IsType<TokenResponse>(res);
            Assert.Equal(TokenFactory.GetTokenResponse().ExpiresIn, res.ExpiresIn);
        }


        [Fact]
        public async Task GeneratePasswordResetToken_ShouldReturnTokenString()
        {
            var res = _tokenService.GeneratePasswordResetToken();

            Assert.NotNull(res);
            Assert.IsType<string>(res);
            Assert.NotEqual(0, res.Length);
        }

        [Fact]
        public async Task GetPrincipalFromExpiredToken_ShouldReturnClaimsPrincipalInstance()
        {
            var res = _tokenService.GetPrincipalFromExpiredToken(TokenFactory.GetValidToken());

            Assert.NotNull(res);
            Assert.IsType<ClaimsPrincipal>(res);
        }

        [Fact]
        public async Task GetPrincipalFromExpiredToken_ShouldThrowArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _tokenService.GetPrincipalFromExpiredToken(""));
        }

        [Fact]
        public async Task GetPrincipalFromExpiredToken_ShouldThrowArgumentException()
        {
            Assert.Throws<ArgumentException>(() => _tokenService.GetPrincipalFromExpiredToken("invalid-token"));
        }

        [Fact]
        public async Task GetPrincipalFromExpiredToken_ShouldThrowSecurityTokenInvalidSignatureException()
        {
            Assert.Throws<SecurityTokenInvalidSignatureException>(() => _tokenService.GetPrincipalFromExpiredToken(TokenFactory.GetInvalidToken()));
        }

    }
}
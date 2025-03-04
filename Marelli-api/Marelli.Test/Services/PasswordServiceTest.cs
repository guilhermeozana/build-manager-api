using Marelli.Business.Exceptions;
using Marelli.Business.IServices;
using Marelli.Business.Services;
using Marelli.Infra.IRepositories;
using Marelli.Test.Utils.Factories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace Marelli.Test.Services
{
    public class PasswordServiceTest
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly Mock<IUrlHelper> _urlHelperMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<ITokenService> _tokenServiceMock;
        private readonly PasswordService _passwordService;

        public PasswordServiceTest()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _emailServiceMock = new Mock<IEmailService>();
            _urlHelperMock = new Mock<IUrlHelper>();
            _configurationMock = new Mock<IConfiguration>();
            _tokenServiceMock = new Mock<ITokenService>();

            _passwordService = new PasswordService(_userRepositoryMock.Object, _emailServiceMock.Object, _configurationMock.Object, _tokenServiceMock.Object);
        }

        [Fact]
        public async Task ForgotPassword_ShouldSendResetPasswordEmail()
        {
            _userRepositoryMock.Setup(u => u.GetUserByEmail(It.IsAny<string>())).ReturnsAsync(UserFactory.GetUser());
            _tokenServiceMock.Setup(t => t.GeneratePasswordResetToken()).Returns(TokenFactory.GetValidToken());
            _emailServiceMock.Setup(e => e.SendEmail(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));

            await _passwordService.ForgotPassword(PasswordFactory.GetForgotPasswordRequest());

            _emailServiceMock.Verify(e => e.SendEmail(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task ResetPassword_ShouldUpdateUserPassword()
        {
            _userRepositoryMock.Setup(u => u.UpdateUserPassword(It.IsAny<string>(), It.IsAny<string>()));
            _userRepositoryMock.Setup(u => u.GetUserByEmail(It.IsAny<string>())).ReturnsAsync(UserFactory.GetUser());

            await _passwordService.ResetPassword(PasswordFactory.GetResetPasswordRequest());

            _userRepositoryMock.Verify(u => u.UpdateUserPassword(It.IsAny<string>(), It.IsAny<string>()), Times.Once());
        }

        [Fact]
        public async Task ResetPassword_ShouldThrowInvalidPasswordException()
        {
            _userRepositoryMock.Setup(u => u.UpdateUserPassword(It.IsAny<string>(), It.IsAny<string>())).ThrowsAsync(new InvalidPasswordException("invalid password"));

            var resetPasswordRequest = PasswordFactory.GetResetPasswordRequest();
            resetPasswordRequest.NewPassword = "invalid-password";

            await Assert.ThrowsAsync<InvalidPasswordException>(async () => await _passwordService.ResetPassword(resetPasswordRequest));
        }
    }
}

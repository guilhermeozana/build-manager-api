using Marelli.Api.Controllers;
using Marelli.Business.IServices;
using Marelli.Domain.Dtos;
using Marelli.Test.Utils.Factories;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Marelli.Test.Controllers
{
    public class PasswordControllerTest
    {
        private readonly Mock<IPasswordService> _passwordServiceMock;
        private readonly PasswordController _passwordController;

        public PasswordControllerTest()
        {
            _passwordServiceMock = new Mock<IPasswordService>();

            _passwordController = new PasswordController(_passwordServiceMock.Object);
        }

        [Fact]
        public async Task ForgotPassword_ShouldOkResult()
        {

            _passwordServiceMock.Setup(p => p.ForgotPassword(It.IsAny<ForgotPasswordRequest>()));

            var result = await _passwordController.ForgotPassword(PasswordFactory.GetForgotPasswordRequest());

            var okResult = Assert.IsType<OkObjectResult>(result);

            Assert.NotNull(okResult.Value);

            var okResultValue = Assert.IsType<string>(okResult.Value);

            Assert.Equal("Password reset email sent.", okResultValue);
        }

        [Fact]
        public async Task ResetPassword_ShouldOkResult()
        {

            _passwordServiceMock.Setup(p => p.ResetPassword(It.IsAny<ResetPasswordRequest>()));

            var result = await _passwordController.ResetPassword(PasswordFactory.GetResetPasswordRequest());

            var okResult = Assert.IsType<OkObjectResult>(result);

            Assert.NotNull(okResult.Value);

            var okResultValue = Assert.IsType<string>(okResult.Value);

            Assert.Equal("Password redefined successfully.", okResultValue);
        }

    }
}

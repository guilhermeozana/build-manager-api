using Marelli.Api.Controllers;
using Marelli.Business.IServices;
using Marelli.Test.Utils.Factories;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Marelli.Test.Controllers
{
    public class EmailControllerTest
    {
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly EmailController _emailController;

        public EmailControllerTest()
        {
            _emailServiceMock = new Mock<IEmailService>();

            _emailController = new EmailController(_emailServiceMock.Object);
        }

        [Fact]
        public async Task SendEmail_ShouldReturnOkResult()
        {
            var buildTable = BuildTableRowFactory.GetBuildTable();

            _emailServiceMock.Setup(e => e.SendEmail(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));

            var result = await _emailController.SendEmail("test", "test", "test");

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);

            var okResultValue = Assert.IsType<string>(okResult.Value);
            Assert.Contains("Email has been sent successfully", okResultValue);
        }

    }
}

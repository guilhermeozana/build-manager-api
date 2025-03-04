using Marelli.Api.Controllers;
using Marelli.Business.IServices;
using Marelli.Domain.Entities;
using Marelli.Test.Utils.Factories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using Xunit;

namespace Marelli.Test.Controllers
{
    public class BaselineControllerTest
    {
        private readonly Mock<IBaselineService> _baselineServiceMock;
        private readonly BaselineController _baselineController;

        public BaselineControllerTest()
        {
            _baselineServiceMock = new Mock<IBaselineService>();

            _baselineController = new BaselineController(_baselineServiceMock.Object);
        }

        [Fact]
        public async Task SaveBaseline_ShouldReturnOkResult()
        {
            //Arrange
            var mockRole = "Administrator";
            var claims = new List<Claim>
            {
                new Claim("role", mockRole)
            };

            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _baselineController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            var baseline = BaselineFactory.GetBaseline();

            _baselineServiceMock.Setup(n => n.SaveBaseline(baseline)).ReturnsAsync(baseline);

            //Act
            var result = await _baselineController.SaveBaseline(baseline);


            //Assert
            var okResult = Assert.IsType<OkObjectResult>(result);

            Assert.NotNull(okResult.Value);

            var okResultValue = Assert.IsType<Baseline>(okResult.Value);

            Assert.Equal(baseline.FileName, okResultValue.FileName);
        }

        [Fact]
        public async Task SaveBaseline_ShouldReturnUnauthorizedResult()
        {
            //Arrange
            var mockRole = "Developer";
            var claims = new List<Claim>
            {
                new Claim("role", mockRole)
            };

            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _baselineController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            var baseline = BaselineFactory.GetBaseline();

            _baselineServiceMock.Setup(n => n.SaveBaseline(baseline)).ReturnsAsync(baseline);

            //Act
            var result = await _baselineController.SaveBaseline(baseline);


            //Assert
            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task ListBaseline_ShouldReturnOkResult()
        {
            var baseline = BaselineFactory.GetBaseline();

            _baselineServiceMock.Setup(n => n.ListBaseline(It.IsAny<int>())).ReturnsAsync(new List<Baseline> { baseline });

            var result = await _baselineController.ListBaseline(1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);

            var okResultValue = Assert.IsType<List<Baseline>>(okResult.Value);
            Assert.Equal(baseline.FileName, okResultValue.First().FileName);
        }


        [Fact]
        public async Task GetBaseline_ShouldReturnOkResult()
        {
            var baseline = BaselineFactory.GetBaseline();

            _baselineServiceMock.Setup(n => n.GetBaseline(It.IsAny<int>())).ReturnsAsync(baseline);

            var result = await _baselineController.GetBaseline(1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);

            var okResultValue = Assert.IsType<Baseline>(okResult.Value);
            Assert.Equal(baseline.FileName, okResultValue.FileName);
        }


        [Fact]
        public async Task UpdateBaseline_ShouldReturnReturnOkResult()
        {
            //Arrange
            var mockRole = "Administrator";
            var claims = new List<Claim>
            {
                new Claim("role", mockRole)
            };

            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _baselineController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            var baseline = BaselineFactory.GetBaseline();

            _baselineServiceMock.Setup(n => n.UpdateBaseline(It.IsAny<int>(), It.IsAny<Baseline>())).ReturnsAsync(1);

            //Act
            var result = await _baselineController.UpdateBaseline(baseline.Id, baseline);

            //Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);

            var okResultValue = Assert.IsType<int>(okResult.Value);
            Assert.NotEqual(0, okResultValue);
        }

        [Fact]
        public async Task UpdateBaseline_ShouldReturnReturnUnauthorizedResult()
        {
            //Arrange
            var mockRole = "Developer";
            var claims = new List<Claim>
            {
                new Claim("role", mockRole)
            };

            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _baselineController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            var baseline = BaselineFactory.GetBaseline();

            _baselineServiceMock.Setup(n => n.UpdateBaseline(It.IsAny<int>(), It.IsAny<Baseline>())).ReturnsAsync(1);

            //Act
            var result = await _baselineController.UpdateBaseline(baseline.Id, baseline);

            //Assert
            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task DeleteBaseline_ShouldReturnOkResult()
        {
            var mockRole = "Administrator";
            var claims = new List<Claim>
            {
                new Claim("role", mockRole)
            };

            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _baselineController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            _baselineServiceMock.Setup(n => n.DeleteBaseline(It.IsAny<int>())).ReturnsAsync(1);

            var result = await _baselineController.DeleteBaseline(1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);

            var okResultValue = Assert.IsType<int>(okResult.Value);
            Assert.NotEqual(0, okResultValue);
        }

        [Fact]
        public async Task DeleteBaseline_ShouldReturnUnauthorizedResult()
        {
            var mockRole = "Developer";
            var claims = new List<Claim>
            {
                new Claim("role", mockRole)
            };

            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _baselineController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            _baselineServiceMock.Setup(n => n.DeleteBaseline(It.IsAny<int>())).ReturnsAsync(1);

            var result = await _baselineController.DeleteBaseline(1);

            Assert.IsType<UnauthorizedObjectResult>(result);
        }
    }
}

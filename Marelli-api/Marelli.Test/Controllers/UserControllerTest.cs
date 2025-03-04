using Marelli.Api.Controllers;
using Marelli.Business.IServices;
using Marelli.Domain.Dtos;
using Marelli.Domain.Entities;
using Marelli.Test.Utils.Factories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using Xunit;

namespace Marelli.Test.Controllers
{
    public class UserControllerTest
    {
        private readonly Mock<IUserService> _userServiceMock;
        private readonly UserController _userController;

        public UserControllerTest()
        {
            _userServiceMock = new Mock<IUserService>();

            _userController = new UserController(_userServiceMock.Object);
        }

        [Fact]
        public async Task SaveUser_ShouldReturnOkResult()
        {
            var mockRole = "Administrator";
            var claims = new List<Claim>
            {
                new Claim("role", mockRole)
            };

            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _userController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            var user = UserFactory.GetUser();

            _userServiceMock.Setup(u => u.SaveUser(user)).ReturnsAsync(1);

            var result = await _userController.SaveUser(user);

            var okResult = Assert.IsType<OkObjectResult>(result);

            Assert.NotNull(okResult.Value);

            var okResultValue = Assert.IsType<int>(okResult.Value);

            Assert.NotEqual(0, okResultValue);
        }

        [Fact]
        public async Task SaveUser_ShouldReturnUnauthorizedResult()
        {
            var mockRole = "Unauthorized";
            var claims = new List<Claim>
            {
                new Claim("role", mockRole)
            };

            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _userController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            var user = UserFactory.GetUser();

            _userServiceMock.Setup(u => u.SaveUser(user)).ReturnsAsync(1);

            var result = await _userController.SaveUser(user);

            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task ListUsers_ShouldReturnOkResult()
        {
            var user = UserFactory.GetUserResponse();

            _userServiceMock.Setup(u => u.ListUsers()).ReturnsAsync(new List<UserResponse> { user });

            var result = await _userController.ListUsers();

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);

            var okResultValue = Assert.IsType<List<UserResponse>>(okResult.Value);
            Assert.Equal(user.Name, okResultValue.First().Name);
        }

        [Fact]
        public async Task UpdateUser_ShouldReturnReturnOkResult()
        {
            var mockRole = "Administrator";
            var claims = new List<Claim>
            {
                new Claim("role", mockRole)
            };

            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _userController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            var user = UserFactory.GetUser();

            _userServiceMock.Setup(u => u.UpdateUser(It.IsAny<int>(), It.IsAny<User>())).ReturnsAsync(1);

            var result = await _userController.UpdateUser(user.Id, user);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);

            var okResultValue = Assert.IsType<int>(okResult.Value);
            Assert.NotEqual(0, okResultValue);
        }

        [Fact]
        public async Task UpdateUser_ShouldReturnReturnUnauthorizedResult()
        {
            var mockRole = "Unauthorized";
            var claims = new List<Claim>
            {
                new Claim("role", mockRole)
            };

            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _userController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            var user = UserFactory.GetUser();

            _userServiceMock.Setup(u => u.UpdateUser(It.IsAny<int>(), It.IsAny<User>())).ReturnsAsync(1);

            var result = await _userController.UpdateUser(user.Id, user);

            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task DeleteUser_ShouldReturnReturnOkResult()
        {
            var mockRole = "Administrator";
            var claims = new List<Claim>
            {
                new Claim("role", mockRole)
            };

            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _userController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            _userServiceMock.Setup(u => u.DeleteUser(It.IsAny<int>())).ReturnsAsync(1);

            var result = await _userController.DeleteUser(1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);

            var okResultValue = Assert.IsType<int>(okResult.Value);
            Assert.NotEqual(0, okResultValue);
        }

        [Fact]
        public async Task DeleteUser_ShouldReturnReturnUnauthorizedResult()
        {
            var mockRole = "Unauthorized";
            var claims = new List<Claim>
            {
                new Claim("role", mockRole)
            };

            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _userController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            _userServiceMock.Setup(u => u.DeleteUser(It.IsAny<int>())).ReturnsAsync(1);

            var result = await _userController.DeleteUser(1);

            Assert.IsType<UnauthorizedObjectResult>(result);
        }
    }
}

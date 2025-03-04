using Marelli.Api.Controllers;
using Marelli.Business.IServices;
using Marelli.Domain.Dtos;
using Marelli.Test.Utils.Factories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using Xunit;

namespace Marelli.Test.Controllers
{
    public class GroupControllerTest
    {
        private readonly Mock<IGroupService> _groupServiceMock;
        private readonly GroupController _groupController;

        public GroupControllerTest()
        {
            _groupServiceMock = new Mock<IGroupService>();

            _groupController = new GroupController(_groupServiceMock.Object);
        }

        [Fact]
        public async Task SaveGroup_ShouldReturnOkResult()
        {
            var mockRole = "Administrator";
            var claims = new List<Claim>
            {
                new Claim("role", mockRole)
            };

            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _groupController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            var groupRequest = GroupFactory.GetGroupRequest();

            _groupServiceMock.Setup(g => g.SaveGroup(groupRequest)).ReturnsAsync(1);

            var result = await _groupController.SaveGroup(groupRequest);

            var okResult = Assert.IsType<OkObjectResult>(result);

            Assert.NotNull(okResult.Value);

            var okResultValue = Assert.IsType<int>(okResult.Value);

            Assert.NotEqual(0, okResultValue);
        }

        [Fact]
        public async Task SaveGroup_ShouldReturnUnauthorizedResult()
        {
            var mockRole = "Unauthorized";
            var claims = new List<Claim>
            {
                new Claim("role", mockRole)
            };

            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _groupController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            var groupRequest = GroupFactory.GetGroupRequest();

            _groupServiceMock.Setup(g => g.SaveGroup(groupRequest)).ReturnsAsync(1);

            var result = await _groupController.SaveGroup(groupRequest);

            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task ListGroups_ShouldReturnOkResult()
        {
            var group = GroupFactory.GetGroupResponse();

            _groupServiceMock.Setup(g => g.ListGroups()).ReturnsAsync(new List<GroupResponse> { group });

            var result = await _groupController.ListGroups();

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);

            var okResultValue = Assert.IsType<List<GroupResponse>>(okResult.Value);
            Assert.Equal(group.Name, okResultValue.First().Name);
        }


        [Fact]
        public async Task ListGroupsByName_ShouldReturnOkResult()
        {
            var group = GroupFactory.GetGroupResponse();

            _groupServiceMock.Setup(g => g.ListGroups()).ReturnsAsync(new List<GroupResponse> { group });

            var result = await _groupController.ListGroupsByName(group.Name);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);

            var okResultValue = Assert.IsType<List<GroupResponse>>(okResult.Value);
            Assert.Equal(group.Name, okResultValue.First().Name);
        }


        [Fact]
        public async Task UpdateGroup_ShouldReturnReturnOkResult()
        {
            var mockRole = "Administrator";
            var claims = new List<Claim>
            {
                new Claim("role", mockRole)
            };

            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _groupController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            var groupRequest = GroupFactory.GetGroupRequest();

            _groupServiceMock.Setup(g => g.UpdateGroup(It.IsAny<int>(), It.IsAny<GroupRequest>())).ReturnsAsync(1);

            var result = await _groupController.UpdateGroup(groupRequest.Id, groupRequest);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);

            var okResultValue = Assert.IsType<int>(okResult.Value);
            Assert.NotEqual(0, okResultValue);
        }

        [Fact]
        public async Task UpdateGroup_ShouldReturnReturnUnauthorizedResult()
        {
            var mockRole = "Unauthorized";
            var claims = new List<Claim>
            {
                new Claim("role", mockRole)
            };

            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _groupController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            var groupRequest = GroupFactory.GetGroupRequest();

            _groupServiceMock.Setup(g => g.UpdateGroup(It.IsAny<int>(), It.IsAny<GroupRequest>())).ReturnsAsync(1);

            var result = await _groupController.UpdateGroup(groupRequest.Id, groupRequest);

            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task DeleteGroup_ShouldReturnReturnOkResult()
        {
            var mockRole = "Administrator";
            var claims = new List<Claim>
            {
                new Claim("role", mockRole)
            };

            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _groupController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            _groupServiceMock.Setup(g => g.DeleteGroup(It.IsAny<int>())).ReturnsAsync(1);

            var result = await _groupController.DeleteGroup(1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);

            var okResultValue = Assert.IsType<int>(okResult.Value);
            Assert.NotEqual(0, okResultValue);
        }

        [Fact]
        public async Task DeleteGroup_ShouldReturnReturnUnauthorizedResult()
        {
            var mockRole = "Unauthorized";
            var claims = new List<Claim>
            {
                new Claim("role", mockRole)
            };

            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _groupController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            _groupServiceMock.Setup(g => g.DeleteGroup(It.IsAny<int>())).ReturnsAsync(1);

            var result = await _groupController.DeleteGroup(1);

            Assert.IsType<UnauthorizedObjectResult>(result);
        }

    }
}

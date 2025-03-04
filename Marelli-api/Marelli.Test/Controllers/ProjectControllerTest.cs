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
    public class ProjectControllerTest
    {
        private readonly Mock<IProjectService> _projectServiceMock;
        private readonly ProjectController _projectController;

        public ProjectControllerTest()
        {
            _projectServiceMock = new Mock<IProjectService>();

            _projectController = new ProjectController(_projectServiceMock.Object);
        }

        [Fact]
        public async Task SaveProject_ShouldReturnOkResult()
        {
            var mockRole = "Administrator";
            var claims = new List<Claim>
            {
                new Claim("role", mockRole)
            };

            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _projectController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            var project = ProjectFactory.GetProject();

            _projectServiceMock.Setup(p => p.SaveProject(project)).ReturnsAsync(1);

            var result = await _projectController.SaveProject(project);

            var okResult = Assert.IsType<OkObjectResult>(result);

            Assert.NotNull(okResult.Value);

            var okResultValue = Assert.IsType<int>(okResult.Value);

            Assert.NotEqual(0, okResultValue);
        }

        [Fact]
        public async Task SaveProject_ShouldReturnUnauthorizedResult()
        {
            var mockRole = "Unauthorized";
            var claims = new List<Claim>
            {
                new Claim("role", mockRole)
            };

            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _projectController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            var project = ProjectFactory.GetProject();

            _projectServiceMock.Setup(p => p.SaveProject(project)).ReturnsAsync(1);

            var result = await _projectController.SaveProject(project);

            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task ListProjects_ShouldReturnOkResult()
        {
            var project = ProjectFactory.GetProjectResponse();

            var claims = new List<Claim>
        {
            new Claim("role", "Administrator")
        };

            var mockClaimsPrincipal = new Mock<ClaimsPrincipal>();
            mockClaimsPrincipal.Setup(m => m.Claims).Returns(claims);

            _projectController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = mockClaimsPrincipal.Object
                }
            };

            _projectServiceMock.Setup(p => p.ListProjects(It.IsAny<int>())).ReturnsAsync(new List<ProjectResponse> { project });

            var result = await _projectController.ListProjects(1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);

            var okResultValue = Assert.IsType<List<ProjectResponse>>(okResult.Value);
            Assert.Equal(project.Name, okResultValue.First().Name);
        }

        [Fact]
        public async Task ListProjectsByName_ShouldReturnOkResult()
        {
            var project = ProjectFactory.GetProjectResponse();

            _projectServiceMock.Setup(p => p.ListProjectsByName(It.IsAny<string>())).ReturnsAsync(new List<ProjectResponse> { project });

            var result = await _projectController.ListProjectsByName(project.Name);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);

            var okResultValue = Assert.IsType<List<ProjectResponse>>(okResult.Value);
            Assert.Equal(project.Name, okResultValue.First().Name);
        }

        [Fact]
        public async Task UpdateProject_ShouldReturnReturnOkResult()
        {
            var mockRole = "Administrator";
            var claims = new List<Claim>
            {
                new Claim("role", mockRole)
            };

            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _projectController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            var project = ProjectFactory.GetProject();

            _projectServiceMock.Setup(p => p.UpdateProject(It.IsAny<int>(), It.IsAny<Project>())).ReturnsAsync(1);

            var result = await _projectController.UpdateProject(project.Id, project);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);

            var okResultValue = Assert.IsType<int>(okResult.Value);
            Assert.NotEqual(0, okResultValue);
        }

        [Fact]
        public async Task UpdateProject_ShouldReturnReturnUnauthorizedResult()
        {
            var mockRole = "Unauthorized";
            var claims = new List<Claim>
            {
                new Claim("role", mockRole)
            };

            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _projectController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            var project = ProjectFactory.GetProject();

            _projectServiceMock.Setup(p => p.UpdateProject(It.IsAny<int>(), It.IsAny<Project>())).ReturnsAsync(1);

            var result = await _projectController.UpdateProject(project.Id, project);

            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task DeleteProject_ShouldReturnReturnOkResult()
        {
            var mockRole = "Administrator";
            var claims = new List<Claim>
            {
                new Claim("role", mockRole)
            };

            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _projectController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            _projectServiceMock.Setup(p => p.DeleteProjectById(It.IsAny<int>())).ReturnsAsync(1);

            var result = await _projectController.DeleteProject(1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);

            var okResultValue = Assert.IsType<int>(okResult.Value);
            Assert.NotEqual(0, okResultValue);
        }

        [Fact]
        public async Task DeleteProject_ShouldReturnReturnUnauthorizedResult()
        {
            var mockRole = "Unauthorized";
            var claims = new List<Claim>
            {
                new Claim("role", mockRole)
            };

            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _projectController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            _projectServiceMock.Setup(p => p.DeleteProjectById(It.IsAny<int>())).ReturnsAsync(1);

            var result = await _projectController.DeleteProject(1);

            Assert.IsType<UnauthorizedObjectResult>(result);
        }
    }
}

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
    public class BuildTableControllerTest
    {
        private readonly Mock<IBuildTableRowService> _buildTableServiceMock;
        private readonly BuildTableController _buildTableController;

        public BuildTableControllerTest()
        {
            _buildTableServiceMock = new Mock<IBuildTableRowService>();

            _buildTableController = new BuildTableController(_buildTableServiceMock.Object);
        }

        [Fact]
        public async Task SaveBuildTable_ShouldReturnOkResult()
        {
            //Arrange
            var claims = new List<Claim>
            {
                new Claim("role", "Administrator")
            };

            var mockClaimsPrincipal = new Mock<ClaimsPrincipal>();
            mockClaimsPrincipal.Setup(m => m.Claims).Returns(claims);

            _buildTableController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = mockClaimsPrincipal.Object
                }
            };

            var buildTable = BuildTableRowFactory.GetBuildTable();

            _buildTableServiceMock.Setup(b => b.SaveBuildTable(buildTable)).ReturnsAsync(buildTable);

            //Act
            var result = await _buildTableController.SaveBuildTable(buildTable);

            //Assert
            var okResult = Assert.IsType<OkObjectResult>(result);

            Assert.NotNull(okResult.Value);

            var okResultValue = Assert.IsType<BuildTableRow>(okResult.Value);

            Assert.Equal(buildTable.Status, okResultValue.Status);
        }

        [Fact]
        public async Task SaveBuildTable_ShouldReturnUnauthorizedResult()
        {
            //Arrange
            var claims = new List<Claim>
            {
                new Claim("role", "Unauthorized")
            };

            var mockClaimsPrincipal = new Mock<ClaimsPrincipal>();
            mockClaimsPrincipal.Setup(m => m.Claims).Returns(claims);

            _buildTableController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = mockClaimsPrincipal.Object
                }
            };

            var buildTable = BuildTableRowFactory.GetBuildTable();

            _buildTableServiceMock.Setup(b => b.SaveBuildTable(buildTable)).ReturnsAsync(buildTable);

            //Act
            var result = await _buildTableController.SaveBuildTable(buildTable);

            //Assert
            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task ListBuildTable_ShouldReturnOkResult()
        {
            //Arrange
            var claims = new List<Claim>
            {
                new Claim("role", "Administrator")
            };

            var mockClaimsPrincipal = new Mock<ClaimsPrincipal>();
            mockClaimsPrincipal.Setup(m => m.Claims).Returns(claims);

            _buildTableController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = mockClaimsPrincipal.Object
                }
            };

            var buildTable = BuildTableRowFactory.GetBuildTable();

            _buildTableServiceMock.Setup(b => b.ListBuildTable(It.IsAny<int>())).ReturnsAsync(new List<BuildTableRow> { buildTable });

            //Act
            var result = await _buildTableController.ListBuildTable(1);

            //Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);

            var okResultValue = Assert.IsType<List<BuildTableRow>>(okResult.Value);
            Assert.Equal(buildTable.Status, okResultValue.First().Status);
        }

        [Fact]
        public async Task ListBuildTable_ShouldReturnUnauthorizedResult()
        {
            //Arrange
            var claims = new List<Claim>
            {
                new Claim("role", "Unauthorized")
            };

            var mockClaimsPrincipal = new Mock<ClaimsPrincipal>();
            mockClaimsPrincipal.Setup(m => m.Claims).Returns(claims);

            _buildTableController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = mockClaimsPrincipal.Object
                }
            };

            var buildTable = BuildTableRowFactory.GetBuildTable();

            _buildTableServiceMock.Setup(b => b.ListBuildTable(It.IsAny<int>())).ReturnsAsync(new List<BuildTableRow> { buildTable });

            //Act
            var result = await _buildTableController.ListBuildTable(1);

            //Assert
            var okResult = Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task ListBuildTableInProgress_ShouldReturnOkResult()
        {
            //Arrange
            var buildTable = BuildTableRowFactory.GetBuildTable();

            var claims = new List<Claim>
            {
                new Claim("role", "Administrator")
            };

            var mockClaimsPrincipal = new Mock<ClaimsPrincipal>();
            mockClaimsPrincipal.Setup(m => m.Claims).Returns(claims);

            _buildTableController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = mockClaimsPrincipal.Object
                }
            };

            _buildTableServiceMock.Setup(b => b.ListBuildTableInProgress(It.IsAny<int>())).ReturnsAsync(new List<BuildTableRow> { buildTable });

            //Act
            var result = await _buildTableController.ListBuildTableInProgress(1);

            //Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);

            var okResultValue = Assert.IsType<List<BuildTableRow>>(okResult.Value);
            Assert.Equal(buildTable.Status, okResultValue.First().Status);
        }

        [Fact]
        public async Task ListBuildTableInProgress_ShouldReturnUnauthorizedResult()
        {
            //Arrange
            var buildTable = BuildTableRowFactory.GetBuildTable();

            var claims = new List<Claim>
            {
                new Claim("role", "Unauthorized")
            };

            var mockClaimsPrincipal = new Mock<ClaimsPrincipal>();
            mockClaimsPrincipal.Setup(m => m.Claims).Returns(claims);

            _buildTableController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = mockClaimsPrincipal.Object
                }
            };

            _buildTableServiceMock.Setup(b => b.ListBuildTableInProgress(It.IsAny<int>())).ReturnsAsync(new List<BuildTableRow> { buildTable });

            //Act
            var result = await _buildTableController.ListBuildTableInProgress(1);

            //Assert
            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task ListBuildTableInQueue_ShouldReturnOkResult()
        {
            //Arrange
            var buildTable = BuildTableRowFactory.GetBuildTable();

            var claims = new List<Claim>
            {
                new Claim("role", "Administrator")
            };

            var mockClaimsPrincipal = new Mock<ClaimsPrincipal>();
            mockClaimsPrincipal.Setup(m => m.Claims).Returns(claims);

            _buildTableController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = mockClaimsPrincipal.Object
                }
            };

            _buildTableServiceMock.Setup(b => b.ListBuildTableInQueue()).ReturnsAsync(new List<BuildTableRow> { buildTable });

            //Act
            var result = await _buildTableController.ListBuildTableInQueue();

            //Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);

            var okResultValue = Assert.IsType<List<BuildTableRow>>(okResult.Value);
            Assert.Equal(buildTable.Status, okResultValue.First().Status);
        }

        [Fact]
        public async Task ListBuildTableInQueue_ShouldReturnUnauthorizedResult()
        {
            //Arrange
            var buildTable = BuildTableRowFactory.GetBuildTable();

            var claims = new List<Claim>
            {
                new Claim("role", "Unauthorized")
            };

            var mockClaimsPrincipal = new Mock<ClaimsPrincipal>();
            mockClaimsPrincipal.Setup(m => m.Claims).Returns(claims);

            _buildTableController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = mockClaimsPrincipal.Object
                }
            };

            _buildTableServiceMock.Setup(b => b.ListBuildTableInQueue()).ReturnsAsync(new List<BuildTableRow> { buildTable });

            //Act
            var result = await _buildTableController.ListBuildTableInQueue();

            //Assert
            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task GetBuildTable_ShouldReturnOkResult()
        {
            //Arrange
            var mockRole = "Administrator";
            var claims = new List<Claim>
            {
                new Claim("role", mockRole)
            };

            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _buildTableController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            var buildTable = BuildTableRowFactory.GetBuildTable();

            _buildTableServiceMock.Setup(b => b.GetBuildTable(It.IsAny<int>())).ReturnsAsync(buildTable);

            //Act
            var result = await _buildTableController.GetBuildTable(1);

            //Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);

            var okResultValue = Assert.IsType<BuildTableRow>(okResult.Value);
            Assert.Equal(buildTable.Status, okResultValue.Status);
        }

        [Fact]
        public async Task GetBuildTable_ShouldReturnUnauthorizedResult()
        {
            //Arrange
            var mockRole = "Unauthorized";
            var claims = new List<Claim>
            {
                new Claim("role", mockRole)
            };

            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _buildTableController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            var buildTable = BuildTableRowFactory.GetBuildTable();

            _buildTableServiceMock.Setup(b => b.GetBuildTable(It.IsAny<int>())).ReturnsAsync(buildTable);

            //Act
            var result = await _buildTableController.GetBuildTable(1);

            //Assert
            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task GetLastUploaded_ShouldReturnOkResult()
        {
            //Arrange
            var mockRole = "Administrator";
            var claims = new List<Claim>
            {
                new Claim("role", mockRole)
            };

            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _buildTableController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            var buildTable = BuildTableRowFactory.GetBuildTable();

            _buildTableServiceMock.Setup(b => b.GetLastUploadedByUser(It.IsAny<int>())).ReturnsAsync(buildTable);

            //Act
            var result = await _buildTableController.GetLastUploaded(1);

            //Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);

            var okResultValue = Assert.IsType<BuildTableRow>(okResult.Value);
            Assert.Equal(buildTable.Status, okResultValue.Status);
        }

        [Fact]
        public async Task GetLastUploaded_ShouldReturnUnauthorizedResult()
        {
            //Arrange
            var mockRole = "Unauthorized";
            var claims = new List<Claim>
            {
                new Claim("role", mockRole)
            };

            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _buildTableController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            var buildTable = BuildTableRowFactory.GetBuildTable();

            _buildTableServiceMock.Setup(b => b.GetLastUploadedByUser(It.IsAny<int>())).ReturnsAsync(buildTable);

            //Act
            var result = await _buildTableController.GetLastUploaded(1);

            //Assert
            Assert.IsType<UnauthorizedObjectResult>(result);
        }






        [Fact]
        public async Task GetFirstInQueue_ShouldReturnOkResult()
        {
            //Arrange
            var mockRole = "Administrator";
            var claims = new List<Claim>
            {
                new Claim("role", mockRole)
            };

            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _buildTableController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            var buildTable = BuildTableRowFactory.GetBuildTable();

            _buildTableServiceMock.Setup(b => b.GetFirstInQueue(It.IsAny<int>())).ReturnsAsync(buildTable);

            //Act
            var result = await _buildTableController.GetFirstInQueue(1);

            //Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);

            var okResultValue = Assert.IsType<BuildTableRow>(okResult.Value);
            Assert.Equal(buildTable.Status, okResultValue.Status);
        }

        [Fact]
        public async Task UpdateBuildTable_ShouldReturnReturnOkResult()
        {
            //Arrange
            var mockRole = "Administrator";
            var claims = new List<Claim>
            {
                new Claim("role", mockRole)
            };

            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _buildTableController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            var buildTable = BuildTableRowFactory.GetBuildTable();

            _buildTableServiceMock.Setup(b => b.UpdateBuildTable(It.IsAny<int>(), It.IsAny<BuildTableRow>())).ReturnsAsync(1);

            //Act
            var result = await _buildTableController.UpdateBuildTable(buildTable.Id, buildTable);

            //Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);

            var okResultValue = Assert.IsType<int>(okResult.Value);
            Assert.NotEqual(0, okResultValue);
        }

        [Fact]
        public async Task UpdateBuildTable_ShouldReturnReturnUnauthorizedResult()
        {
            //Arrange
            var mockRole = "Unauthorized";
            var claims = new List<Claim>
            {
                new Claim("role", mockRole)
            };

            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _buildTableController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            var buildTable = BuildTableRowFactory.GetBuildTable();

            _buildTableServiceMock.Setup(b => b.UpdateBuildTable(It.IsAny<int>(), It.IsAny<BuildTableRow>())).ReturnsAsync(1);

            //Act
            var result = await _buildTableController.UpdateBuildTable(buildTable.Id, buildTable);

            //Assert
            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task DeleteBuildTable_ShouldReturnReturnOkResult()
        {
            //Arrange
            var mockRole = "Administrator";
            var claims = new List<Claim>
            {
                new Claim("role", mockRole)
            };

            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _buildTableController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            _buildTableServiceMock.Setup(b => b.DeleteBuildTable(It.IsAny<int>())).ReturnsAsync(1);

            //Act
            var result = await _buildTableController.DeleteBuildTable(1);

            //Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);

            var okResultValue = Assert.IsType<int>(okResult.Value);
            Assert.NotEqual(0, okResultValue);
        }

        [Fact]
        public async Task DeleteBuildTable_ShouldReturnReturnUnauthorizedResult()
        {
            //Arrange
            var mockRole = "Unauthorized";
            var claims = new List<Claim>
            {
                new Claim("role", mockRole)
            };

            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _buildTableController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            _buildTableServiceMock.Setup(b => b.DeleteBuildTable(It.IsAny<int>())).ReturnsAsync(1);

            //Act
            var result = await _buildTableController.DeleteBuildTable(1);

            //Assert
            Assert.IsType<UnauthorizedObjectResult>(result);
        }
    }
}

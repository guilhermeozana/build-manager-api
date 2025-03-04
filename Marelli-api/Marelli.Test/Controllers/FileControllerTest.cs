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
    public class FileControllerTest
    {
        private readonly Mock<IFileService> _fileServiceMock;
        private readonly FileController _fileController;

        public FileControllerTest()
        {
            _fileServiceMock = new Mock<IFileService>();

            _fileController = new FileController(_fileServiceMock.Object);
        }

        [Fact]
        public async Task UploadZip_ShouldReturnOkResult()
        {
            var mockRole = "Administrator";
            var claims = new List<Claim>
            {
                new Claim("role", mockRole)
            };

            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _fileController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            var buildTable = BuildTableRowFactory.GetBuildTable();
            var formFile = FileFactory.GetFormFile();

            _fileServiceMock.Setup(f => f.UploadZip(It.IsAny<IFormFile>(), It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(buildTable);

            var result = await _fileController.UploadZip(formFile, 1, 1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);

            var okResultValue = Assert.IsType<BuildTableRow>(okResult.Value);
            Assert.Equal(buildTable.FileName, okResultValue.FileName);
        }

        [Fact]
        public async Task UploadZip_ShouldReturnUnauthorizedResult()
        {
            var mockRole = "Unauthorized";
            var claims = new List<Claim>
            {
                new Claim("role", mockRole)
            };

            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _fileController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            var buildTable = BuildTableRowFactory.GetBuildTable();
            var formFile = FileFactory.GetFormFile();

            _fileServiceMock.Setup(f => f.UploadZip(It.IsAny<IFormFile>(), It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(buildTable);

            var result = await _fileController.UploadZip(formFile, 1, 1);

            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task DownloadZip_ShouldReturnOkResult()
        {
            var mockRole = "Administrator";
            var claims = new List<Claim>
            {
                new Claim("role", mockRole)
            };

            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _fileController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            var fileResponse = FileFactory.GetFileResponse();

            _fileServiceMock.Setup(f => f.DownloadZip(It.IsAny<int>())).ReturnsAsync(fileResponse);

            var result = await _fileController.DownloadZip(1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);

            var okResultValue = Assert.IsType<FileResponse>(okResult.Value);
            Assert.Equal(fileResponse.Name, okResultValue.Name);
        }

        [Fact]
        public async Task DownloadZip_ShouldReturnUnauthorizedResult()
        {
            var mockRole = "Unauthorized";
            var claims = new List<Claim>
            {
                new Claim("role", mockRole)
            };

            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _fileController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            var fileResponse = FileFactory.GetFileResponse();

            _fileServiceMock.Setup(f => f.DownloadZip(It.IsAny<int>())).ReturnsAsync(fileResponse);

            var result = await _fileController.DownloadZip(1);

            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task RemoveZip_ShouldReturnOkResult()
        {
            var mockRole = "Administrator";
            var claims = new List<Claim>
            {
                new Claim("role", mockRole)
            };

            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _fileController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            var fileResponse = FileFactory.GetFileResponse();

            _fileServiceMock.Setup(f => f.RemoveZip(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()));

            var result = await _fileController.RemoveZip(1, 1, 1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);

            var okResultValue = Assert.IsType<string>(okResult.Value);
            Assert.Equal("File deleted successfully.", okResultValue);
        }

        [Fact]
        public async Task RemoveZip_ShouldReturnUnauthorizedResult()
        {
            var mockRole = "Unauthorized";
            var claims = new List<Claim>
            {
                new Claim("role", mockRole)
            };

            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _fileController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            var fileResponse = FileFactory.GetFileResponse();

            _fileServiceMock.Setup(f => f.RemoveZip(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()));

            var result = await _fileController.RemoveZip(1, 1, 1);

            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task UploadBaseline_ShouldReturnOkResult()
        {
            var mockRole = "Administrator";
            var claims = new List<Claim>
            {
                new Claim("role", mockRole)
            };

            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _fileController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            var baseline = BaselineFactory.GetBaseline();
            var formFile = FileFactory.GetFormFile();

            _fileServiceMock.Setup(f => f.UploadBaseline(It.IsAny<IFormFile>(), It.IsAny<int>(), It.IsAny<string>())).ReturnsAsync(baseline);

            var result = await _fileController.UploadBaseline(formFile, 1, "test");

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);

            var okResultValue = Assert.IsType<Baseline>(okResult.Value);
            Assert.Equal(baseline.FileName, okResultValue.FileName);
        }

        [Fact]
        public async Task UploadBaseline_ShouldReturnUnauthorizedResult()
        {
            var mockRole = "Unauthorized";
            var claims = new List<Claim>
            {
                new Claim("role", mockRole)
            };

            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _fileController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            var baseline = BaselineFactory.GetBaseline();
            var formFile = FileFactory.GetFormFile();

            _fileServiceMock.Setup(f => f.UploadBaseline(It.IsAny<IFormFile>(), It.IsAny<int>(), It.IsAny<string>())).ReturnsAsync(baseline);

            var result = await _fileController.UploadBaseline(formFile, 1, "test");

            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task DownloadBaseline_ShouldReturnOkResult()
        {
            var mockRole = "Administrator";
            var claims = new List<Claim>
            {
                new Claim("role", mockRole)
            };

            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _fileController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            var fileResponse = FileFactory.GetFileResponse();

            _fileServiceMock.Setup(f => f.DownloadBaseline(It.IsAny<int>())).ReturnsAsync(fileResponse);

            var result = await _fileController.DownloadBaseline(1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);

            var okResultValue = Assert.IsType<FileResponse>(okResult.Value);
            Assert.Equal(fileResponse.Name, okResultValue.Name);
        }

        [Fact]
        public async Task DownloadBaseline_ShouldReturnUnauthorizedResult()
        {
            var mockRole = "Unauthorized";
            var claims = new List<Claim>
            {
                new Claim("role", mockRole)
            };

            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _fileController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            var fileResponse = FileFactory.GetFileResponse();

            _fileServiceMock.Setup(f => f.DownloadBaseline(It.IsAny<int>())).ReturnsAsync(fileResponse);

            var result = await _fileController.DownloadBaseline(1);

            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task RemoveBaseline_ShouldReturnOkResult()
        {
            var mockRole = "Administrator";
            var claims = new List<Claim>
            {
                new Claim("role", mockRole)
            };

            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _fileController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            var fileResponse = FileFactory.GetFileResponse();

            _fileServiceMock.Setup(f => f.RemoveBaseline(It.IsAny<int>()));

            var result = await _fileController.RemoveBaseline(1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);

            var okResultValue = Assert.IsType<string>(okResult.Value);
            Assert.Equal("Baseline deleted successfully.", okResultValue);
        }

        [Fact]
        public async Task RemoveBaseline_ShouldReturnUnauthorizedResult()
        {
            var mockRole = "Unauthorized";
            var claims = new List<Claim>
            {
                new Claim("role", mockRole)
            };

            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _fileController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            var fileResponse = FileFactory.GetFileResponse();

            _fileServiceMock.Setup(f => f.RemoveBaseline(It.IsAny<int>()));

            var result = await _fileController.RemoveBaseline(1);

            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task DownloadLogs_ShouldReturnOkResult()
        {
            var mockRole = "Administrator";
            var claims = new List<Claim>
            {
                new Claim("role", mockRole)
            };

            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _fileController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            var fileResponse = FileFactory.GetFileResponse();

            _fileServiceMock.Setup(f => f.DownloadLogs(It.IsAny<int>())).ReturnsAsync(fileResponse);

            var result = await _fileController.DownloadLogs(1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);

            var okResultValue = Assert.IsType<FileResponse>(okResult.Value);
            Assert.Equal(fileResponse.Name, okResultValue.Name);
        }

        [Fact]
        public async Task DownloadLogs_ShouldReturnUnauthorizedResult()
        {
            var mockRole = "Unauthorized";
            var claims = new List<Claim>
            {
                new Claim("role", mockRole)
            };

            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _fileController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            var fileResponse = FileFactory.GetFileResponse();

            _fileServiceMock.Setup(f => f.DownloadLogs(It.IsAny<int>())).ReturnsAsync(fileResponse);

            var result = await _fileController.DownloadLogs(1);

            Assert.IsType<UnauthorizedObjectResult>(result);
        }

    }
}

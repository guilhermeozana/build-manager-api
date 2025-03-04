using Marelli.Api.Controllers;
using Marelli.Business.IServices;
using Marelli.Domain.Entities;
using Marelli.Test.Utils.Factories;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Marelli.Test.Controllers
{
    public class BuildLogControllerTest
    {
        private readonly Mock<IBuildLogService> _buildLogServiceMock;
        private readonly BuildLogController _buildLogController;

        public BuildLogControllerTest()
        {
            _buildLogServiceMock = new Mock<IBuildLogService>();

            _buildLogController = new BuildLogController(_buildLogServiceMock.Object);
        }

        [Fact]
        public async Task SaveBuildLog_ShouldReturnOkResult()
        {
            var buildLog = BuildLogFactory.GetBuildLog();

            _buildLogServiceMock.Setup(n => n.SaveBuildLog(buildLog)).ReturnsAsync(buildLog);

            var result = await _buildLogController.SaveBuildLog(buildLog);

            var okResult = Assert.IsType<OkObjectResult>(result);

            Assert.NotNull(okResult.Value);

            var okResultValue = Assert.IsType<BuildLog>(okResult.Value);

            Assert.Equal(buildLog.Status, okResultValue.Status);
        }

        [Fact]
        public async Task ListBuildLog_ShouldReturnOkResult()
        {
            var buildLog = BuildLogFactory.GetBuildLog();

            _buildLogServiceMock.Setup(n => n.ListBuildLog(It.IsAny<int>())).ReturnsAsync(new List<BuildLog> { buildLog });

            var result = await _buildLogController.ListBuildLog(1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);

            var okResultValue = Assert.IsType<List<BuildLog>>(okResult.Value);
            Assert.Equal(buildLog.Status, okResultValue.First().Status);
        }


        [Fact]
        public async Task GetBuildLog_ShouldReturnOkResult()
        {
            var buildLog = BuildLogFactory.GetBuildLog();

            _buildLogServiceMock.Setup(n => n.GetBuildLog(It.IsAny<int>())).ReturnsAsync(buildLog);

            var result = await _buildLogController.GetBuildLog(1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);

            var okResultValue = Assert.IsType<BuildLog>(okResult.Value);
            Assert.Equal(buildLog.Status, okResultValue.Status);
        }


        [Fact]
        public async Task UpdateBuildLog_ShouldReturnReturnOkResult()
        {
            var buildLog = BuildLogFactory.GetBuildLog();

            _buildLogServiceMock.Setup(n => n.UpdateBuildLog(It.IsAny<int>(), It.IsAny<BuildLog>())).ReturnsAsync(1);

            var result = await _buildLogController.UpdateBuildLog(buildLog.Id, buildLog);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);

            var okResultValue = Assert.IsType<int>(okResult.Value);
            Assert.NotEqual(0, okResultValue);
        }

        [Fact]
        public async Task DeleteBuildLog_ShouldReturnReturnOkResult()
        {
            _buildLogServiceMock.Setup(n => n.DeleteBuildLog(It.IsAny<int>())).ReturnsAsync(1);

            var result = await _buildLogController.DeleteBuildLog(1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);

            var okResultValue = Assert.IsType<int>(okResult.Value);
            Assert.NotEqual(0, okResultValue);
        }

    }
}

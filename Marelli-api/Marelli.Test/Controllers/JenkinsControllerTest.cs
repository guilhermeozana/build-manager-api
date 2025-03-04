using Marelli.Api.Controllers;
using Marelli.Business.IServices;
using Marelli.Domain.Dtos;
using Marelli.Domain.Entities;
using Marelli.Test.Utils.Factories;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Marelli.Test.Controllers
{
    public class JenkinsControllerTest
    {
        private readonly Mock<IJenkinsService> _jenkinsServiceMock;
        private readonly JenkinsController _jenkinsController;

        public JenkinsControllerTest()
        {
            _jenkinsServiceMock = new Mock<IJenkinsService>();

            _jenkinsController = new JenkinsController(_jenkinsServiceMock.Object);
        }

        [Fact]
        public async Task Invoke_ShouldReturnOkResult()
        {
            var buildingState = JenkinsFactory.GetBuildingState();

            _jenkinsServiceMock.Setup(j => j.Invoke(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<bool>())).ReturnsAsync(buildingState);

            var result = await _jenkinsController.Invoke(1, 1, 1, true, false);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);

            var okResultValue = Assert.IsType<BuildingState>(okResult.Value);
            Assert.Equal(buildingState.Date, okResultValue.Date);
        }

        [Fact]
        public async Task StopBuild_ShouldReturnOkResult()
        {
            var buildingState = JenkinsFactory.GetBuildingState();

            _jenkinsServiceMock.Setup(j => j.StopBuild(It.IsAny<int>()));

            var result = await _jenkinsController.StopBuild(1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);

            var okResultValue = Assert.IsType<string>(okResult.Value);
            Assert.Contains("stopped successfully", okResultValue);
        }


        [Fact]
        public async Task GetAllData_ShouldReturnOkResult()
        {
            var jenkinsAllData = JenkinsFactory.GetJenkinsAllDataResponse();

            _jenkinsServiceMock.Setup(j => j.GetAllData(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(jenkinsAllData);

            var result = await _jenkinsController.GetAllData(1, 1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);

            var okResultValue = Assert.IsType<JenkinsAllDataResponse>(okResult.Value);
            Assert.Equal(jenkinsAllData.BuildingStates.First().Date, okResultValue.BuildingStates.First().Date);
            Assert.Equal(jenkinsAllData.LogAndArtifacts.First().FileLogS3BucketLocation, okResultValue.LogAndArtifacts.First().FileLogS3BucketLocation);
            Assert.Equal(jenkinsAllData.FileVerifies.First().Filename, okResultValue.FileVerifies.First().Filename);
        }


        // Building State

        [Fact]
        public async Task SaveBuildingState_ShouldReturnOkResult()
        {
            var buildingState = JenkinsFactory.GetBuildingState();

            _jenkinsServiceMock.Setup(j => j.SaveBuildingState(buildingState)).ReturnsAsync(buildingState);

            var result = await _jenkinsController.SaveBuildingState(buildingState);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);

            var okResultValue = Assert.IsType<BuildingState>(okResult.Value);
            Assert.Equal(buildingState.Date, okResultValue.Date);
        }

        [Fact]
        public async Task ListBuildingState_ShouldReturnOkResult()
        {
            var buildingState = JenkinsFactory.GetBuildingState();

            _jenkinsServiceMock.Setup(j => j.ListBuildingState(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(new List<BuildingState> { buildingState });

            var result = await _jenkinsController.ListBuildingState(1, 1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);

            var okResultValue = Assert.IsType<List<BuildingState>>(okResult.Value);
            Assert.Equal(buildingState.Date, okResultValue.First().Date);
        }

        [Fact]
        public async Task GetBuildingState_ShouldReturnOkResult()
        {
            var buildingState = JenkinsFactory.GetBuildingState();

            _jenkinsServiceMock.Setup(j => j.GetBuildingStateById(It.IsAny<int>())).ReturnsAsync(buildingState);

            var result = await _jenkinsController.GetBuildingState(1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);

            var okResultValue = Assert.IsType<BuildingState>(okResult.Value);
            Assert.Equal(buildingState.Date, okResultValue.Date);
        }

        [Fact]
        public async Task GetBuildingStateByBuildId_ShouldReturnOkResult()
        {
            var buildingState = JenkinsFactory.GetBuildingState();

            _jenkinsServiceMock.Setup(j => j.GetBuildingStateByBuildId(It.IsAny<int>())).ReturnsAsync(buildingState);

            var result = await _jenkinsController.GetBuildingStateByBuildId(1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);

            var okResultValue = Assert.IsType<BuildingState>(okResult.Value);
            Assert.Equal(buildingState.Date, okResultValue.Date);
        }


        [Fact]
        public async Task UpdateBuildingState_ShouldReturnReturnOkResult()
        {
            var buildingState = JenkinsFactory.GetBuildingState();

            _jenkinsServiceMock.Setup(j => j.UpdateBuildingState(It.IsAny<int>(), It.IsAny<BuildingState>())).ReturnsAsync(buildingState);

            var result = await _jenkinsController.UpdateBuildingState(buildingState.Id, buildingState);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);

            var okResultValue = Assert.IsType<BuildingState>(okResult.Value);
            Assert.Equal(buildingState.Date, okResultValue.Date);
        }

        [Fact]
        public async Task DeleteBuildingState_ShouldReturnReturnOkResult()
        {
            _jenkinsServiceMock.Setup(j => j.DeleteBuildingState(It.IsAny<int>())).ReturnsAsync(1);

            var result = await _jenkinsController.DeleteBuildingState(1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);

            var okResultValue = Assert.IsType<int>(okResult.Value);
            Assert.NotEqual(0, okResultValue);
        }

        // Log And Artifact

        [Fact]
        public async Task SaveLogAndArtifact_ShouldReturnOkResult()
        {
            var logAndArtifact = JenkinsFactory.GetLogAndArtifact();

            _jenkinsServiceMock.Setup(j => j.SaveLogAndArtifact(logAndArtifact)).ReturnsAsync(1);

            var result = await _jenkinsController.SaveLogAndArtifact(logAndArtifact);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);

            var okResultValue = Assert.IsType<int>(okResult.Value);
            Assert.NotEqual(0, okResultValue);
        }

        [Fact]
        public async Task ListLogAndArtifact_ShouldReturnOkResult()
        {
            var logAndArtifact = JenkinsFactory.GetLogAndArtifact();

            _jenkinsServiceMock.Setup(j => j.ListLogAndArtifact(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(new List<LogAndArtifact> { logAndArtifact });

            var result = await _jenkinsController.ListLogAndArtifact(1, 1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);

            var okResultValue = Assert.IsType<List<LogAndArtifact>>(okResult.Value);
            Assert.Equal(logAndArtifact.FileLogS3BucketLocation, okResultValue.First().FileLogS3BucketLocation);
        }

        [Fact]
        public async Task GetLogAndArtifact_ShouldReturnOkResult()
        {
            var logAndArtifact = JenkinsFactory.GetLogAndArtifact();

            _jenkinsServiceMock.Setup(j => j.GetLogAndArtifactById(It.IsAny<int>())).ReturnsAsync(logAndArtifact);

            var result = await _jenkinsController.GetLogAndArtifact(1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);

            var okResultValue = Assert.IsType<LogAndArtifact>(okResult.Value);
            Assert.Equal(logAndArtifact.FileLogS3BucketLocation, okResultValue.FileLogS3BucketLocation);
        }

        [Fact]
        public async Task UpdateLogAndArtifact_ShouldReturnReturnOkResult()
        {
            var logAndArtifact = JenkinsFactory.GetLogAndArtifact();

            _jenkinsServiceMock.Setup(j => j.UpdateLogAndArtifact(It.IsAny<int>(), It.IsAny<LogAndArtifact>())).ReturnsAsync(1);

            var result = await _jenkinsController.UpdateLogAndArtifact(logAndArtifact.Id, logAndArtifact);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);

            var okResultValue = Assert.IsType<int>(okResult.Value);
            Assert.NotEqual(0, okResultValue);
        }

        [Fact]
        public async Task DeleteLogAndArtifact_ShouldReturnReturnOkResult()
        {
            _jenkinsServiceMock.Setup(j => j.DeleteLogAndArtifact(It.IsAny<int>())).ReturnsAsync(1);

            var result = await _jenkinsController.DeleteLogAndArtifact(1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);

            var okResultValue = Assert.IsType<int>(okResult.Value);
            Assert.NotEqual(0, okResultValue);
        }

        // File Verify

        [Fact]
        public async Task SaveFileVerify_ShouldReturnOkResult()
        {
            var fileVerify = JenkinsFactory.GetFileVerify();

            _jenkinsServiceMock.Setup(j => j.SaveFileVerify(fileVerify)).ReturnsAsync(1);

            var result = await _jenkinsController.SaveFileVerify(fileVerify);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);

            var okResultValue = Assert.IsType<int>(okResult.Value);
            Assert.NotEqual(0, okResultValue);
        }

        [Fact]
        public async Task ListFileVerify_ShouldReturnOkResult()
        {
            var fileVerify = JenkinsFactory.GetFileVerify();

            _jenkinsServiceMock.Setup(j => j.ListFileVerify(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(new List<FileVerify> { fileVerify });

            var result = await _jenkinsController.ListFileVerify(1, 1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);

            var okResultValue = Assert.IsType<List<FileVerify>>(okResult.Value);
            Assert.Equal(fileVerify.Filename, okResultValue.First().Filename);
        }

        [Fact]
        public async Task GetFileVerify_ShouldReturnOkResult()
        {
            var fileVerify = JenkinsFactory.GetFileVerify();

            _jenkinsServiceMock.Setup(j => j.GetFileVerifyById(It.IsAny<int>())).ReturnsAsync(fileVerify);

            var result = await _jenkinsController.GetFileVerify(1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);

            var okResultValue = Assert.IsType<FileVerify>(okResult.Value);
            Assert.Equal(fileVerify.Filename, okResultValue.Filename);
        }

        [Fact]
        public async Task UpdateFileVerify_ShouldReturnReturnOkResult()
        {
            var fileVerify = JenkinsFactory.GetFileVerify();

            _jenkinsServiceMock.Setup(j => j.UpdateFileVerify(It.IsAny<int>(), It.IsAny<FileVerify>())).ReturnsAsync(1);

            var result = await _jenkinsController.UpdateFileVerify(fileVerify.Id, fileVerify);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);

            var okResultValue = Assert.IsType<int>(okResult.Value);
            Assert.NotEqual(0, okResultValue);
        }

        [Fact]
        public async Task DeleteFileVerify_ShouldReturnReturnOkResult()
        {

            _jenkinsServiceMock.Setup(j => j.DeleteFileVerify(It.IsAny<int>())).ReturnsAsync(1);

            var result = await _jenkinsController.DeleteFileVerify(1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);

            var okResultValue = Assert.IsType<int>(okResult.Value);
            Assert.NotEqual(0, okResultValue);
        }

    }
}

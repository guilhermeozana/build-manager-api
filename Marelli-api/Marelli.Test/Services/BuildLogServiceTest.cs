using Marelli.Business.Exceptions;
using Marelli.Business.Hubs;
using Marelli.Business.Services;
using Marelli.Domain.Entities;
using Marelli.Infra.IRepositories;
using Marelli.Test.Utils.Factories;
using Microsoft.AspNetCore.SignalR;
using Moq;
using Xunit;

namespace Marelli.Test.Services
{
    public class BuildLogServiceTest
    {
        private readonly Mock<IBuildLogRepository> _buildLogRepositoryMock;
        private readonly Mock<IHubContext<BuildLogHub>> _hubContextMock;

        private readonly BuildLogService _buildLogService;

        public BuildLogServiceTest()
        {
            _buildLogRepositoryMock = new Mock<IBuildLogRepository>();
            _hubContextMock = new Mock<IHubContext<BuildLogHub>>();

            _buildLogService = new BuildLogService(_buildLogRepositoryMock.Object, _hubContextMock.Object);
        }


        [Fact]
        public async Task SaveBuildLog_ShouldReturnBuildLogInstance()
        {
            var buildLogExpected = BuildLogFactory.GetBuildLog();

            var mockClientProxy = new Mock<IClientProxy>();

            _hubContextMock.Setup(h => h.Clients.All).Returns(mockClientProxy.Object);
            _buildLogRepositoryMock.Setup(b => b.ListBuildLog(It.IsAny<int>())).ReturnsAsync(new List<BuildLog>() { buildLogExpected });
            _buildLogRepositoryMock.Setup(b => b.SaveBuildLog(It.IsAny<BuildLog>())).ReturnsAsync(buildLogExpected);

            var res = await _buildLogService.SaveBuildLog(buildLogExpected);

            Assert.NotNull(res);
            Assert.Equal(buildLogExpected, res);
        }

        [Fact]
        public async Task ListBuildLog_ShouldReturnBuildLogList()
        {
            var buildLogList = new List<BuildLog>() { BuildLogFactory.GetBuildLog() };

            _buildLogRepositoryMock.Setup(b => b.ListBuildLog(It.IsAny<int>())).ReturnsAsync(buildLogList);

            var res = await _buildLogService.ListBuildLog(1);

            Assert.NotEmpty(res);
            Assert.Equal(buildLogList, res);
        }

        [Fact]
        public async Task GetBuildLog_ShouldReturnBuildLogInstance()
        {
            var buildLogExpected = BuildLogFactory.GetBuildLog();

            _buildLogRepositoryMock.Setup(b => b.GetBuildLog(It.IsAny<int>())).ReturnsAsync(buildLogExpected);

            var res = await _buildLogService.GetBuildLog(1);

            Assert.NotNull(res);
            Assert.Equal(buildLogExpected, res);
        }

        [Fact]
        public async Task GetBuildLog_ShouldThrowNotFoundException()
        {
            _buildLogRepositoryMock.Setup(b => b.GetBuildLog(It.IsAny<int>())).ReturnsAsync((BuildLog)null);

            await Assert.ThrowsAsync<NotFoundException>(async () => await _buildLogService.GetBuildLog(1));
        }

        [Fact]
        public async Task UpdateBuildLog_ShouldReturnGreaterThanZero()
        {
            var buildLog = BuildLogFactory.GetBuildLog();
            var mockClientProxy = new Mock<IClientProxy>();

            _hubContextMock.Setup(h => h.Clients.All).Returns(mockClientProxy.Object);
            _buildLogRepositoryMock.Setup(b => b.GetBuildLog(It.IsAny<int>())).ReturnsAsync(buildLog);
            _buildLogRepositoryMock.Setup(b => b.UpdateBuildLog(It.IsAny<int>(), It.IsAny<BuildLog>(), It.IsAny<BuildLog>())).ReturnsAsync(1);
            _buildLogRepositoryMock.Setup(b => b.ListBuildLog(It.IsAny<int>())).ReturnsAsync(new List<BuildLog>() { buildLog });


            var res = await _buildLogService.UpdateBuildLog(1, buildLog);

            Assert.Equal(1, res);
        }

        [Fact]
        public async Task UpdateBuildLog_ShouldThrowNotFoundException()
        {
            _buildLogRepositoryMock.Setup(b => b.GetBuildLog(It.IsAny<int>())).ReturnsAsync((BuildLog)null);

            await Assert.ThrowsAsync<NotFoundException>(async () => await _buildLogService.UpdateBuildLog(1, BuildLogFactory.GetBuildLog()));
        }

        [Fact]
        public async Task DeleteBuildLog_ShouldReturnGreaterThanZero()
        {
            _buildLogRepositoryMock.Setup(b => b.GetBuildLog(It.IsAny<int>())).ReturnsAsync(BuildLogFactory.GetBuildLog());
            _buildLogRepositoryMock.Setup(b => b.DeleteBuildLog(It.IsAny<BuildLog>())).ReturnsAsync(1);

            var res = await _buildLogService.DeleteBuildLog(1);

            Assert.Equal(1, res);
        }

        [Fact]
        public async Task DeleteBuildLog_ShouldThrowNotFoundException()
        {
            _buildLogRepositoryMock.Setup(b => b.GetBuildLog(It.IsAny<int>())).ReturnsAsync((BuildLog)null);

            await Assert.ThrowsAsync<NotFoundException>(async () => await _buildLogService.DeleteBuildLog(1));
        }

    }
}
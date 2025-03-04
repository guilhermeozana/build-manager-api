using Marelli.Business.Hubs;
using Marelli.Domain.Entities;
using Marelli.Test.Utils.Factories;
using Microsoft.AspNetCore.SignalR;
using Moq;
using Xunit;

namespace Marelli.Test.Hubs
{

    public class BuildLogHubTest
    {
        [Fact]
        public async Task SendBuildLogs_ShouldSendLogsToAllClients()
        {
            // Arrange
            var mockClients = new Mock<IHubCallerClients>();
            var mockClientProxy = new Mock<IClientProxy>();
            mockClients.Setup(clients => clients.All).Returns(mockClientProxy.Object);

            var hub = new BuildLogHub
            {
                Clients = mockClients.Object
            };

            var buildLogs = new List<BuildLog> { BuildLogFactory.GetBuildLog() };

            // Act
            await hub.SendBuildLogs(buildLogs);

            // Assert
            mockClientProxy.Verify(
                proxy => proxy.SendCoreAsync(
                    "ReceiveBuildLogs",
                    new object[] { buildLogs },
                    default),
                Times.Once);
        }
    }
}

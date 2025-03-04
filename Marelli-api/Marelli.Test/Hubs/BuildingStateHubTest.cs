using Marelli.Business.Hubs;
using Marelli.Test.Utils.Factories;
using Microsoft.AspNetCore.SignalR;
using Moq;
using Xunit;

namespace Marelli.Test.Hubs
{

    public class BuildStateHubTest
    {
        [Fact]
        public async Task SendBuildState_ShouldSendMessageToAllClients()
        {
            // Arrange
            var mockClients = new Mock<IHubCallerClients>();
            var mockClientProxy = new Mock<IClientProxy>();
            mockClients.Setup(clients => clients.All).Returns(mockClientProxy.Object);

            var hub = new BuildStateHub
            {
                Clients = mockClients.Object
            };

            var buildingState = JenkinsFactory.GetBuildingState();

            // Act
            await hub.SendBuildState(buildingState);

            // Assert
            mockClientProxy.Verify(
                proxy => proxy.SendCoreAsync(
                    "ReceiveBuildState",
                    new[] { buildingState },
                    default),
                Times.Once);
        }
    }

}

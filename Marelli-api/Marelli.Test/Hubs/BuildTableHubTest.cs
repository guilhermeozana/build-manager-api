using Marelli.Business.Hubs;
using Marelli.Domain.Entities;
using Marelli.Test.Utils.Factories;
using Microsoft.AspNetCore.SignalR;
using Moq;
using Xunit;

namespace Marelli.Test.Hubs
{

    public class BuildTableHubTests
    {
        [Fact]
        public async Task SendBuildTableRows_ShouldSendRowsToAllClients()
        {
            // Arrange
            var mockClients = new Mock<IHubCallerClients>();
            var mockClientProxy = new Mock<IClientProxy>();
            mockClients.Setup(clients => clients.All).Returns(mockClientProxy.Object);

            var hub = new BuildTableHub
            {
                Clients = mockClients.Object
            };

            var buildTableRows = new List<BuildTableRow>
        {
            BuildTableRowFactory.GetBuildTable()
        };

            // Act
            await hub.SendBuildTableRows(buildTableRows);

            // Assert
            mockClientProxy.Verify(
                proxy => proxy.SendCoreAsync(
                    "ReceiveBuildTableRows",
                    new object[] { buildTableRows },
                    default),
                Times.Once);
        }
    }


}

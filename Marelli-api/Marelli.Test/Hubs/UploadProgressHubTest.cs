using Marelli.Business.Hubs;
using Microsoft.AspNetCore.SignalR;
using Moq;
using Xunit;

namespace Marelli.Test.Hubs
{

    public class UploadProgressHubTest
    {
        [Fact]
        public async Task SendProgress_ShouldSendMessageToAllClients()
        {
            // Arrange
            var mockClients = new Mock<IHubCallerClients>();
            var mockClientProxy = new Mock<IClientProxy>();
            mockClients.Setup(clients => clients.All).Returns(mockClientProxy.Object);

            var hub = new UploadProgressHub
            {
                Clients = mockClients.Object
            };

            // Act
            await hub.SendProgress(1);

            // Assert
            mockClientProxy.Verify(
                proxy => proxy.SendCoreAsync(
                    "ReceiveProgress",
                    new object[] { 1 },
                    default),
                Times.Once);
        }
    }

}

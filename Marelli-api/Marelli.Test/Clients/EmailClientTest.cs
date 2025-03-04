using MailKit;
using MailKit.Net.Smtp;
using Marelli.Business.Clients;
using MimeKit;
using Moq;
using Xunit;

namespace Marelli.Business.Tests
{
    public class EmailClientTests
    {
        [Fact]
        public async Task ConnectAsync_ShouldInvokeSmtpClientConnectAsync()
        {
            // Arrange
            var smtpClientMock = new Mock<ISmtpClient>();
            var emailClient = new EmailClient(smtpClientMock.Object);
            var host = "smtp.example.com";
            var port = 587;
            var options = MailKit.Security.SecureSocketOptions.StartTls;

            // Act
            await emailClient.ConnectAsync(host, port, options);

            // Assert
            smtpClientMock.Verify(x => x.ConnectAsync(host, port, options, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task AuthenticateAsync_ShouldInvokeSmtpClientAuthenticateAsync()
        {
            // Arrange
            var smtpClientMock = new Mock<ISmtpClient>();
            var emailClient = new EmailClient(smtpClientMock.Object);
            var userName = "user@example.com";
            var password = "password";

            // Act
            await emailClient.AuthenticateAsync(userName, password);

            // Assert
            smtpClientMock.Verify(x => x.AuthenticateAsync(userName, password, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task SendAsync_ShouldInvokeSmtpClientSendAsync()
        {
            // Arrange
            var smtpClientMock = new Mock<ISmtpClient>();
            var emailClient = new EmailClient(smtpClientMock.Object);
            var message = new MimeMessage();

            // Act
            await emailClient.SendAsync(message);

            // Assert
            smtpClientMock.Verify(x => x.SendAsync(message, It.IsAny<CancellationToken>(), It.IsAny<ITransferProgress>()), Times.Once);
        }

        [Fact]
        public async Task DisconnectAsync_ShouldInvokeSmtpClientDisconnectAsync()
        {
            // Arrange
            var smtpClientMock = new Mock<ISmtpClient>();
            var emailClient = new EmailClient(smtpClientMock.Object);
            var quit = true;

            // Act
            await emailClient.DisconnectAsync(quit);

            // Assert
            smtpClientMock.Verify(x => x.DisconnectAsync(quit, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public void Dispose_ShouldInvokeSmtpClientDispose()
        {
            // Arrange
            var smtpClientMock = new Mock<ISmtpClient>();
            var emailClient = new EmailClient(smtpClientMock.Object);

            // Act
            emailClient.Dispose();

            // Assert
            smtpClientMock.Verify(x => x.Dispose(), Times.Once);
        }
    }
}

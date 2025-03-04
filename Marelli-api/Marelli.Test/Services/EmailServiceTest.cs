using MailKit.Net.Smtp;
using MailKit.Security;
using Marelli.Business.IClients;
using Marelli.Business.Services;
using Marelli.Domain.Entities;
using Marelli.Infra.Configuration;
using Microsoft.Extensions.Options;
using MimeKit;
using Moq;
using Xunit;

namespace Marelli.Test.Services
{
    public class EmailServiceTest
    {
        private readonly Mock<IOptions<SmtpOptions>> _smtpOptionsMock;
        private readonly Mock<IOptions<AppSettings>> _appSettingsMock;
        private readonly Mock<IEmailClient> _emailClientMock;

        private readonly EmailService _emailService;

        public EmailServiceTest()
        {
            _smtpOptionsMock = new Mock<IOptions<SmtpOptions>>();
            _appSettingsMock = new Mock<IOptions<AppSettings>>();
            _emailClientMock = new Mock<IEmailClient>();

            _smtpOptionsMock.Setup(o => o.Value).Returns(new SmtpOptions
            {
                Port = 587,
                Username = "user@example.com",
                Password = "password"
            });

            _appSettingsMock.Setup(o => o.Value).Returns(new AppSettings
            {
                UrlHost = "http://test.com",
                Secret = "test"
            });

            _emailService = new EmailService(_smtpOptionsMock.Object, _appSettingsMock.Object, _emailClientMock.Object);
        }

        [Fact]
        public async Task SendEmail_ShouldSendEmailSuccessfully()
        {
            _emailClientMock.Setup(e => e.ConnectAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<SecureSocketOptions>(), It.IsAny<CancellationToken>()));
            _emailClientMock.Setup(e => e.AuthenticateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()));
            _emailClientMock.Setup(e => e.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<CancellationToken>()));

            await _emailService.SendEmail("test", "test", "test");

            _emailClientMock.Verify(e => e.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task SendEmail_ShouldHandleSmtpCommandException()
        {
            var exceptionMessage = "SMTP command error";
            var smtpStatusCode = SmtpStatusCode.BadCommandSequence;
            var smtpErrorCode = SmtpErrorCode.RecipientNotAccepted;

            _emailClientMock.Setup(e => e.ConnectAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<SecureSocketOptions>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new SmtpCommandException(smtpErrorCode, smtpStatusCode, exceptionMessage));

            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _emailService.SendEmail("test", "test", "test"));

            Assert.Contains(exceptionMessage, exception.Message);
        }


        [Fact]
        public async Task SendEmail_ShouldHandleGeneralException()
        {
            var generalExceptionMessage = "An unexpected error occurred";
            _emailClientMock.Setup(e => e.ConnectAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<SecureSocketOptions>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception(generalExceptionMessage));

            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _emailService.SendEmail("test", "test", "test"));

            Assert.Contains(generalExceptionMessage, exception.Message);
        }

    }
}

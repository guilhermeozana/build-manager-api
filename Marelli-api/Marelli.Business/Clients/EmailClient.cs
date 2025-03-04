using MailKit.Net.Smtp;
using Marelli.Business.IClients;

namespace Marelli.Business.Clients
{
    public class EmailClient : IEmailClient
    {
        private readonly ISmtpClient _smtpClient;

        public EmailClient(ISmtpClient smtpClient)
        {
            _smtpClient = smtpClient;
        }

        public async Task ConnectAsync(string host, int port, MailKit.Security.SecureSocketOptions options, CancellationToken cancellationToken = default)
        {
            await _smtpClient.ConnectAsync(host, port, options, cancellationToken);
        }

        public async Task AuthenticateAsync(string userName, string password, CancellationToken cancellationToken = default)
        {
            await _smtpClient.AuthenticateAsync(userName, password, cancellationToken);
        }

        public async Task SendAsync(MimeKit.MimeMessage message, CancellationToken cancellationToken = default)
        {
            await _smtpClient.SendAsync(message, cancellationToken);
        }

        public async Task DisconnectAsync(bool quit, CancellationToken cancellationToken = default)
        {
            await _smtpClient.DisconnectAsync(quit, cancellationToken);
        }

        public void Dispose()
        {
            _smtpClient.Dispose();
        }
    }

}

namespace Marelli.Business.IClients
{
    public interface IEmailClient : IDisposable
    {
        Task ConnectAsync(string host, int port, MailKit.Security.SecureSocketOptions options, CancellationToken cancellationToken = default);
        Task AuthenticateAsync(string userName, string password, CancellationToken cancellationToken = default);
        Task SendAsync(MimeKit.MimeMessage message, CancellationToken cancellationToken = default);
        Task DisconnectAsync(bool quit, CancellationToken cancellationToken = default);
    }

}

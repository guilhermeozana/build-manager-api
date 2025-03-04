using Marelli.Business.IClients;
using Marelli.Business.IServices;
using Marelli.Domain.Entities;
using Marelli.Infra.Configuration;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Marelli.Business.Services
{

    public class EmailService : IEmailService
    {
        private readonly SmtpOptions _smtpOptions;
        private readonly AppSettings _appSettings;
        private readonly IEmailClient _emailClient;

        public EmailService(IOptions<SmtpOptions> smtpOptions, IOptions<AppSettings> appSettings, IEmailClient emailClient)
        {
            _smtpOptions = smtpOptions.Value;
            _appSettings = appSettings.Value;
            _emailClient = emailClient;
        }

        //public async Task SendEmail(string email, string subject, string message)
        //{
        //    using (var smtpClient = new SmtpClient(_smtpOptions.SmtpServer, _smtpOptions.Port))
        //    {
        //        smtpClient.UseDefaultCredentials = false;
        //        smtpClient.Credentials = new NetworkCredential(_smtpOptions.Username, _smtpOptions.Password);
        //        smtpClient.EnableSsl = _smtpOptions.EnableSsl;

        //        var mailMessage = new MailMessage
        //        {
        //            From = new MailAddress(_smtpOptions.FromEmail, _smtpOptions.FromName),
        //            Subject = subject,
        //            Body = message,
        //            IsBodyHtml = true
        //        };

        //        mailMessage.To.Add(email);

        //        await smtpClient.SendMailAsync(mailMessage);
        //    }
        //}

        public async Task SendEmail(string to, string subject, string body)
        {
            try
            {
                var smt = new SmtpConfiguration("smtp.seuservidor.com", 587, "franciscodiogo11@gmail.com", "!098Cao10@");
                var email = new MimeKit.MimeMessage();
                email.From.Add(MailboxAddress.Parse("diogof@deverreddddddd.cloud"));
                email.To.Add(MailboxAddress.Parse(to));
                email.Subject = subject;
                email.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = body };

                await _emailClient.ConnectAsync("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
                await _emailClient.AuthenticateAsync("operacao@vonbraunlabs.com.br", "x9xYJNWNPKS3kfAvQz");
                await _emailClient.SendAsync(email);

                await _emailClient.DisconnectAsync(true);
            }
            catch (MailKit.Net.Smtp.SmtpCommandException smtpEx)
            {
                throw new Exception($"Error sending email: {smtpEx.Message}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Unexpected error: {ex.Message}");
            }
        }

    }
}

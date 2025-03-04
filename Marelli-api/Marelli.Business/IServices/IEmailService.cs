namespace Marelli.Business.IServices
{
    public interface IEmailService
    {
        public Task SendEmail(string to, string subject, string body);


    }
}

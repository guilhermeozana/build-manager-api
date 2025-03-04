namespace Marelli.Business.IServices
{
    public interface ISecretsManagerService
    {
        public Task<string> GetDbConnectionString();
    }
}

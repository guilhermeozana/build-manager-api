using Marelli.Domain.Dtos;

namespace Marelli.Business.IServices
{
    public interface IAuthorizationService
    {
        public Task<TokenResponse> GenerateToken(string email, string password);
    }
}

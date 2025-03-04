using Marelli.Domain.Dtos;
using Marelli.Domain.Entities;
using System.Security.Claims;

namespace Marelli.Business.IServices
{
    public interface ITokenService
    {
        public Task<TokenResponse> GenerateAccessToken(User user);

        public string GeneratePasswordResetToken();

        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
    }
}

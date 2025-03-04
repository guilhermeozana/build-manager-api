using Marelli.Business.IServices;
using Marelli.Domain.Entities;
using Marelli.Infra.IRepositories;
using Microsoft.AspNetCore.Http;

namespace Marelli.Business.Services
{
    public class UserTokenService : IUserTokenService
    {
        private readonly IUserTokenRepository _userTokenRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public UserTokenService(IUserTokenRepository userTokenRepository, IHttpContextAccessor httpContextAccessor)
        {
            _userTokenRepository = userTokenRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<int> SaveUserToken(UserToken request)
        {
            return await _userTokenRepository.SaveUserToken(request);
        }

        public async Task<bool> VerifyUserTokenValid(string token)
        {
            return await _userTokenRepository.VerifyUserTokenValid(token);
        }

        public async Task RevokeUserTokens(int userId)
        {
            await _userTokenRepository.RevokeUserTokens(userId);
        }

        public async Task RevokeUserTokensExceptCurrent(int userId)
        {
            var token = _httpContextAccessor.HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (token != null)
            {
                await _userTokenRepository.RevokeUserTokensExceptCurrent(userId, token);
            }
        }

    }
}

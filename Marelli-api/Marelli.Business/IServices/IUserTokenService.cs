using Marelli.Domain.Entities;

namespace Marelli.Business.IServices
{
    public interface IUserTokenService
    {
        public Task<int> SaveUserToken(UserToken request);

        public Task<bool> VerifyUserTokenValid(string token);

        public Task RevokeUserTokens(int userId);
        public Task RevokeUserTokensExceptCurrent(int userId);

    }
}

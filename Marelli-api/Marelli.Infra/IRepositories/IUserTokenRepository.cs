using Marelli.Domain.Entities;

namespace Marelli.Infra.IRepositories
{
    public interface IUserTokenRepository
    {
        public Task<int> SaveUserToken(UserToken userToken);

        public Task<UserToken> GetUserToken(string token);

        public Task<bool> VerifyUserTokenValid(string token);

        public Task RevokeUserTokens(int userId);

        public Task RevokeUserTokensExceptCurrent(int userId, string currentToken);

        public Task RemoveExpiredUserTokens();
    }
}

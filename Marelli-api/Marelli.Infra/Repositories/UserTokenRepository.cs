using Marelli.Domain.Entities;
using Marelli.Infra.Context;
using Marelli.Infra.IRepositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Marelli.Infra.Repositories
{
    public class UserTokenRepository : IUserTokenRepository
    {
        private readonly DemurrageContext _context;

        public UserTokenRepository(DemurrageContext context)
        {
            _context = context;
        }

        public async Task<int> SaveUserToken(UserToken userToken)
        {
            _context.ChangeTracker.Clear();

            await RemoveExpiredUserTokens();

            _context.UserToken.Add(userToken);

            return await _context.SaveChangesAsync();
        }

        public async Task<UserToken> GetUserToken(string token)
        {
            return await _context.UserToken
                .Where(t => t.Token == token)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> VerifyUserTokenValid(string token)
        {
            var isUserTokenValid = !(_context.UserToken
                     .Where(t => t.Token == token && t.ExpiryDate > DateTime.UtcNow && !t.IsRevoked).IsNullOrEmpty());

            return isUserTokenValid;
        }

        public async Task RevokeUserTokens(int userId)
        {
            var userTokens = await _context.UserToken
                .Where(t => t.UserId == userId && t.ExpiryDate > DateTime.UtcNow)
                .ToListAsync();

            foreach (var userToken in userTokens)
            {
                userToken.IsRevoked = true;
            }

            await _context.SaveChangesAsync();
        }

        public async Task RevokeUserTokensExceptCurrent(int userId, string currentToken)
        {
            var userTokens = await _context.UserToken
                .Where(t => t.UserId == userId && t.Token != currentToken && t.ExpiryDate > DateTime.UtcNow)
                .ToListAsync();

            foreach (var userToken in userTokens)
            {
                userToken.IsRevoked = true;
            }

            await _context.SaveChangesAsync();
        }

        public async Task RemoveExpiredUserTokens()
        {
            var userTokens = await _context.UserToken
                .Where(t => t.ExpiryDate <= DateTime.UtcNow)
                .ToListAsync();

            _context.RemoveRange(userTokens);

            await _context.SaveChangesAsync();
        }
    }
}

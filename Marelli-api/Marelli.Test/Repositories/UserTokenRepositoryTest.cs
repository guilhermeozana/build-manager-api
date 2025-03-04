using Marelli.Domain.Entities;
using Marelli.Infra.Repositories;
using Marelli.Test.Utils.Factories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Marelli.Test.Repositories
{
    public class UserTokenRepositoryTest
    {


        [Fact]
        public async Task SaveUserToken_ShouldReturnGreaterThanZero()
        {
            var demurrageContext = DbContextFactory.GetDemurrageContextTest();

            var userTokenRepository = new UserTokenRepository(demurrageContext);

            var userToken = UserTokenFactory.GetUserToken();

            var result = await userTokenRepository.SaveUserToken(userToken);

            Assert.Equal(1, result);
        }

        [Fact]
        public async Task GetUserToken_ShouldReturnUserTokenInstance()
        {
            var demurrageContext = DbContextFactory.GetDemurrageContextTest();
            var userTokenRepository = new UserTokenRepository(demurrageContext);
            var userToken = UserTokenFactory.GetUserToken();

            demurrageContext.Add(userToken);
            await demurrageContext.SaveChangesAsync();

            var result = await userTokenRepository.GetUserToken(userToken.Token);

            Assert.IsType<UserToken>(result);
            Assert.Equal(userToken.Token, result.Token);
        }

        [Fact]
        public async Task VerifyUserTokenValid_ShouldReturnTrue()
        {
            var demurrageContext = DbContextFactory.GetDemurrageContextTest();
            var userTokenRepository = new UserTokenRepository(demurrageContext);
            var userToken = UserTokenFactory.GetUserToken();

            demurrageContext.Add(userToken);
            await demurrageContext.SaveChangesAsync();

            var result = await userTokenRepository.VerifyUserTokenValid(userToken.Token);

            Assert.True(result);
        }

        [Fact]
        public async Task RevokeUserTokens_ShouldRevokeAllExpiredUserTokens()
        {
            var demurrageContext = DbContextFactory.GetDemurrageContextTest();
            var userTokenRepository = new UserTokenRepository(demurrageContext);
            var userToken = UserTokenFactory.GetUserToken();

            await userTokenRepository.SaveUserToken(userToken);

            await userTokenRepository.RevokeUserTokens(userToken.UserId);

            var revokedTokens = await demurrageContext.UserToken
                .Where(u => u.Token == userToken.Token && u.IsRevoked)
                .ToListAsync();

            Assert.Single(revokedTokens);
        }

        [Fact]
        public async Task RevokeUserTokens_ShouldRevokeAllExpiredUserTokensExceptCurrent()
        {
            var demurrageContext = DbContextFactory.GetDemurrageContextTest();
            var userTokenRepository = new UserTokenRepository(demurrageContext);
            var currentUserToken = UserTokenFactory.GetUserToken();

            var otherUserToken = UserTokenFactory.GetUserToken();
            otherUserToken.Id = 2;
            otherUserToken.Token = "other-token";

            await userTokenRepository.SaveUserToken(currentUserToken);
            await userTokenRepository.SaveUserToken(otherUserToken);

            await userTokenRepository.RevokeUserTokensExceptCurrent(currentUserToken.UserId, currentUserToken.Token);

            var unrevokedUserTokens = await demurrageContext.UserToken
                .Where(u => u.Token == currentUserToken.Token && !u.IsRevoked)
                .ToListAsync();

            var revokedUserTokens = await demurrageContext.UserToken
                .Where(u => u.Token == otherUserToken.Token && u.IsRevoked)
                .ToListAsync();

            Assert.Single(unrevokedUserTokens);
            Assert.Single(revokedUserTokens);
        }

        [Fact]
        public async Task RemoveExpiredUserTokens_ShouldRemoveAllExpiredUserTokens()
        {
            var demurrageContext = DbContextFactory.GetDemurrageContextTest();
            var userTokenRepository = new UserTokenRepository(demurrageContext);
            var userToken = UserTokenFactory.GetUserToken();
            userToken.ExpiryDate = DateTime.UtcNow.AddHours(-1);

            await userTokenRepository.SaveUserToken(userToken);

            await userTokenRepository.RemoveExpiredUserTokens();

            var userTokens = await demurrageContext.UserToken
                .Where(u => u.Token == userToken.Token)
                .ToListAsync();

            Assert.Empty(userTokens);
        }



    }
}

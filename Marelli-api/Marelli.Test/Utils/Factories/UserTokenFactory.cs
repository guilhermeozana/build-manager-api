using Marelli.Domain.Entities;

namespace Marelli.Test.Utils.Factories
{
    public class UserTokenFactory
    {

        public static UserToken GetUserToken()
        {
            return new UserToken()
            {
                ExpiryDate = DateTime.UtcNow.AddDays(1),
                IsRevoked = false,
                Token = "my-token",
                UserId = 1
            };
        }
    }
}

using Marelli.Domain.Dtos;

namespace Marelli.Test.Utils.Factories
{
    public class PasswordFactory
    {

        public static ForgotPasswordRequest GetForgotPasswordRequest()
        {
            return new ForgotPasswordRequest()
            {
                Email = "test@email.com"
            };
        }

        public static ResetPasswordRequest GetResetPasswordRequest()
        {
            return new ResetPasswordRequest()
            {
                Email = "test@email.com",
                NewPassword = "password@123A",
                Token = TokenFactory.GetValidToken()
            };
        }
    }
}

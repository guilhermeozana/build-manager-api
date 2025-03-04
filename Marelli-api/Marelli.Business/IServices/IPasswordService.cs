using Marelli.Domain.Dtos;

namespace Marelli.Business.IServices
{
    public interface IPasswordService
    {

        public Task ForgotPassword(ForgotPasswordRequest model);

        public Task ResetPassword(ResetPasswordRequest model);
    }
}

namespace Marelli.Domain.Dtos
{
    public class ResetPasswordRequest
    {
        public string Token { get; set; }
        public string NewPassword { get; set; }
        public string Email { get; set; }
    }
}

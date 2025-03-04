namespace Marelli.Domain.Dtos
{
    public class TokenResponse
    {
        public string AccessToken { get; set; }

        public double ExpiresIn { get; set; }
    }
}

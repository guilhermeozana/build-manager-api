using Marelli.Domain.Dtos;

namespace Marelli.Test.Utils.Factories
{
    public class TokenFactory
    {
        public static TokenResponse GetTokenResponse()
        {
            return new TokenResponse()
            {
                AccessToken = GetValidToken(),
                ExpiresIn = TimeSpan.FromHours(24).TotalSeconds
            };
        }

        public static string GetValidToken()
        {
            return "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6Im15LWVtYWlsQHRlc3QuY29tIiwicm9sZSI6IkFETUlOIiwibmJmIjoxNzI0NDM4NDMxLCJleHAiOjE3MjQ1MjQ4MzEsImlhdCI6MTcyNDQzODQzMX0.qtaF-XjRHax0t8sKsYf3zPoJCJUjvCXOhP602sn7M-o";
        }

        public static string GetInvalidToken()
        {
            return "eyJhbGciOiAiSFMyNTYiLCAidHlwIjogIkpXVCJ9.eyJzdWIiOiAiMTIzNDU2Nzg5MCIsICJuYW1lIjogIkpvaG4iLCAiaWF0IjogMTYxNzE0NTAwMCwgImV4cCI6IDE2MTcxNDUwMDB9.invalid-signature";
        }
    }
}

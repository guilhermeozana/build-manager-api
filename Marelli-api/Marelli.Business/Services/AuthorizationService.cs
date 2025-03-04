using Marelli.Business.Exceptions;
using Marelli.Business.IServices;
using Marelli.Business.Utils;
using Marelli.Domain.Dtos;
using Marelli.Domain.Entities;

namespace Marelli.Business.Services
{
    public class AuthorizationService : IAuthorizationService
    {
        private readonly IUserService _userService;
        private readonly ITokenService _tokenService;

        public AuthorizationService(IUserService userService, ITokenService tokenService)
        {
            _userService = userService;
            _tokenService = tokenService;
        }

        public async Task<TokenResponse> GenerateToken(string email, string password)
        {
            var user = new User();

            try
            {
                user = await _userService.GetUserWithPassword(email);

                if (!EncryptionUtils.VerifyPassword(password, user.Password))
                {
                    throw new InvalidPasswordException("Incorrect password!");
                }

            }
            catch (Exception)
            {
                throw new UnauthorizedAccessException("Invalid email or password.");
            }

            var tokenResponse = await _tokenService.GenerateAccessToken(user);

            return tokenResponse;
        }

    }
}

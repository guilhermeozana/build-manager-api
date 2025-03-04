using Marelli.Business.Exceptions;
using Marelli.Business.IServices;
using Marelli.Business.Utils;
using Marelli.Domain.Dtos;
using Marelli.Infra.IRepositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Marelli.Business.Services
{
    public class PasswordService : IPasswordService
    {
        private readonly IUserRepository _userRepository;

        private readonly IEmailService _emailService;
        private readonly IUrlHelper _urlHelper;
        private readonly IConfiguration _configuration;
        private readonly ITokenService _tokenService;

        public PasswordService(IUserRepository userRepository, IEmailService emailService, IConfiguration configuration, ITokenService tokenService)
        {
            _userRepository = userRepository;
            _emailService = emailService;
            _configuration = configuration;
            _tokenService = tokenService;
        }

        public async Task ForgotPassword(ForgotPasswordRequest model)
        {
            var user = await _userRepository.GetUserByEmail(model.Email);

            if (user != null)
            {
                var urlHost = _configuration["AppSettings:UrlHost"];
                var changePasswordPath = "/change-password";

                var token = _tokenService.GeneratePasswordResetToken();
                var resetLink = $"{urlHost}{changePasswordPath}?token={token}&email={model.Email}";
                var message = $"Please, reset your password by clicking this link: {resetLink}";
                var subject = "Redefine password";

                await _emailService.SendEmail(model.Email, subject, message);
            }
        }

        public async Task ResetPassword(ResetPasswordRequest model)
        {
            var principal = _tokenService.GetPrincipalFromExpiredToken(model.Token);

            PasswordUtils.VerifyPasswordIsValid(model.NewPassword);

            var encryptedPassword = EncryptionUtils.HashPassword(model.NewPassword);

            var user = await _userRepository.GetUserByEmail(model.Email);

            if (user == null)
            {
                throw new NotFoundException($"User not found with email {model.Email}");
            }

            await _userRepository.UpdateUserPassword(model.Email, encryptedPassword);
        }
    }
}

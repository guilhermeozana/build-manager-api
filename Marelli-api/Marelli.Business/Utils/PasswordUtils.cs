using Marelli.Business.Exceptions;
using System.Text.RegularExpressions;

namespace Marelli.Business.Utils
{
    public class PasswordUtils
    {
        public static void VerifyPasswordIsValid(string password)
        {

            if (password.Length < 8)
            {
                throw new InvalidPasswordException("The password must have at least 8 characters.");
            }
            if (!Regex.IsMatch(password, @"[A-Z]"))
            {
                throw new InvalidPasswordException("The password must have at least one capital letter.");
            }
            if (!Regex.IsMatch(password, @"[a-z]"))
            {
                throw new InvalidPasswordException("The password must have at least one lowercase letter.");
            }
            if (!Regex.IsMatch(password, @"\d"))
            {
                throw new InvalidPasswordException("The password must have at least one number.");
            }
            if (!Regex.IsMatch(password, @"[@!#$%&*]"))
            {
                throw new InvalidPasswordException("The password must have special characters (@, !, #, $, %, etc).");
            }
        }
    }
}

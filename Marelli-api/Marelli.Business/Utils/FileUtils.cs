using Microsoft.AspNetCore.Http;
using System.Security.Cryptography;

namespace Marelli.Business.Utils
{
    public class FileUtils
    {
        public static string CalculateMD5(IFormFile file)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = file.OpenReadStream())
                {
                    byte[] hashBytes = md5.ComputeHash(stream);

                    return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
                }
            }
        }

        //public static string GenerateTag(string fileName)
        //{
        //    using (SHA256 sha256 = SHA256.Create())
        //    {
        //        byte[] bytes = Encoding.UTF8.GetBytes(fileName);

        //        byte[] hashBytes = sha256.ComputeHash(bytes);

        //        string hashString = Convert.ToBase64String(hashBytes)
        //            .Replace('+', '-')
        //            .Replace('/', '_')
        //            .TrimEnd('=');

        //        return hashString.Substring(0, 10);
        //    }
        //}
    }
}

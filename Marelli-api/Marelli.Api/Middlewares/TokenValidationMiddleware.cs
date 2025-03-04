using Marelli.Business.IServices;
using Newtonsoft.Json;
using System.Security.Claims;
using System.Text;


namespace Marelli.Api.Middlewares
{
    public class TokenValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IServiceProvider _serviceProvider;

        public TokenValidationMiddleware(RequestDelegate next, IServiceProvider serviceProvider)
        {
            _next = next;
            _serviceProvider = serviceProvider;
        }

        public async Task Invoke(HttpContext context)
        {
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (token != null)
            {

                using (var scope = _serviceProvider.CreateScope())
                {
                    var userTokenService = scope.ServiceProvider.GetRequiredService<IUserTokenService>();

                    var isUserTokenValid = await userTokenService.VerifyUserTokenValid(token);

                    if (isUserTokenValid)
                    {
                        var identity = new ClaimsIdentity(ParseClaimsFromJwt(token), "jwt");
                        context.User = new ClaimsPrincipal(identity);
                    }
                    else
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        await context.Response.WriteAsync("Invalid token.");
                        return;
                    }
                }

            }

            await _next(context);
        }

        private static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
        {
            var parts = jwt.Split('.');
            if (parts.Length != 3)
                throw new ArgumentException("Invalid JWT token");

            var payload = parts[1];
            var jsonBytes = ParseBase64WithoutPadding(payload);
            var keyValuePairs = JsonConvert.DeserializeObject<Dictionary<string, object>>(Encoding.UTF8.GetString(jsonBytes));

            return keyValuePairs.Select(kvp => new Claim(kvp.Key, kvp.Value.ToString()));
        }

        private static byte[] ParseBase64WithoutPadding(string base64)
        {
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }
            return Convert.FromBase64String(base64.Replace('-', '+').Replace('_', '/'));
        }
    }

}

using Marelli.Api.Configuration;
using Marelli.Business.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Marelli.Api.Controllers;
[ApiController]
[Route("[controller]")]

public class AuthorizeController : ControllerBase
{
    private readonly AppSettings _appSettings;
    private readonly UsuarioService _UsuarioService;
    public AuthorizeController(IOptions<AppSettings> appSettings, UsuarioService usuarioService)
    {
        _appSettings = appSettings.Value;
        _UsuarioService = usuarioService;
    }

    [HttpGet("GetToken")]
    public async Task<IActionResult> GerarToken(string nomeUsuario, string senha)
    {

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
        var usuario = await _UsuarioService.ObtemPorUsuarioSenha(nomeUsuario, senha);
        var claims = new List<Claim>()
        {
            new Claim(ClaimTypes.Name, usuario.NomeUsuario),
            new Claim(ClaimTypes.Role, usuario.Perfil),
        };

        var token = tokenHandler.CreateToken(new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Issuer = _appSettings.Emissor,
            Audience = _appSettings.ValidoEm,
            Expires = DateTime.UtcNow.AddHours(_appSettings.ExpiracaoHoras),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        }); ;
        var encodedToken = tokenHandler.WriteToken(token);

        var response = new
        {
            AccessToken = encodedToken,
            ExpiresIn = TimeSpan.FromHours(_appSettings.ExpiracaoHoras).TotalSeconds,
        };

        return Ok(response);
    }


    //public async Task<IActionResult> GerarToken(string nomeUsuario, string senha)
    //{

    //    var tokenHandler = new JwtSecurityTokenHandler();
    //    var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
    //    var usuario = await _UsuarioService.ObtemPorUsuarioSenha(nomeUsuario, senha);
    //    var claims = new List<Claim>()
    //    {
    //        new Claim(ClaimTypes.Name, usuario.NomeUsuario),
    //        new Claim(ClaimTypes.Role, usuario.Perfil),
    //    };

    //    var token = tokenHandler.CreateToken(new SecurityTokenDescriptor
    //    {
    //        Subject = new ClaimsIdentity(claims),
    //        Issuer = _appSettings.Emissor,
    //        Audience = _appSettings.ValidoEm,
    //        Expires = DateTime.UtcNow.AddHours(_appSettings.ExpiracaoHoras),
    //        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
    //    }); ;
    //    var encodedToken = tokenHandler.WriteToken(token);

    //    var response = new
    //    {
    //        AccessToken = encodedToken,
    //        ExpiresIn = TimeSpan.FromHours(_appSettings.ExpiracaoHoras).TotalSeconds,
    //    };

    //    return Ok(response);
    //}
}

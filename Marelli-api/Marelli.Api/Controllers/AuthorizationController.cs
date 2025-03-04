using Marelli.Business.IServices;
using Microsoft.AspNetCore.Mvc;

namespace Marelli.Api.Controllers;
[Route("api/[controller]")]
[ApiController]

public class AuthorizationController : ControllerBase
{
    private readonly IAuthorizationService _authorizationService;

    public AuthorizationController(IAuthorizationService authorizationService)
    {
        _authorizationService = authorizationService;
    }

    [HttpGet("GetToken/{email}/{password}")]
    public async Task<IActionResult> GenerateToken(string email, string password)
    {
        var tokenResponse = await _authorizationService.GenerateToken(email, password);

        return Ok(tokenResponse);
    }
}

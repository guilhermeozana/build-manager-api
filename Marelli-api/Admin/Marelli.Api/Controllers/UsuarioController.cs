using Marelli.Business.Exceptions;
using Marelli.Business.Services;
using Marelli.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Marelli.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class UsuarioController : Controller
    {
        private readonly UsuarioService _UsuarioService;

        public UsuarioController(UsuarioService UsuarioService)
        {
            _UsuarioService = UsuarioService;
        }

        [HttpPost]
        public async Task<IActionResult> SalvarUsuario([FromBody] Usuario req)
        {
            try
            {
                var retorno = await _UsuarioService.SalvarUsuario(req);
                return Ok(retorno);
            }
            catch (BusinessException bex)
            {
                return BadRequest(bex.BrokenRules?[0]);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }


    }
}

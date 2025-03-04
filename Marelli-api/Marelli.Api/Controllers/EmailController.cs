
using Marelli.Business.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace Marelli.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class EmailController : ControllerBase
    {
        private readonly IEmailService _emailService;


        public EmailController(IEmailService emailService)
        {
            _emailService = emailService;
        }

        [HttpPost]
        [Route("Send/{email}/{subject}/{message}")]
        public async Task<IActionResult> SendEmail(string email, string subject, string message)
        {
            await _emailService.SendEmail(email, subject, message);

            return Ok("Email has been sent successfully.");
        }


    }
}

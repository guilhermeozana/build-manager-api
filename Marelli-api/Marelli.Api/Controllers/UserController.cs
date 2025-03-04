using Marelli.Business.IServices;
using Marelli.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Marelli.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : Controller
    {
        private readonly IUserService _UserService;

        public UserController(IUserService UserService)
        {
            _UserService = UserService;
        }

        [HttpPost("Save")]
        public async Task<IActionResult> SaveUser([FromBody] User req)
        {
            var roleClaim = User.Claims.FirstOrDefault(c => c.Type == "role")?.Value;

            var allowedRoles = new List<string> { "Administrator", "Project Manager", "Software Manager" };

            if (roleClaim != null && !allowedRoles.Contains(roleClaim))
            {
                return Unauthorized("Access denied: You do not have permission to perform this action.");
            }

            var response = await _UserService.SaveUser(req);

            return Ok(response);
        }

        [HttpGet("List")]
        public async Task<IActionResult> ListUsers()
        {
            var response = await _UserService.ListUsers();

            return Ok(response);
        }


        [HttpGet("GetByEmail/{email}")]
        public async Task<IActionResult> GetUserByEmail(string email)
        {
            var user = await _UserService.GetUserResponseByEmail(email);

            return Ok(user);
        }

        [HttpPut("Update/{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] User req)
        {
            var roleClaim = User.Claims.FirstOrDefault(c => c.Type == "role")?.Value;

            var allowedRoles = new List<string> { "Administrator", "Project Manager", "Software Manager" };

            if (roleClaim != null && !allowedRoles.Contains(roleClaim))
            {
                return Unauthorized("Access denied: You do not have permission to perform this action.");
            }

            var response = await _UserService.UpdateUser(id, req);

            return Ok(response);
        }

        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var roleClaim = User.Claims.FirstOrDefault(c => c.Type == "role")?.Value;

            var allowedRoles = new List<string> { "Administrator", "Project Manager", "Software Manager" };

            if (roleClaim != null && !allowedRoles.Contains(roleClaim))
            {
                return Unauthorized("Access denied: You do not have permission to perform this action.");
            }

            var response = await _UserService.DeleteUser(id);

            return Ok(response);
        }
    }
}

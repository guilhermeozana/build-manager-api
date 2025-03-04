using Marelli.Business.IServices;
using Marelli.Domain.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Marelli.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class GroupController : Controller
    {
        private readonly IGroupService _groupService;

        public GroupController(IGroupService groupService)
        {
            _groupService = groupService;
        }

        [HttpPost("Save")]
        public async Task<IActionResult> SaveGroup([FromBody] GroupRequest req)
        {
            var roleClaim = User.Claims.FirstOrDefault(c => c.Type == "role")?.Value;

            var allowedRoles = new List<string> { "Administrator", "Project Manager", "Software Manager" };

            if (roleClaim != null && !allowedRoles.Contains(roleClaim))
            {
                return Unauthorized("Access denied: You do not have permission to perform this action.");
            }

            var savedGroup = await _groupService.SaveGroup(req);

            return Ok(savedGroup);
        }

        [HttpGet("List")]
        public async Task<IActionResult> ListGroups()
        {
            var groups = await _groupService.ListGroups();

            return Ok(groups);
        }

        [HttpGet("List/{name}")]
        public async Task<IActionResult> ListGroupsByName(string name)
        {
            var groups = await _groupService.ListGroups();

            var result = groups.Where(x => x.Name == name).ToList();

            return Ok(result);
        }

        [HttpPut("Update/{id}")]
        public async Task<IActionResult> UpdateGroup(int id, [FromBody] GroupRequest req)
        {
            var roleClaim = User.Claims.FirstOrDefault(c => c.Type == "role")?.Value;

            var allowedRoles = new List<string> { "Administrator", "Project Manager", "Software Manager" };

            if (roleClaim != null && !allowedRoles.Contains(roleClaim))
            {
                return Unauthorized("Access denied: You do not have permission to perform this action.");
            }

            var groupUpdated = await _groupService.UpdateGroup(id, req);

            return Ok(groupUpdated);
        }

        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> DeleteGroup(int id)
        {
            var roleClaim = User.Claims.FirstOrDefault(c => c.Type == "role")?.Value;

            var allowedRoles = new List<string> { "Administrator", "Project Manager", "Software Manager" };

            if (roleClaim != null && !allowedRoles.Contains(roleClaim))
            {
                return Unauthorized("Access denied: You do not have permission to perform this action.");
            }

            var groupDeleted = await _groupService.DeleteGroup(id);

            return Ok(groupDeleted);
        }

    }
}

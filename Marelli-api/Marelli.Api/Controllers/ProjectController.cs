using Marelli.Business.IServices;
using Marelli.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Marelli.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProjectController : Controller
    {
        private readonly IProjectService _projectService;

        public ProjectController(IProjectService projectService)
        {
            _projectService = projectService;
        }

        [HttpPost("Save")]
        public async Task<IActionResult> SaveProject([FromBody] Project req)
        {
            var roleClaim = User.Claims.FirstOrDefault(c => c.Type == "role")?.Value;

            var allowedRoles = new List<string> { "Administrator", "Project Manager", "Software Manager" };

            if (roleClaim != null && !allowedRoles.Contains(roleClaim))
            {
                return Unauthorized("Access denied: You do not have permission to perform this action.");
            }

            var savedProject = await _projectService.SaveProject(req);

            return Ok(savedProject);
        }

        [HttpGet("List/{userId}")]
        public async Task<IActionResult> ListProjects(int userId)
        {
            var roleClaim = User.Claims.FirstOrDefault(c => c.Type == "role")?.Value;

            if (roleClaim != null && (roleClaim.Equals("Administrator")))
            {
                userId = 0;
            }

            var projects = await _projectService.ListProjects(userId);

            return Ok(projects);
        }

        [HttpGet("ListByName/{name}")]
        public async Task<IActionResult> ListProjectsByName(string name)
        {
            var response = await _projectService.ListProjectsByName(name);

            return Ok(response);
        }

        [HttpPut("Update/{id}")]
        public async Task<IActionResult> UpdateProject(int id, [FromBody] Project req)
        {
            var roleClaim = User.Claims.FirstOrDefault(c => c.Type == "role")?.Value;

            var allowedRoles = new List<string> { "Administrator", "Project Manager", "Software Manager", "Software Project Leader" };

            if (roleClaim != null && !allowedRoles.Contains(roleClaim))
            {
                return Unauthorized("Access denied: You do not have permission to perform this action.");
            }

            var response = await _projectService.UpdateProject(id, req);

            return Ok(response);
        }

        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> DeleteProject(int id)
        {
            var roleClaim = User.Claims.FirstOrDefault(c => c.Type == "role")?.Value;

            var allowedRoles = new List<string> { "Administrator", "Project Manager", "Software Manager" };

            if (roleClaim != null && !allowedRoles.Contains(roleClaim))
            {
                return Unauthorized("Access denied: You do not have permission to perform this action.");
            }

            var response = await _projectService.DeleteProjectById(id);

            return Ok(response);
        }

    }
}

using Marelli.Business.IServices;
using Marelli.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Marelli.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BaselineController : ControllerBase
    {
        private readonly IBaselineService _baselineService;

        public BaselineController(IBaselineService baselineService)
        {
            _baselineService = baselineService;
        }

        [HttpPost("Save")]
        public async Task<IActionResult> SaveBaseline([FromBody] Baseline req)
        {
            var roleClaim = User.Claims.FirstOrDefault(c => c.Type == "role")?.Value;

            var allowedRoles = new List<string> { "Administrator", "Software Manager", "Software Project Leader", "Domain Leader", "Integrator" };

            if (roleClaim != null && !allowedRoles.Contains(roleClaim))
            {
                return Unauthorized("Access denied: You do not have permission to perform this action.");
            }

            var baseline = await _baselineService.SaveBaseline(req);

            return Ok(baseline);
        }

        [HttpGet("List/{projectId}")]
        public async Task<IActionResult> ListBaseline(int projectId)
        {
            var baseline = await _baselineService.ListBaseline(projectId);

            return Ok(baseline);
        }

        [HttpGet("Get/{id}")]
        public async Task<IActionResult> GetBaseline(int id)
        {
            var baseline = await _baselineService.GetBaseline(id);

            return Ok(baseline);
        }

        [HttpPut("Update/{id}")]
        public async Task<IActionResult> UpdateBaseline(int id, [FromBody] Baseline req)
        {
            var roleClaim = User.Claims.FirstOrDefault(c => c.Type == "role")?.Value;

            var allowedRoles = new List<string> { "Administrator", "Software Manager", "Software Project Leader", "Domain Leader", "Integrator" };

            if (roleClaim != null && !allowedRoles.Contains(roleClaim))
            {
                return Unauthorized("Access denied: You do not have permission to perform this action.");
            }

            var baseline = await _baselineService.UpdateBaseline(id, req);

            return Ok(baseline);
        }

        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> DeleteBaseline(int id)
        {
            var roleClaim = User.Claims.FirstOrDefault(c => c.Type == "role")?.Value;

            var allowedRoles = new List<string> { "Administrator", "Software Manager", "Software Project Leader", "Domain Leader", "Integrator" };

            if (roleClaim != null && !allowedRoles.Contains(roleClaim))
            {
                return Unauthorized("Access denied: You do not have permission to perform this action.");
            }

            var baseline = await _baselineService.DeleteBaseline(id);

            return Ok(baseline);
        }

    }
}

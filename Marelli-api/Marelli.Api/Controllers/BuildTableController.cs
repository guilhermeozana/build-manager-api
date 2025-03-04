using Marelli.Business.IServices;
using Marelli.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Marelli.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BuildTableController : ControllerBase
    {
        private readonly IBuildTableRowService _BuildTableRowsServices;

        public BuildTableController(IBuildTableRowService buildTableRowsServices)
        {
            _BuildTableRowsServices = buildTableRowsServices;
        }

        [HttpPost("Save")]
        public async Task<IActionResult> SaveBuildTable([FromBody] BuildTableRow req)
        {
            var roleClaim = User.Claims.FirstOrDefault(c => c.Type == "role")?.Value;

            var allowedRoles = new List<string> { "Administrator", "Software Project Leader", "Domain Leader", "Integrator", "Developer", "Customer Software Project Leader", "Customer Developer", "Customer Integrator" };

            if (roleClaim != null && !allowedRoles.Contains(roleClaim))
            {
                return Unauthorized("Access denied: You do not have permission to perform this action.");
            }

            var buildTable = await _BuildTableRowsServices.SaveBuildTable(req);

            return Ok(buildTable);
        }

        [HttpGet("List/{userId}")]
        public async Task<IActionResult> ListBuildTable(int userId)
        {
            var roleClaim = User.Claims.FirstOrDefault(c => c.Type == "role")?.Value;

            var allowedRoles = new List<string> { "Administrator", "Software Project Leader", "Domain Leader", "Integrator", "Developer", "Customer Software Project Leader", "Customer Developer", "Customer Integrator" };

            if (roleClaim != null && !allowedRoles.Contains(roleClaim))
            {
                return Unauthorized("Access denied: You do not have permission to perform this action.");
            }

            if (roleClaim != null && roleClaim == "Administrator")
            {
                userId = 0;
            }

            var buildTable = await _BuildTableRowsServices.ListBuildTable(userId);

            return Ok(buildTable);
        }

        [HttpGet("ListInProgress/{userId}")]
        public async Task<IActionResult> ListBuildTableInProgress(int userId)
        {
            var roleClaim = User.Claims.FirstOrDefault(c => c.Type == "role")?.Value;

            var allowedRoles = new List<string> { "Administrator", "Software Project Leader", "Domain Leader", "Integrator", "Developer", "Customer Software Project Leader", "Customer Developer", "Customer Integrator" };

            if (roleClaim != null && !allowedRoles.Contains(roleClaim))
            {
                return Unauthorized("Access denied: You do not have permission to perform this action.");
            }

            if (roleClaim != null && roleClaim == "Administrator")
            {
                userId = 0;
            }

            var buildTable = await _BuildTableRowsServices.ListBuildTableInProgress(userId);

            return Ok(buildTable);
        }

        [HttpGet("ListInQueue")]
        public async Task<IActionResult> ListBuildTableInQueue()
        {
            var roleClaim = User.Claims.FirstOrDefault(c => c.Type == "role")?.Value;

            var allowedRoles = new List<string> { "Administrator", "Software Project Leader", "Domain Leader", "Integrator", "Developer", "Customer Software Project Leader", "Customer Developer", "Customer Integrator" };

            if (roleClaim != null && !allowedRoles.Contains(roleClaim))
            {
                return Unauthorized("Access denied: You do not have permission to perform this action.");
            }

            var buildTable = await _BuildTableRowsServices.ListBuildTableInQueue();

            return Ok(buildTable);
        }

        [HttpGet("Get/{id}")]
        public async Task<IActionResult> GetBuildTable(int id)
        {
            var roleClaim = User.Claims.FirstOrDefault(c => c.Type == "role")?.Value;

            var allowedRoles = new List<string> { "Administrator", "Software Project Leader", "Domain Leader", "Integrator", "Developer", "Customer Software Project Leader", "Customer Developer", "Customer Integrator" };

            if (roleClaim != null && !allowedRoles.Contains(roleClaim))
            {
                return Unauthorized("Access denied: You do not have permission to perform this action.");
            }

            var buildTable = await _BuildTableRowsServices.GetBuildTable(id);

            return Ok(buildTable);
        }

        [HttpGet("GetLastUploaded/{userId}")]
        public async Task<IActionResult> GetLastUploaded(int userId)
        {
            var roleClaim = User.Claims.FirstOrDefault(c => c.Type == "role")?.Value;

            var allowedRoles = new List<string> { "Administrator", "Software Project Leader", "Domain Leader", "Integrator", "Developer", "Customer Software Project Leader", "Customer Developer", "Customer Integrator" };

            if (roleClaim != null && !allowedRoles.Contains(roleClaim))
            {
                return Unauthorized("Access denied: You do not have permission to perform this action.");
            }

            var buildTable = await _BuildTableRowsServices.GetLastUploadedByUser(userId);

            return Ok(buildTable);
        }

        [HttpGet("GetFirstInQueue/{userId}")]
        public async Task<IActionResult> GetFirstInQueue(int userId)
        {
            var roleClaim = User.Claims.FirstOrDefault(c => c.Type == "role")?.Value;

            if (roleClaim != null && (roleClaim.Equals("Administrator")))
            {
                userId = 0;
            }

            var buildTable = await _BuildTableRowsServices.GetFirstInQueue(userId);

            return Ok(buildTable);
        }

        [HttpPut("Update/{id}")]
        public async Task<IActionResult> UpdateBuildTable(int id, [FromBody] BuildTableRow req)
        {
            var roleClaim = User.Claims.FirstOrDefault(c => c.Type == "role")?.Value;

            var allowedRoles = new List<string> { "Administrator", "Software Project Leader", "Domain Leader", "Integrator", "Developer", "Customer Software Project Leader", "Customer Developer", "Customer Integrator" };

            if (roleClaim != null && !allowedRoles.Contains(roleClaim))
            {
                return Unauthorized("Access denied: You do not have permission to perform this action.");
            }

            var buildTable = await _BuildTableRowsServices.UpdateBuildTable(id, req);

            return Ok(buildTable);
        }

        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> DeleteBuildTable(int id)
        {
            var roleClaim = User.Claims.FirstOrDefault(c => c.Type == "role")?.Value;

            var allowedRoles = new List<string> { "Administrator", "Software Project Leader", "Domain Leader", "Integrator", "Developer", "Customer Software Project Leader", "Customer Developer", "Customer Integrator" };

            if (roleClaim != null && !allowedRoles.Contains(roleClaim))
            {
                return Unauthorized("Access denied: You do not have permission to perform this action.");
            }

            var buildTable = await _BuildTableRowsServices.DeleteBuildTable(id);

            return Ok(buildTable);
        }

    }
}
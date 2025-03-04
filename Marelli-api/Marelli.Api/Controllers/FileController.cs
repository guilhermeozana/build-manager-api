using Marelli.Business.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Marelli.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FileController : ControllerBase
    {
        private readonly IFileService _fileService;


        public FileController(IFileService fileService)
        {
            _fileService = fileService;
        }

        [HttpPost]
        [Route("AWS/UploadZip/{userId}/{projectId}")]
        public async Task<IActionResult> UploadZip(IFormFile file, int userId, int projectId)
        {
            var roleClaim = User.Claims.FirstOrDefault(c => c.Type == "role")?.Value;

            var allowedRoles = new List<string> { "Administrator", "Software Project Leader", "Domain Leader", "Integrator", "Developer", "Customer Software Project Leader", "Customer Developer", "Customer Integrator" };

            if (roleClaim != null && !allowedRoles.Contains(roleClaim))
            {
                return Unauthorized("Access denied: You do not have permission to perform this action.");
            }

            var buildSaved = await _fileService.UploadZip(file, userId, projectId);

            return Ok(buildSaved);
        }

        [HttpGet]
        [Route("AWS/DownloadZip/{buildId}")]
        public async Task<IActionResult> DownloadZip(int buildId)
        {
            var roleClaim = User.Claims.FirstOrDefault(c => c.Type == "role")?.Value;

            var allowedRoles = new List<string> { "Administrator", "Software Project Leader", "Domain Leader", "Integrator", "Developer", "Customer Software Project Leader", "Customer Developer", "Customer Integrator" };

            if (roleClaim != null && !allowedRoles.Contains(roleClaim))
            {
                return Unauthorized("Access denied: You do not have permission to perform this action.");
            }

            var fileResponse = await _fileService.DownloadZip(buildId);

            return Ok(fileResponse);

        }

        [HttpDelete]
        [Route("AWS/RemoveZip/{userId}/{projectId}/{buildId}")]
        public async Task<IActionResult> RemoveZip(int userId, int projectId, int buildId)
        {
            var roleClaim = User.Claims.FirstOrDefault(c => c.Type == "role")?.Value;

            var allowedRoles = new List<string> { "Administrator", "Software Project Leader", "Domain Leader", "Integrator", "Developer", "Customer Software Project Leader", "Customer Developer", "Customer Integrator" };

            if (roleClaim != null && !allowedRoles.Contains(roleClaim))
            {
                return Unauthorized("Access denied: You do not have permission to perform this action.");
            }

            await _fileService.RemoveZip(userId, projectId, buildId);

            return Ok("File deleted successfully.");
        }

        [HttpPost]
        [Route("AWS/UploadBaseline/{projectId}/{description}")]
        public async Task<IActionResult> UploadBaseline(IFormFile file, int projectId, string description)
        {
            var roleClaim = User.Claims.FirstOrDefault(c => c.Type == "role")?.Value;

            var allowedRoles = new List<string> { "Administrator", "Software Manager", "Software Project Leader", "Domain Leader", "Integrator" };

            if (roleClaim != null && !allowedRoles.Contains(roleClaim))
            {
                return Unauthorized("Access denied: You do not have permission to perform this action.");
            }

            var baseline = await _fileService.UploadBaseline(file, projectId, description);

            return Ok(baseline);
        }

        [HttpGet]
        [Route("AWS/DownloadBaseline/{baselineId}")]
        public async Task<IActionResult> DownloadBaseline(int baselineId)
        {
            var roleClaim = User.Claims.FirstOrDefault(c => c.Type == "role")?.Value;

            var allowedRoles = new List<string> { "Administrator", "Software Manager", "Software Project Leader", "Domain Leader", "Integrator" };

            if (roleClaim != null && !allowedRoles.Contains(roleClaim))
            {
                return Unauthorized("Access denied: You do not have permission to perform this action.");
            }

            var fileResponse = await _fileService.DownloadBaseline(baselineId);

            return Ok(fileResponse);

        }

        [HttpDelete]
        [Route("AWS/RemoveBaseline/{baselineId}")]
        public async Task<IActionResult> RemoveBaseline(int baselineId)
        {
            var roleClaim = User.Claims.FirstOrDefault(c => c.Type == "role")?.Value;

            var allowedRoles = new List<string> { "Administrator", "Software Manager", "Software Project Leader", "Domain Leader", "Integrator" };

            if (roleClaim != null && !allowedRoles.Contains(roleClaim))
            {
                return Unauthorized("Access denied: You do not have permission to perform this action.");
            }

            await _fileService.RemoveBaseline(baselineId);

            return Ok("Baseline deleted successfully.");
        }

        [HttpGet]
        [Route("AWS/DownloadLogs/{buildId}")]
        public async Task<IActionResult> DownloadLogs(int buildId)
        {
            var roleClaim = User.Claims.FirstOrDefault(c => c.Type == "role")?.Value;

            var allowedRoles = new List<string> { "Administrator", "Project Manager", "Software Manager", "Software Project Leader", "Domain Leader", "Integrator", "Developer", "Customer Project Manager", "Customer Software Project Leader" };

            if (roleClaim != null && !allowedRoles.Contains(roleClaim))
            {
                return Unauthorized("Access denied: You do not have permission to perform this action.");
            }

            var fileResponse = await _fileService.DownloadLogs(buildId);

            return Ok(fileResponse);

        }
    }
}

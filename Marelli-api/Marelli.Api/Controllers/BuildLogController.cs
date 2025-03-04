using Marelli.Business.IServices;
using Marelli.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Marelli.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BuildLogController : ControllerBase
    {
        private readonly IBuildLogService _BuildLogService;

        public BuildLogController(IBuildLogService buildLogService)
        {
            _BuildLogService = buildLogService;
        }

        [HttpPost("Save")]
        public async Task<IActionResult> SaveBuildLog([FromBody] BuildLog req)
        {
            var buildLog = await _BuildLogService.SaveBuildLog(req);

            return Ok(buildLog);
        }

        [HttpGet("List/{buildId}")]
        public async Task<IActionResult> ListBuildLog(int buildId)
        {
            var buildLog = await _BuildLogService.ListBuildLog(buildId);
            return Ok(buildLog);
        }

        [HttpGet("Get/{id}")]
        public async Task<IActionResult> GetBuildLog(int id)
        {
            var buildLog = await _BuildLogService.GetBuildLog(id);

            return Ok(buildLog);
        }

        [HttpPut("Update/{id}")]
        public async Task<IActionResult> UpdateBuildLog(int id, [FromBody] BuildLog req)
        {
            var buildLog = await _BuildLogService.UpdateBuildLog(id, req);

            return Ok(buildLog);
        }

        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> DeleteBuildLog(int id)
        {
            var buildLog = await _BuildLogService.DeleteBuildLog(id);

            return Ok(buildLog);
        }

    }
}

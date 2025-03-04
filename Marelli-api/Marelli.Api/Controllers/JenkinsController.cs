using Marelli.Business.IServices;
using Marelli.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Marelli.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class JenkinsController : ControllerBase
    {
        private IJenkinsService _jenkinsService;
        private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public JenkinsController(IJenkinsService jenkinsService)
        {
            _jenkinsService = jenkinsService;
        }

        [HttpPost]
        [Route("Invoke/{userId}/{projectId}/{buildId}/{sendNotification}/{rebuild}")]
        public async Task<IActionResult> Invoke(int userId, int projectId, int buildId, bool sendNotification, bool rebuild)
        {
            var buildingState = new BuildingState();

            await _semaphore.WaitAsync();
            try
            {
                buildingState = await _jenkinsService.Invoke(userId, projectId, buildId, sendNotification, rebuild);
            }
            finally
            {
                _semaphore.Release();
            }
            return Ok(buildingState);
        }

        [HttpPost]
        [Route("StopBuild/{buildId}")]
        public async Task<IActionResult> StopBuild(int buildId)
        {
            await _jenkinsService.StopBuild(buildId);

            return Ok("Build stopped successfully.");
        }

        [HttpGet]
        [Route("GetAllData/{userId}/{projectId}")]
        public async Task<IActionResult> GetAllData(int userId, int projectId)
        {
            var allData = await _jenkinsService.GetAllData(userId, projectId);

            return Ok(allData);
        }


        [HttpPost("BuildingState/Save")]
        public async Task<IActionResult> SaveBuildingState([FromBody] BuildingState req)
        {
            var retorno = await _jenkinsService.SaveBuildingState(req);

            return Ok(retorno);
        }

        [HttpGet("BuildingState/List/{userId}/{projectId}")]
        public async Task<IActionResult> ListBuildingState(int userId, int projectId)
        {
            var buildingState = await _jenkinsService.ListBuildingState(userId, projectId);

            return Ok(buildingState);
        }

        [HttpGet("BuildingState/Get/{id}")]
        public async Task<IActionResult> GetBuildingState(int id)
        {
            var buildingState = await _jenkinsService.GetBuildingStateById(id);

            return Ok(buildingState);
        }

        [HttpGet("BuildingState/GetByBuildId/{buildId}")]
        public async Task<IActionResult> GetBuildingStateByBuildId(int buildId)
        {
            var buildingState = await _jenkinsService.GetBuildingStateByBuildId(buildId);

            return Ok(buildingState);
        }

        [HttpPut("BuildingState/Update/{id}")]
        public async Task<IActionResult> UpdateBuildingState(int id, [FromBody] BuildingState req)
        {
            var retorno = await _jenkinsService.UpdateBuildingState(id, req);

            return Ok(retorno);
        }

        [HttpDelete("BuildingState/Delete/{id}")]
        public async Task<IActionResult> DeleteBuildingState(int id)
        {
            var retorno = await _jenkinsService.DeleteBuildingState(id);

            return Ok(retorno);
        }


        [HttpPost("LogAndArtifact/Save")]
        public async Task<IActionResult> SaveLogAndArtifact([FromBody] LogAndArtifact req)
        {
            var logAndArtifact = await _jenkinsService.SaveLogAndArtifact(req);

            return Ok(logAndArtifact);
        }

        [HttpGet("LogAndArtifact/List/{userId}/{projectId}")]
        public async Task<IActionResult> ListLogAndArtifact(int userId, int projectId)
        {
            var logAndArtifact = await _jenkinsService.ListLogAndArtifact(userId, projectId);
            return Ok(logAndArtifact);
        }

        [HttpGet("LogAndArtifact/Get/{id}")]
        public async Task<IActionResult> GetLogAndArtifact(int id)
        {
            var logAndArtifact = await _jenkinsService.GetLogAndArtifactById(id);

            return Ok(logAndArtifact);
        }

        [HttpPut("LogAndArtifact/Update/{id}")]
        public async Task<IActionResult> UpdateLogAndArtifact(int id, [FromBody] LogAndArtifact req)
        {
            var logAndArtifact = await _jenkinsService.UpdateLogAndArtifact(id, req);

            return Ok(logAndArtifact);
        }

        [HttpDelete("LogAndArtifact/Delete/{id}")]
        public async Task<IActionResult> DeleteLogAndArtifact(int id)
        {
            var logAndArtifact = await _jenkinsService.DeleteLogAndArtifact(id);

            return Ok(logAndArtifact);
        }


        [HttpPost("FileVerify/Save")]
        public async Task<IActionResult> SaveFileVerify([FromBody] FileVerify req)
        {

            var file = await _jenkinsService.SaveFileVerify(req);
            return Ok(file);
        }

        [HttpGet("FileVerify/List/{userId}/{projectId}")]
        public async Task<IActionResult> ListFileVerify(int userId, int projectId)
        {
            var fileVerify = await _jenkinsService.ListFileVerify(userId, projectId);

            return Ok(fileVerify);
        }

        [HttpGet("FileVerify/Get/{id}")]
        public async Task<IActionResult> GetFileVerify(int id)
        {
            var fileVerify = await _jenkinsService.GetFileVerifyById(id);

            return Ok(fileVerify);
        }

        [HttpPut("FileVerify/Update/{id}")]
        public async Task<IActionResult> UpdateFileVerify(int id, [FromBody] FileVerify req)
        {
            var fileVerify = await _jenkinsService.UpdateFileVerify(id, req);

            return Ok(fileVerify);
        }

        [HttpDelete("FileVerify/Delete/{id}")]
        public async Task<IActionResult> DeleteFileVerify(int id)
        {
            var fileVerify = await _jenkinsService.DeleteFileVerify(id);

            return Ok(fileVerify);
        }
    }
}
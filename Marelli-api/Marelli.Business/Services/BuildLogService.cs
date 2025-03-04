using Marelli.Business.Exceptions;
using Marelli.Business.Hubs;
using Marelli.Business.IServices;
using Marelli.Domain.Entities;
using Marelli.Infra.IRepositories;
using Microsoft.AspNetCore.SignalR;

namespace Marelli.Business.Services
{

    public class BuildLogService : IBuildLogService
    {
        private readonly IBuildLogRepository _buildLogRepository;
        //private readonly ValidadoresHelper _validadoresHelper;
        private readonly IHubContext<BuildLogHub> _hubContext;


        public BuildLogService(IBuildLogRepository buildLogRepository, IHubContext<BuildLogHub> hubContext)
        {
            _buildLogRepository = buildLogRepository;
            //_validadoresHelper = new ValidadoresHelper();
            _hubContext = hubContext;
        }

        public async Task<BuildLog> SaveBuildLog(BuildLog request)
        {
            var buildLog = await _buildLogRepository.SaveBuildLog(request);

            var buildLogs = await ListBuildLog(request.UserId);

            await _hubContext.Clients.All.SendAsync("ReceiveBuildLogs", buildLogs);

            return buildLog;
        }

        public async Task<List<BuildLog>> ListBuildLog(int buildId)
        {
            return await _buildLogRepository.ListBuildLog(buildId);
        }

        public async Task<BuildLog> GetBuildLog(int id)
        {
            var buildLog = await _buildLogRepository.GetBuildLog(id);

            if (buildLog == null)
            {
                throw new NotFoundException($"Build Log not found with ID {id}.");
            }

            return buildLog;
        }

        public async Task<int> UpdateBuildLog(int id, BuildLog request)
        {
            var buildLog = await GetBuildLog(id);

            var updated = await _buildLogRepository.UpdateBuildLog(id, buildLog, request);

            var buildLogs = await ListBuildLog(request.UserId);

            await _hubContext.Clients.All.SendAsync("ReceiveBuildLogs", buildLogs);

            return updated;
        }

        public async Task<int> DeleteBuildLog(int id)
        {
            var buildLog = await GetBuildLog(id);

            return await _buildLogRepository.DeleteBuildLog(buildLog);
        }

        public async Task<int> DeleteBuildLogsByBuildId(int buildId)
        {
            return await _buildLogRepository.DeleteBuildLogsByBuildId(buildId);
        }

    }
}
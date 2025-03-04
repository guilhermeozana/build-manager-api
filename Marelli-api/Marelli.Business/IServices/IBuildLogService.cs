using Marelli.Domain.Entities;

namespace Marelli.Business.IServices
{
    public interface IBuildLogService
    {
        public Task<BuildLog> SaveBuildLog(BuildLog request);

        public Task<List<BuildLog>> ListBuildLog(int buildId);

        public Task<BuildLog> GetBuildLog(int id);

        public Task<int> UpdateBuildLog(int id, BuildLog request);

        public Task<int> DeleteBuildLog(int id);

        public Task<int> DeleteBuildLogsByBuildId(int buildId);

    }
}

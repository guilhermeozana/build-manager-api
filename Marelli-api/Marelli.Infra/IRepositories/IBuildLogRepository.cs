using Marelli.Domain.Entities;

namespace Marelli.Infra.IRepositories
{
    public interface IBuildLogRepository
    {
        public Task<BuildLog> SaveBuildLog(BuildLog entity);

        public Task<List<BuildLog>> ListBuildLog(int buildId);

        public Task<BuildLog> GetBuildLog(int id);

        public Task<int> UpdateBuildLog(int id, BuildLog current, BuildLog request);

        public Task<int> DeleteBuildLog(BuildLog buildLog);

        public Task<int> DeleteBuildLogsByBuildId(int buildId);
    }
}

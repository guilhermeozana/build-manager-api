using Marelli.Domain.Dtos;
using Marelli.Domain.Entities;

namespace Marelli.Business.IServices
{
    public interface IJenkinsService
    {
        public Task<BuildingState> Invoke(int userId, int projectId, int buildId, bool sendNotification, bool rebuild);
        public Task StopBuild(int buildId);
        public Task<JenkinsAllDataResponse> GetAllData(int userId, int projectId);

        public Task<BuildingState> SaveBuildingState(BuildingState request);

        public Task<List<BuildingState>> ListBuildingState(int userId, int projectId);

        public Task<BuildingState> GetBuildingStateById(int id);

        public Task<BuildingState> GetBuildingStateByBuildId(int buildId);

        public Task<BuildingState> UpdateBuildingState(int id, BuildingState request);

        public Task<int> DeleteBuildingState(int id);

        public Task<int> SaveLogAndArtifact(LogAndArtifact request);

        public Task<List<LogAndArtifact>> ListLogAndArtifact(int userId, int projectId);

        public Task<LogAndArtifact> GetLogAndArtifactById(int id);

        public Task<LogAndArtifact> GetLogAndArtifactByBuildId(int buildId);

        public Task<int> UpdateLogAndArtifact(int id, LogAndArtifact request);

        public Task<int> DeleteLogAndArtifact(int id);

        public Task<int> SaveFileVerify(FileVerify request);

        public Task<List<FileVerify>> ListFileVerify(int userId, int projectId);

        public Task<FileVerify> GetFileVerifyById(int id);

        public Task<int> UpdateFileVerify(int id, FileVerify request);

        public Task<int> DeleteFileVerify(int id);
    }
}

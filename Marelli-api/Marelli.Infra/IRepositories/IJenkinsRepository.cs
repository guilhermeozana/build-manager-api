using Marelli.Domain.Dtos;
using Marelli.Domain.Entities;

namespace Marelli.Infra.IRepositories
{
    public interface IJenkinsRepository
    {
        public Task<JenkinsAllDataResponse> GetAllData(int userId, int projectId);

        //Building State

        public Task<BuildingState> SaveBuildingState(BuildingState entity);

        public Task<List<BuildingState>> ListBuildingState(int userId, int projectId);

        public Task<BuildingState> GetBuildingStateById(int id);

        public Task<BuildingState> GetBuildingStateByBuildId(int buildId);

        public Task<BuildingState> UpdateBuildingState(int id, BuildingState current, BuildingState updated);

        public Task<int> DeleteBuildingState(BuildingState buildingState);

        //Log And Artifact

        public Task<int> SaveLogAndArtifact(LogAndArtifact entity);

        public Task<List<LogAndArtifact>> ListLogAndArtifact(int userId, int projectId);

        public Task<LogAndArtifact> GetLogAndArtifactById(int id);

        public Task<LogAndArtifact> GetLogAndArtifactByBuildId(int buildId);

        public Task<int> UpdateLogAndArtifact(int id, LogAndArtifact current, LogAndArtifact updated);

        public Task<int> DeleteLogAndArtifact(LogAndArtifact logAndArtifact);

        //File Verify

        public Task<int> SaveFileVerify(FileVerify entity);

        public Task<List<FileVerify>> ListFileVerify(int userId, int projectId);

        public Task<FileVerify> GetFileVerifyById(int id);

        public Task<FileVerify> GetFileVerifyByBuildId(int buildId);

        public Task<int> UpdateFileVerify(int id, FileVerify current, FileVerify updated);

        public Task<int> DeleteFileVerify(FileVerify fileVerify);
    }
}

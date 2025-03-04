using Marelli.Domain.Entities;

namespace Marelli.Business.IServices
{
    public interface IBuildTableRowService
    {
        public Task<BuildTableRow> SaveBuildTable(BuildTableRow request);

        public Task<List<BuildTableRow>> ListBuildTable(int userId);
        public Task<List<BuildTableRow>> ListBuildTableInProgress(int userId);
        public Task<List<BuildTableRow>> ListBuildTableInQueue();
        public Task<List<BuildTableRow>> ListBuildTableByProject(int projectId);

        public Task<List<BuildTableRow>> ListAllInProgressBuildsOlderThan(int hours, CancellationToken cancellationToken);

        public Task<BuildTableRow> GetBuildTable(int id);

        public Task<BuildTableRow> GetLastUploadedByUser(int userId);
        public Task<BuildTableRow> GetLastUploadedByUserProjects(int userId);
        public Task<BuildTableRow> GetFirstInQueue(int userId);
        public Task<int> UpdateBuildTable(int id, BuildTableRow request);

        public Task<int> DeleteBuildTable(int id);

        public Task<int> DeleteBuildTable(BuildTableRow buildTable);
    }
}

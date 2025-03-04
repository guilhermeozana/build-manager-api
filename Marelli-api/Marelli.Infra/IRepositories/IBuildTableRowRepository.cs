using Marelli.Domain.Entities;

namespace Marelli.Infra.IRepositories
{
    public interface IBuildTableRowRepository
    {
        public Task<BuildTableRow> SaveBuildTableAsync(BuildTableRow entity);

        public Task<List<BuildTableRow>> ListBuildTableAsync(int userId);
        public Task<List<BuildTableRow>> ListBuildTableInProgressAsync(int userId);
        public Task<List<BuildTableRow>> ListBuildTableInQueueAsync();
        public Task<List<BuildTableRow>> ListBuildTableByProjectAsync(int projectId);

        public Task<List<BuildTableRow>> ListAllInProgressBuildsOlderThanAsync(int hours, CancellationToken cancellationToken);

        public Task<BuildTableRow> GetBuildTableRowAsync(int id);

        public Task<BuildTableRow> GetLastUploadedByUserAsync(int userId);
        public Task<BuildTableRow> GetLastUploadedByUserProjectsAsync(int userId);
        public Task<BuildTableRow> GetFirstInQueueAsync(int userId);

        public Task<int> UpdateBuildTableAsync(int id, BuildTableRow current, BuildTableRow request);
        public Task<int> DeleteBuildTableAsync(BuildTableRow buildTable);
    }
}

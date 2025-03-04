using Marelli.Domain.Entities;
using Marelli.Infra.Context;
using Marelli.Infra.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace Marelli.Infra.Repositories
{

    public class BuildTableRowRepository : IBuildTableRowRepository
    {
        private readonly DemurrageContext _context;

        public BuildTableRowRepository(DemurrageContext context)
        {
            _context = context;
        }

        public async Task<BuildTableRow> SaveBuildTableAsync(BuildTableRow entity)
        {
            _context.BuildTableRow.Add(entity);
            await _context.SaveChangesAsync();

            return entity;
        }

        public async Task<List<BuildTableRow>> ListBuildTableAsync(int userId)
        {
            var builds = await _context.BuildTableRow
                .Where(b => !b.Deleted)
                .OrderByDescending(b => b.Date)
                .ToListAsync();

            if (userId > 0)
            {
                var projectsIds = await _context.UserProject.Where(up => up.UserId == userId).Select(up => up.ProjectId).ToListAsync();

                builds = builds.Where(b => projectsIds.Contains(b.ProjectId)).ToList();
            }

            return builds;
        }

        public async Task<List<BuildTableRow>> ListBuildTableInProgressAsync(int userId)
        {
            var builds = await _context.BuildTableRow
                .Where(b => !b.Deleted && !new[] { "Build", "In Queue", "Failed", "Download" }.Contains(b.Status))
                .OrderByDescending(b => b.Date)
                .ToListAsync();

            if (userId > 0)
            {
                var projectsIds = await _context.UserProject.Where(up => up.UserId == userId).Select(up => up.ProjectId).ToListAsync();

                builds = builds.Where(b => projectsIds.Contains(b.ProjectId)).ToList();
            }

            return builds;
        }

        public async Task<List<BuildTableRow>> ListBuildTableInQueueAsync()
        {
            var builds = await _context.BuildTableRow
                .Where(b => !b.Deleted && b.Status.Equals("In Queue"))
                .OrderBy(b => b.Date)
                .ToListAsync();

            return builds;
        }

        public async Task<List<BuildTableRow>> ListBuildTableByProjectAsync(int projectId)
        {
            return await _context.BuildTableRow
                .Where(b => !b.Deleted && b.ProjectId == projectId)
                .OrderByDescending(b => b.Date)
                .ToListAsync();
        }

        public async Task<List<BuildTableRow>> ListAllInProgressBuildsOlderThanAsync(int hours, CancellationToken cancellationToken)
        {
            var fourHoursAgo = DateTime.UtcNow.AddHours(-hours);

            var builds = await _context.BuildTableRow
                .Where(b =>
                    !b.Deleted &&
                    b.Date <= fourHoursAgo &&
                    !new[] { "Build", "In Queue", "Failed", "Download" }.Contains(b.Status))
                .ToListAsync(cancellationToken);

            return builds;
        }

        public async Task<BuildTableRow> GetBuildTableRowAsync(int id)
        {
            return await _context.BuildTableRow
                .Where(b => !b.Deleted && b.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<BuildTableRow> GetLastUploadedByUserAsync(int userId)
        {
            if (userId > 0)
            {
                var user = await _context.User.Where(u => u.Id == userId).FirstOrDefaultAsync();

                var userProjects = await _context.UserProject.Where(up => up.UserId == userId).ToListAsync();
                var userProjectsIds = userProjects.Select(up => up.ProjectId).ToList();

                var buildTableRows = await _context.BuildTableRow
                    .Where(b => !b.Deleted && b.UserId == userId)
                    .OrderByDescending(b => b.Date)
                    .ToListAsync();

                if (!user.Role.Equals("Administrator"))
                {
                    buildTableRows = buildTableRows
                    .Where(b => userProjectsIds.Contains(b.ProjectId))
                    .ToList();
                }

                return buildTableRows.FirstOrDefault();
            }
            else
            {
                return await _context.BuildTableRow
                    .Where(b => !b.Deleted)
                    .OrderByDescending(b => b.Date)
                    .FirstOrDefaultAsync();
            }

        }

        public async Task<BuildTableRow> GetLastUploadedByUserProjectsAsync(int userId)
        {
            if (userId > 0)
            {
                var userProjects = await _context.UserProject.Where(up => up.UserId == userId).ToListAsync();
                var userProjectsIds = userProjects.Select(up => up.ProjectId).ToList();

                return await _context.BuildTableRow
                    .Where(b => !b.Deleted && userProjectsIds.Contains(b.ProjectId))
                    .OrderByDescending(b => b.Date)
                    .FirstOrDefaultAsync();
            }
            else
            {
                return await _context.BuildTableRow
                    .Where(b => !b.Deleted)
                    .OrderByDescending(b => b.Date)
                    .FirstOrDefaultAsync();
            }
        }

        public async Task<BuildTableRow> GetFirstInQueueAsync(int userId)
        {
            var inQueueBuilds = await _context.BuildTableRow
                .Where(b => !b.Deleted && b.Status.Equals("In Queue"))
                .OrderBy(b => b.Date)
                .ToListAsync();

            if (userId > 0)
            {
                inQueueBuilds = inQueueBuilds.Where(b => b.UserId == userId).ToList();
            }

            return inQueueBuilds.FirstOrDefault();
        }

        public async Task<int> UpdateBuildTableAsync(int id, BuildTableRow current, BuildTableRow updated)
        {

            current.Status = updated.Status;
            current.Date = updated.Date;
            current.Developer = updated.Developer;
            current.Deleted = updated.Deleted;
            current.FileName = updated.FileName;
            current.TagName = updated.TagName;
            current.TagDescription = updated.TagDescription;
            current.Md5Hash = updated.Md5Hash;

            _context.BuildTableRow.Entry(current).State = EntityState.Modified;
            return await _context.SaveChangesAsync();
        }

        public async Task<int> DeleteBuildTableAsync(BuildTableRow buildTable)
        {
            _context.Remove(buildTable);

            return await _context.SaveChangesAsync();

        }
    }
}
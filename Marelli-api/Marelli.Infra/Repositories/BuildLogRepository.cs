using Marelli.Domain.Entities;
using Marelli.Infra.Context;
using Marelli.Infra.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace Marelli.Infra.Repositories
{

    public class BuildLogRepository : IBuildLogRepository
    {
        private readonly DemurrageContext _context;

        public BuildLogRepository(DemurrageContext context)
        {
            _context = context;
        }

        public async Task<BuildLog> SaveBuildLog(BuildLog entity)
        {
            _context.BuildLog.Add(entity);
            await _context.SaveChangesAsync();

            return entity;
        }

        public async Task<List<BuildLog>> ListBuildLog(int buildId)
        {
            return await _context.BuildLog
                .Where(b => b.BuildId == buildId)
                .OrderBy(b => b.LogId)
                .ToListAsync();
        }

        public async Task<BuildLog> GetBuildLog(int id)
        {
            return await _context.BuildLog
                .Where(b => b.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<int> UpdateBuildLog(int id, BuildLog current, BuildLog request)
        {

            current.Status = request.Status;
            current.Date = request.Date;
            current.Details = request.Details;
            current.Type = request.Type;
            current.BuildId = request.BuildId;
            current.ProjectId = request.ProjectId;
            current.UserId = request.UserId;

            _context.BuildLog.Entry(current).State = EntityState.Modified;
            return await _context.SaveChangesAsync();
        }

        public async Task<int> DeleteBuildLog(BuildLog buildLog)
        {

            _context.BuildLog.Remove(buildLog);

            return await _context.SaveChangesAsync();

        }
        public async Task<int> DeleteBuildLogsByBuildId(int buildId)
        {
            var buildLogs = await _context.BuildLog.Where(b => b.BuildId == buildId).ToListAsync();

            _context.BuildLog.RemoveRange(buildLogs);

            return await _context.SaveChangesAsync();

        }


    }
}
using Marelli.Domain.Entities;
using Marelli.Infra.Context;
using Marelli.Infra.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace Marelli.Infra.Repositories
{

    public class BaselineRepository : IBaselineRepository
    {
        private readonly DemurrageContext _context;

        public BaselineRepository(DemurrageContext context)
        {
            _context = context;
        }

        public async Task<Baseline> SaveBaseline(Baseline entity)
        {
            if (entity.Selected)
            {
                var baselines = await ListBaseline(entity.ProjectId);

                baselines.ForEach(b =>
                {
                    b.Selected = false;
                    _context.Baseline.Entry(b).State = EntityState.Modified;
                });

                await _context.SaveChangesAsync();
            }

            _context.Baseline.Add(entity);

            await _context.SaveChangesAsync();

            return entity;
        }

        public async Task<List<Baseline>> ListBaseline(int projectId)
        {

            return await _context.Baseline
                .Where(b => b.ProjectId == projectId)
                .OrderByDescending(b => b.UploadDate)
                .ToListAsync();
        }

        public async Task<Baseline> GetBaseline(int id)
        {
            return await _context.Baseline
                .Where(b => b.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<Baseline> GetBaselineByProject(int projectId)
        {
            return await _context.Baseline
                .Where(b => b.ProjectId == projectId && b.Selected)
                .FirstOrDefaultAsync();
        }

        public async Task<int> UpdateBaseline(int id, Baseline current, Baseline request)
        {
            if (request.Selected)
            {
                var baselines = await ListBaseline(request.ProjectId);

                baselines.ForEach(b =>
                {
                    b.Selected = false;
                    _context.Baseline.Entry(b).State = EntityState.Modified;
                });

                await _context.SaveChangesAsync();
            }

            current.Description = request.Description;
            current.UploadDate = request.UploadDate;
            current.FileName = request.FileName;
            current.Selected = request.Selected;
            current.Project = request.Project;
            current.ProjectId = request.ProjectId;

            _context.Baseline.Entry(current).State = EntityState.Modified;
            return await _context.SaveChangesAsync();
        }

        public async Task<int> DeleteBaseline(Baseline baseline)
        {
            _context.Baseline.Remove(baseline);

            var result = await _context.SaveChangesAsync();

            if (baseline.Selected)
            {
                var baselines = await ListBaseline(baseline.ProjectId);
                var lastUploadedBaseline = baselines.FirstOrDefault();

                if (lastUploadedBaseline != null)
                {
                    lastUploadedBaseline.Selected = true;

                    _context.Baseline.Entry(lastUploadedBaseline).State = EntityState.Modified;

                    await _context.SaveChangesAsync();
                }
            }

            return result;

        }


    }
}
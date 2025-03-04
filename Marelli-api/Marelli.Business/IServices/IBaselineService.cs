using Marelli.Domain.Entities;

namespace Marelli.Business.IServices
{
    public interface IBaselineService
    {
        public Task<Baseline> SaveBaseline(Baseline request);

        public Task<List<Baseline>> ListBaseline(int projectId);

        public Task<Baseline> GetBaseline(int id);
        public Task<Baseline> GetBaselineByProject(int projectId);

        public Task<int> UpdateBaseline(int id, Baseline request);

        public Task<int> DeleteBaseline(int id);

    }
}

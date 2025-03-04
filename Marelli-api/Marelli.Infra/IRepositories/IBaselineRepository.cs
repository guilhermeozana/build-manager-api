using Marelli.Domain.Entities;

namespace Marelli.Infra.IRepositories
{
    public interface IBaselineRepository
    {
        public Task<Baseline> SaveBaseline(Baseline entity);

        public Task<List<Baseline>> ListBaseline(int projectId);

        public Task<Baseline> GetBaseline(int id);
        public Task<Baseline> GetBaselineByProject(int projectId);

        public Task<int> UpdateBaseline(int id, Baseline current, Baseline request);

        public Task<int> DeleteBaseline(Baseline buildLog);
    }
}

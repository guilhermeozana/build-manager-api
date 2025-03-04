using Marelli.Business.Exceptions;
using Marelli.Business.IServices;
using Marelli.Domain.Entities;
using Marelli.Infra.IRepositories;

namespace Marelli.Business.Services
{

    public class BaselineService : IBaselineService
    {
        private readonly IBaselineRepository _baselineRepository;
        private readonly IProjectService _projectService;
        //private readonly ValidadoresHelper _validadoresHelper;


        public BaselineService(IBaselineRepository baselineRepository, IProjectService projectService)
        {
            _baselineRepository = baselineRepository;
            _projectService = projectService;
            //_validadoresHelper = new ValidadoresHelper();
        }

        public async Task<Baseline> SaveBaseline(Baseline request)
        {
            await _projectService.GetProjectById(request.ProjectId);

            var baseline = await _baselineRepository.SaveBaseline(request);

            return baseline;
        }

        public async Task<List<Baseline>> ListBaseline(int projectId)
        {
            return await _baselineRepository.ListBaseline(projectId);
        }

        public async Task<Baseline> GetBaseline(int id)
        {
            var baseline = await _baselineRepository.GetBaseline(id);

            if (baseline == null)
            {
                throw new NotFoundException($"Baseline not found with ID {id}.");
            }

            return baseline;
        }

        public async Task<Baseline> GetBaselineByProject(int projectId)
        {
            return await _baselineRepository.GetBaselineByProject(projectId);
        }

        public async Task<int> UpdateBaseline(int id, Baseline request)
        {
            await _projectService.GetProjectById(request.ProjectId);

            var baseline = await GetBaseline(id);

            var updated = await _baselineRepository.UpdateBaseline(id, baseline, request);

            return updated;
        }

        public async Task<int> DeleteBaseline(int id)
        {
            var baseline = await GetBaseline(id);

            return await _baselineRepository.DeleteBaseline(baseline);
        }

    }
}
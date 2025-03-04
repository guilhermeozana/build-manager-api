using Marelli.Domain.Dtos;
using Marelli.Domain.Entities;

namespace Marelli.Business.IServices
{
    public interface IProjectService
    {
        public Task<int> SaveProject(Project request);

        public Task<List<ProjectResponse>> ListProjects(int userId);

        public Task<List<Project>> ListProjectsByGroupId(int grupoId);

        //public Task<List<Project>> ListProjectsByUserId(int userId);

        public Task<List<ProjectResponse>> ListProjectsByName(string nome);

        public Task<Project> GetProjectById(int projectId);

        public Task<int> UpdateProject(int id, Project entity);

        public Task<int> DeleteProjectById(int id);
    }
}

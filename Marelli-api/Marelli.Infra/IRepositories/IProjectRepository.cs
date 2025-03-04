using Marelli.Domain.Entities;

namespace Marelli.Infra.IRepositories
{
    public interface IProjectRepository
    {
        public Task<int> SaveProject(Project entity);

        public Task<List<Project>> ListProjects(int userId);

        public Task<List<Project>> ListProjectsByGroupId(int groupId);

        //public Task<List<Project>> ListProjectsByUserId(int userId);

        public Task<List<Project>> ListProjectsByName(string name);

        public Task<Project> GetProjectById(int projectId);

        public Task<int> UpdateProject(int id, Project currentProject, Project updatedProject);

        public Task<int> DeleteProject(Project entity);

        public Task<int> DeleteProjectById(int id, Project project);
    }
}

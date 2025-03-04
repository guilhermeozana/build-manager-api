using Marelli.Business.Exceptions;
using Marelli.Business.IServices;
using Marelli.Domain.Dtos;
using Marelli.Domain.Entities;
using Marelli.Infra.IRepositories;

namespace Marelli.Business.Services;

public class ProjectService : IProjectService
{
    private readonly IProjectRepository _projectRepository;
    private readonly IGroupRepository _groupRepository;
    private readonly IUserRepository _userRepository;

    public ProjectService(IProjectRepository projectRepository, IGroupRepository groupRepository, IUserRepository userRepository)
    {
        _projectRepository = projectRepository;
        _groupRepository = groupRepository;
        _userRepository = userRepository;
    }


    public async Task<int> SaveProject(Project request)
    {
        var group = await _groupRepository.GetGroupById(request.GroupId);

        if (group == null)
        {
            throw new NotFoundException($"Group not found with ID: {request.GroupId}.");
        }

        var projectsByGroup = await _projectRepository.ListProjectsByGroupId(request.GroupId);
        var project = projectsByGroup.Where(p => p.Name == request.Name).FirstOrDefault();

        if (project != null)
        {
            throw new AlreadyExistsException($"The project {project.Name} already exists in {group.Name} Group!");
        }

        return await _projectRepository.SaveProject(request);
    }

    public async Task<List<ProjectResponse>> ListProjects(int userId)
    {
        var projects = await _projectRepository.ListProjects(userId);

        var projectDtos = new List<ProjectResponse>();

        foreach (var project in projects)
        {
            var projectDto = await MapToResponse(project);

            projectDtos.Add(projectDto);
        }

        return projectDtos;
    }

    public async Task<List<Project>> ListProjectsByGroupId(int grupoId)
    {
        return await _projectRepository.ListProjectsByGroupId(grupoId);
    }

    public async Task<List<ProjectResponse>> ListProjectsByName(string name)
    {
        var projects = await _projectRepository.ListProjectsByName(name);

        var projectDtos = new List<ProjectResponse>();

        foreach (var project in projects)
        {
            var projectDto = await MapToResponse(project);

            projectDtos.Add(projectDto);
        }

        return projectDtos;
    }

    public async Task<Project> GetProjectById(int projectId)
    {
        var project = await _projectRepository.GetProjectById(projectId);

        if (project == null)
        {
            throw new NotFoundException($"Project not found with ID {projectId}.");
        }

        return project;
    }

    public async Task<int> UpdateProject(int id, Project req)
    {
        var savedProject = await GetProjectById(id);

        var groupById = await _groupRepository.GetGroupById(req.GroupId);

        if (groupById == null)
        {
            throw new NotFoundException($"Group does not exist with ID {req.GroupId}.");
        }

        var projectsByGroupId = await ListProjectsByGroupId(req.GroupId);
        var project = projectsByGroupId.Where(p => p.Name == req.Name && p.Id != id).FirstOrDefault();

        if (project != null)
        {
            throw new AlreadyExistsException($"Project with name {project.Name} already exists!");
        }

        foreach (var userProject in req.UsersProject)
        {
            userProject.ProjectId = id;
        }

        return await _projectRepository.UpdateProject(id, savedProject, req);
    }

    public async Task<int> DeleteProjectById(int id)
    {
        var savedProject = await GetProjectById(id);

        var result = await _projectRepository.DeleteProjectById(id, savedProject);

        foreach (var userProject in savedProject.UsersProject)
        {
            userProject.ProjectId = id;
        }

        return result;
    }

    private async Task<ProjectResponse> MapToResponse(Project project)
    {
        var users = await _userRepository.ListUsersByProjectId(project.Id);

        return new ProjectResponse
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            Image = project.Image,
            Group = new GroupResponse
            {
                Id = project.GroupId,
                Name = project.Group.Name
            },
            Users = users
        };
    }
}


using Marelli.Domain.Entities;
using Marelli.Infra.Context;
using Marelli.Infra.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace Marelli.Infra.Repositories;

public class ProjectRepository : IProjectRepository
{
    private readonly DemurrageContext _context;
    private readonly IUserRepository _UserRepository;
    private readonly IGroupRepository _GroupRepository;

    public ProjectRepository(DemurrageContext context, IUserRepository userRepository, IGroupRepository groupRepository)
    {
        _context = context;
        _UserRepository = userRepository;
        _GroupRepository = groupRepository;
    }


    public async Task<int> SaveProject(Project entity)
    {
        _context.ChangeTracker.Clear();

        _context.Project.Add(entity);

        return await _context.SaveChangesAsync();
    }

    public async Task<List<Project>> ListProjects(int userId)
    {
        var projects = await _context.Project
        .Include(p => p.Group)
        .Include(p => p.UsersProject)
        .ToListAsync();

        if (userId > 0)
        {
            projects = projects
                .Where(p => p.UsersProject.Any(up => up.UserId == userId))
                .ToList();
        }

        return projects;
    }

    public async Task<List<Project>> ListProjectsByGroupId(int groupId)
    {
        return await _context.Project.Where(p => p.GroupId == groupId).ToListAsync();
    }

    public async Task<List<Project>> ListProjectsByUserId(int userId)
    {
        return await _context.Project
            .Where(p => p.UsersProject.Any(up => up.UserId == userId))
            .ToListAsync();
    }

    public async Task<List<Project>> ListProjectsByName(string name)
    {
        return await _context.Project
            .Where(p => p.Name == name)
            .Include(p => p.Group)
            .Include(p => p.UsersProject)
            .ToListAsync();
    }

    public async Task<Project> GetProjectById(int projectId)
    {
        return await _context.Project
            .Where(p => p.Id == projectId)
            .Include(p => p.UsersProject)
            .FirstOrDefaultAsync();
    }

    public async Task<int> UpdateProject(int id, Project currentProject, Project updatedProject)
    {
        _context.ChangeTracker.Clear();

        currentProject.Name = updatedProject.Name;
        currentProject.Description = updatedProject.Description;
        currentProject.Image = updatedProject.Image;
        currentProject.GroupId = updatedProject.GroupId;

        _context.Project.Update(currentProject);

        _context.UserProject.RemoveRange(currentProject.UsersProject);

        _context.UserProject.AddRange(updatedProject.UsersProject);

        return await _context.SaveChangesAsync();
    }

    public async Task<int> DeleteProject(Project entity)
    {
        _context.Project.Remove(entity);
        return await _context.SaveChangesAsync();
    }

    public async Task<int> DeleteProjectById(int id, Project project)
    {
        _context.Project.Remove(project);
        return await _context.SaveChangesAsync();
    }

}

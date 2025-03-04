using Marelli.Domain.Dtos;
using Marelli.Domain.Entities;
using Marelli.Infra.Context;
using Marelli.Infra.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace Marelli.Infra.Repositories;

public class GroupRepository : IGroupRepository
{
    private readonly DemurrageContext _context;
    private readonly IUserRepository _UserRepository;

    public GroupRepository(DemurrageContext context, IUserRepository userRepository)
    {
        _context = context;
        _UserRepository = userRepository;
    }

    public async Task<int> SaveGroup(GroupRequest req)
    {
        _context.ChangeTracker.Clear();

        Group group = new Group();

        group.Name = req.Name;
        group.Image = req.Image;
        group.CompanyImage = req.CompanyImage;

        _context.Group.Add(group);

        foreach (var userId in req.UserIds)
        {
            var userGroup = new UserGroup()
            {
                UserId = userId,
                GroupId = group.Id,
            };

            group.UsersGroup.Add(userGroup);
        }

        return await _context.SaveChangesAsync();
    }

    public async Task<List<Group>> ListGroups()
    {
        return await _context.Group
            .Include(g => g.UsersGroup)
            .Include(g => g.Projects)
            .ThenInclude(p => p.UsersProject)
            .ToListAsync();

    }

    public async Task<List<Group>> ListGroupsByUser(int userId)
    {
        var groups = await _context.Group
            .Include(g => g.UsersGroup)
            .Include(g => g.Projects)
            .ToListAsync();

        groups = groups
            .Where(g => g.UsersGroup.Any(ug => ug.UserId == userId))
            .ToList();

        return groups;

    }

    public async Task<Group> GetGroupById(int id)
    {
        return await _context.Group
            .Include(g => g.UsersGroup)
            .Where(g => g.Id == id)
            .FirstOrDefaultAsync();
    }

    public async Task<Group> GetGroupByName(string name)
    {
        return await _context.Group
            .Where(g => g.Name == name)
            .FirstOrDefaultAsync();
    }

    public async Task<Group> GetGroupWithSameName(int id, string name)
    {
        return await _context.Group.Where(g => g.Name == name && g.Id != id).FirstOrDefaultAsync();
    }


    public async Task<int> UpdateGroup(int id, Group savedGroup, GroupRequest updatedGroupReq)
    {
        _context.ChangeTracker.Clear();

        savedGroup.Name = updatedGroupReq.Name;
        savedGroup.Image = updatedGroupReq.Image;
        savedGroup.CompanyImage = updatedGroupReq.CompanyImage;

        _context.Group.Update(savedGroup);

        await _context.SaveChangesAsync();

        _context.ChangeTracker.Clear();

        var users = await _UserRepository.ListUsersByGroupId(savedGroup.Id);

        var userIdsToRemove = users.Where(u => !updatedGroupReq.UserIds.Contains(u.Id)).ToList();

        foreach (var user in userIdsToRemove)
        {
            var userGroupToRemove = await _context.UserGroup.Where(ug => ug.UserId == user.Id && ug.GroupId == id).FirstOrDefaultAsync();

            if (userGroupToRemove != null)
            {
                _context.UserGroup.Remove(userGroupToRemove);
            }

            var userProjectsToRemove = await _context.UserProject.Where(up => up.UserId == user.Id).ToListAsync();
            _context.UserProject.RemoveRange(userProjectsToRemove);

            await _context.SaveChangesAsync();

        }

        foreach (var userId in updatedGroupReq.UserIds)
        {
            if (!users.Any(u => u.Id == userId))
            {
                _context.ChangeTracker.Clear();

                var user = await _UserRepository.GetUserById(userId);

                if (user != null)
                {
                    await _context.UserGroup.AddAsync(new UserGroup()
                    {
                        UserId = user.Id,
                        GroupId = id
                    });

                    _context.User.Update(user);

                    await _context.SaveChangesAsync();
                }
            }
        }

        return 1;
    }


    public async Task<int> DeleteGroup(Group group)
    {
        _context.ChangeTracker.Clear();

        var projects = await _context.Project
            .Where(p => p.GroupId == group.Id)
            .ToListAsync();

        _context.Project.RemoveRange(projects);

        _context.Group.Remove(group);

        return await _context.SaveChangesAsync();
    }

}

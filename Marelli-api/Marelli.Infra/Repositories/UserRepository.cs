using Marelli.Domain.Dtos;
using Marelli.Domain.Entities;
using Marelli.Infra.Context;
using Marelli.Infra.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace Marelli.Infra.Repositories;

public class UserRepository : IUserRepository
{
    private readonly DemurrageContext _context;


    public UserRepository(DemurrageContext context)
    {
        _context = context;
    }

    public async Task<int> SaveUser(User entity)
    {
        _context.ChangeTracker.Clear();

        _context.User.Add(entity);

        return await _context.SaveChangesAsync();
    }

    public async Task<List<UserResponse>> ListUsers()
    {
        return await _context.User
            .Select(u => new UserResponse
            {
                Id = u.Id,
                Email = u.Email,
                Role = u.Role,
                NewUser = u.NewUser,
                Name = u.Name,
            })
            .ToListAsync();
    }

    public async Task<List<User>> ListUsersByProjectId(int projectId)
    {
        var userProjects = await _context.UserProject
            .Where(up => up.ProjectId == projectId)
            .ToListAsync();

        var usersId = userProjects.Select(up => up.UserId).ToList();

        return await _context.User
            .Where(u => usersId.Contains(u.Id))
            .Select(u => new User
            {
                Id = u.Id,
                Email = u.Email,
                Role = u.Role,
                NewUser = u.NewUser,
                Name = u.Name
            })
            .ToListAsync();
    }

    public async Task<List<User>> ListUsersByGroupId(int groupId)
    {
        var usersGroup = _context.UserGroup.Where(ug => ug.GroupId == groupId);

        var users = await _context.User
            .Where(u => usersGroup.Any(ug => ug.UserId == u.Id))
            .Select(u => new User
            {
                Id = u.Id,
                Email = u.Email,
                Password = u.Password,
                Role = u.Role,
                NewUser = u.NewUser,
                Name = u.Name
            })
            .ToListAsync();

        return users;
    }

    public async Task<User> GetUserByEmailAndPassword(string email, string password)
    {
        return await _context.User.FirstOrDefaultAsync(u => u.Email == email && u.Password == password);
    }

    public async Task<User> GetUserByEmail(string email)
    {
        return await _context.User
            .Where(u => u.Email == email)
            .Select(u => new User
            {
                Id = u.Id,
                Email = u.Email,
                Role = u.Role,
                NewUser = u.NewUser,
                Name = u.Name
            })
            .FirstOrDefaultAsync();
    }

    public async Task<User> GetUserById(int id)
    {
        return await _context.User
                            .Where(u => u.Id == id)
                            .Select(u => new User
                            {
                                Id = u.Id,
                                Email = u.Email,
                                Password = u.Password,
                                Role = u.Role,
                                NewUser = u.NewUser,
                                Name = u.Name
                            })
                            .FirstOrDefaultAsync();
    }

    public async Task<User> GetUserWithPassword(string email)
    {
        return await _context.User
                            .Where(u => u.Email == email)
                            .Select(u => new User
                            {
                                Id = u.Id,
                                Email = u.Email,
                                Password = u.Password,
                                Role = u.Role,
                                NewUser = u.NewUser,
                                Name = u.Name
                            })
                            .FirstOrDefaultAsync();
    }

    public async Task<User> UpdateUser(int id, User currentUser, User updatedUser)
    {

        _context.ChangeTracker.Clear();

        currentUser.Email = updatedUser.Email;

        if (updatedUser.Password != null)
        {
            currentUser.Password = updatedUser.Password;
        }

        if (updatedUser.NewUser != null)
        {
            currentUser.NewUser = updatedUser.NewUser;
        }

        if (updatedUser.Role.Equals("Administrator"))
        {
            var userGroups = await _context.UserGroup.Where(up => up.UserId == id).ToListAsync();
            _context.UserGroup.RemoveRange(userGroups);

            var userProjects = await _context.UserProject.Where(up => up.UserId == id).ToListAsync();
            _context.UserProject.RemoveRange(userProjects);
        }

        currentUser.Name = updatedUser.Name;
        currentUser.Role = updatedUser.Role;

        _context.User.Entry(currentUser).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        return currentUser;
    }

    public async Task<User> UpdateUserPassword(string email, string password)
    {
        _context.ChangeTracker.Clear();

        var user = await GetUserWithPassword(email);

        user.Password = password;
        user.NewUser = false;

        _context.User.Update(user);

        await _context.SaveChangesAsync();

        return user;
    }

    public async Task<int> DeleteUser(User entity)
    {
        _context.User.Remove(entity);

        return await _context.SaveChangesAsync();
    }

    public async Task<int> RemoveUserFromGroup(int userId, int groupId)
    {
        var userGroup = await _context.UserGroup
            .FirstOrDefaultAsync(ug => ug.UserId == userId && ug.GroupId == groupId);

        if (userGroup != null)
        {
            _context.UserGroup.Remove(userGroup);
        }

        var projectIdsByGroup = await _context.Project
            .Where(p => p.GroupId == groupId)
            .Select(p => p.Id)
            .ToListAsync();

        if (projectIdsByGroup.Any())
        {
            var userProjectsByGroup = await _context.UserProject
                .Where(up => projectIdsByGroup.Contains(up.ProjectId) && up.UserId == userId)
                .ToListAsync();

            _context.UserProject.RemoveRange(userProjectsByGroup);
        }

        return await _context.SaveChangesAsync();
    }


}

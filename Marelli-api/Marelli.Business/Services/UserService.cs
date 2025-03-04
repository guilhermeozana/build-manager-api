using Marelli.Business.Exceptions;
using Marelli.Business.IServices;
using Marelli.Business.Utils;
using Marelli.Domain.Dtos;
using Marelli.Domain.Entities;
using Marelli.Infra.IRepositories;
using Microsoft.AspNetCore.Http;

namespace Marelli.Business.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IGroupRepository _groupRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IUserTokenRepository _userTokenRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;


    public UserService(IUserRepository UserRepository, IGroupRepository groupRepository, IProjectRepository projectRepository, IUserTokenRepository userTokenRepository, IHttpContextAccessor httpContextAccessor)
    {
        _userRepository = UserRepository;
        _groupRepository = groupRepository;
        _projectRepository = projectRepository;
        _userTokenRepository = userTokenRepository;
        _httpContextAccessor = httpContextAccessor;
    }


    public async Task<int> SaveUser(User req)
    {
        req.NewUser = true;

        var user = await _userRepository.GetUserByEmail(req.Email);

        if (user != null)
        {
            throw new AlreadyExistsException($"User already exists with email {req.Email}");
        }

        PasswordUtils.VerifyPasswordIsValid(req.Password);

        var encryptedPassword = EncryptionUtils.HashPassword(req.Password);

        req.Password = encryptedPassword;

        return await _userRepository.SaveUser(req);
    }

    public async Task<List<UserResponse>> ListUsers()
    {
        var users = await _userRepository.ListUsers();

        foreach (var user in users)
        {
            var projects = await _projectRepository.ListProjects(user.Id);
            projects.ForEach(p => p.Image = null);

            user.Projects = projects;

            var groups = await _groupRepository.ListGroupsByUser(user.Id);
            groups.ForEach(p => p.Image = null);

            user.Groups = groups;
        }

        return users;
    }

    public async Task<List<User>> ListUsersByProjectId(int id)
    {
        return await _userRepository.ListUsersByProjectId(id);
    }

    public async Task<List<User>> ListUsersByGroupId(int id)
    {
        return await _userRepository.ListUsersByGroupId(id);
    }


    public async Task<User> GetUserByEmailAndPassword(string email, string senha)
    {
        var user = await _userRepository.GetUserByEmailAndPassword(email, senha);

        if (user == null)
        {
            throw new NotFoundException($"User not found with email {email} or password incorrect.");
        }

        return user;
    }

    public async Task<UserResponse> GetUserResponseByEmail(string email)
    {
        var user = await _userRepository.GetUserByEmail(email);

        if (user == null)
        {
            return null;
        }

        var groups = await _groupRepository.ListGroupsByUser(user.Id);

        var projects = await _projectRepository.ListProjects(user.Id);

        return new UserResponse
        {
            Id = user.Id,
            Email = user.Email,
            Role = user.Role,
            NewUser = user.NewUser,
            Name = user.Name,
            Groups = groups,
            Projects = projects
        };
    }

    public async Task<User> GetUserByEmail(string email)
    {

        var user = await _userRepository.GetUserByEmail(email);

        if (user != null)
        {
            user.Password = null;
        }

        return user;
    }

    public async Task<User> GetUserById(int id)
    {
        var user = await _userRepository.GetUserById(id);

        if (user == null)
        {
            throw new NotFoundException($"User not found with ID {id}.");
        }

        return user;
    }

    public async Task<User> GetUserWithPassword(string email)
    {
        return await _userRepository.GetUserWithPassword(email);
    }

    public async Task<User> UpdateUserPassword(string email, string password)
    {
        var user = await GetUserByEmail(email);

        PasswordUtils.VerifyPasswordIsValid(password);

        var encryptedPassword = EncryptionUtils.HashPassword(password);

        password = encryptedPassword;

        var updated = await _userRepository.UpdateUserPassword(email, password);

        await _userTokenRepository.RevokeUserTokens(user.Id);

        return updated;
    }

    public async Task<int> UpdateUser(int id, User req)
    {
        var user = await GetUserById(id);

        if (user == null)
        {
            throw new NotFoundException($"User not found with ID {id}.");
        }

        if (req.Password != null)
        {

            PasswordUtils.VerifyPasswordIsValid(req.Password);

            var encryptedPassword = EncryptionUtils.HashPassword(req.Password);

            req.Password = encryptedPassword;
        }

        if (UserUtils.VerifyIsMarelliUser(user.Role) && !UserUtils.VerifyIsMarelliUser(req.Role))
        {
            var groupsByUser = await _groupRepository.ListGroupsByUser(id);

            foreach (var group in groupsByUser)
            {
                await _userRepository.RemoveUserFromGroup(user.Id, group.Id);
            }
        }

        var updated = await _userRepository.UpdateUser(id, user, req);

        if (req.Password != null)
        {
            var token = _httpContextAccessor.HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (token != null)
            {
                await _userTokenRepository.RevokeUserTokensExceptCurrent(id, token);
            }
        }

        updated.Password = null;

        return 1;
    }

    public async Task<int> DeleteUser(int id)
    {
        var user = await GetUserById(id);

        var deleted = await _userRepository.DeleteUser(user);

        await _userTokenRepository.RevokeUserTokens(id);

        return deleted;
    }
}


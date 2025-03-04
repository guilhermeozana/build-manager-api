using Marelli.Business.Exceptions;
using Marelli.Business.IServices;
using Marelli.Domain.Dtos;
using Marelli.Domain.Entities;
using Marelli.Infra.IRepositories;

namespace Marelli.Business.Services;

public class GroupService : IGroupService
{
    private readonly IGroupRepository _groupRepository;
    private readonly IUserRepository _userRepository;

    public GroupService(IGroupRepository groupRepository, IUserRepository userRepository)
    {
        _groupRepository = groupRepository;
        _userRepository = userRepository;
    }

    public async Task<int> SaveGroup(GroupRequest request)
    {

        var groupByName = await _groupRepository.GetGroupByName(request.Name);

        if (groupByName != null)
        {
            throw new AlreadyExistsException($"Group with name {groupByName.Name} already exists!");
        }

        var savedGroup = await _groupRepository.SaveGroup(request);

        return 1;
    }

    public async Task<List<GroupResponse>> ListGroups()
    {
        var groups = await _groupRepository.ListGroups();

        var groupDtos = new List<GroupResponse>();

        foreach (var group in groups)
        {
            var users = await _userRepository.ListUsersByGroupId(group.Id);
            users.ForEach(u => u.Password = null);

            var groupDto = new GroupResponse
            {
                Id = group.Id,
                Name = group.Name,
                Image = group.Image,
                CompanyImage = group.CompanyImage,
                Users = users,
                Projects = group.Projects
            };

            groupDtos.Add(groupDto);
        }

        return groupDtos;
    }

    public async Task<Group> GetGroupById(int id)
    {
        var group = await _groupRepository.GetGroupById(id);

        if (group == null)
        {
            throw new NotFoundException($"Group not found with ID: {id}.");
        }

        return group;
    }

    public async Task<int> UpdateGroup(int id, GroupRequest req)
    {
        var savedGroup = await GetGroupById(id);

        var groupWithSameName = await _groupRepository.GetGroupWithSameName(id, req.Name);

        if (groupWithSameName != null)
        {
            throw new AlreadyExistsException($"Group with name {groupWithSameName.Name} already exists!");
        }

        var result = await _groupRepository.UpdateGroup(id, savedGroup, req);

        return result;
    }

    public async Task<int> DeleteGroup(int id)
    {
        var group = await GetGroupById(id);

        var result = await _groupRepository.DeleteGroup(group);

        return result;
    }
}


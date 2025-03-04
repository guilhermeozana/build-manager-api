using Marelli.Domain.Dtos;
using Marelli.Domain.Entities;

namespace Marelli.Infra.IRepositories
{
    public interface IGroupRepository
    {
        public Task<int> SaveGroup(GroupRequest req);

        public Task<List<Group>> ListGroups();

        public Task<List<Group>> ListGroupsByUser(int userId);

        public Task<Group> GetGroupById(int id);
        public Task<Group> GetGroupByName(string name);
        public Task<Group> GetGroupWithSameName(int id, string name);

        public Task<int> UpdateGroup(int id, Group savedGroup, GroupRequest req);

        public Task<int> DeleteGroup(Group group);
    }
}

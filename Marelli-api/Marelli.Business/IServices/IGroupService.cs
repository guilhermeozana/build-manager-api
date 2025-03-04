using Marelli.Domain.Dtos;
using Marelli.Domain.Entities;

namespace Marelli.Business.IServices
{
    public interface IGroupService
    {
        public Task<int> SaveGroup(GroupRequest request);

        public Task<List<GroupResponse>> ListGroups();

        public Task<Group> GetGroupById(int id);

        public Task<int> UpdateGroup(int id, GroupRequest req);

        public Task<int> DeleteGroup(int id);
    }
}

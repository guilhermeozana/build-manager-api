using Marelli.Domain.Dtos;
using Marelli.Domain.Entities;

namespace Marelli.Infra.IRepositories
{
    public interface IUserRepository
    {
        public Task<int> SaveUser(User entity);

        public Task<List<UserResponse>> ListUsers();

        public Task<List<User>> ListUsersByProjectId(int projectId);

        public Task<List<User>> ListUsersByGroupId(int groupId);

        public Task<User> GetUserByEmailAndPassword(string email, string password);

        public Task<User> GetUserByEmail(string email);

        public Task<User> GetUserById(int id);
        public Task<User> GetUserWithPassword(string email);

        public Task<User> UpdateUser(int id, User currentUser, User updatedUser);

        public Task<User> UpdateUserPassword(string email, string password);

        public Task<int> DeleteUser(User entity);

        public Task<int> RemoveUserFromGroup(int userId, int groupId);

    }
}

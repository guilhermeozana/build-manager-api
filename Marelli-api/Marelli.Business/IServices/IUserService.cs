using Marelli.Domain.Dtos;
using Marelli.Domain.Entities;

namespace Marelli.Business.IServices
{
    public interface IUserService
    {
        public Task<int> SaveUser(User request);

        public Task<List<UserResponse>> ListUsers();

        public Task<List<User>> ListUsersByProjectId(int id);

        public Task<List<User>> ListUsersByGroupId(int id);

        public Task<User> GetUserByEmailAndPassword(string email, string senha);

        public Task<UserResponse> GetUserResponseByEmail(string email);

        public Task<User> GetUserByEmail(string email);

        public Task<User> GetUserById(int id);
        public Task<User> GetUserWithPassword(string email);

        public Task<User> UpdateUserPassword(string email, string password);

        public Task<int> UpdateUser(int id, User entity);
        public Task<int> DeleteUser(int id);
    }
}

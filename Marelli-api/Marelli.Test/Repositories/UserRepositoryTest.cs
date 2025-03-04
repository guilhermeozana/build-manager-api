using Marelli.Domain.Entities;
using Marelli.Infra.Repositories;
using Marelli.Test.Utils.Factories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Marelli.Test.Repositories
{
    public class UserRepositoryTest
    {

        [Fact]
        public async Task SaveUser_ShouldReturnGreaterThanZero()
        {
            var demurrageContext = DbContextFactory.GetDemurrageContextTest();
            var userRepository = new UserRepository(demurrageContext);

            var user = UserFactory.GetUser();

            var result = await userRepository.SaveUser(user);

            Assert.Equal(1, result);
        }

        [Fact]
        public async Task ListUsers_ShouldReturnUserList()
        {
            var demurrageContext = DbContextFactory.GetDemurrageContextTest();
            var userRepository = new UserRepository(demurrageContext);
            var user = UserFactory.GetUser();

            demurrageContext.Add(user);
            await demurrageContext.SaveChangesAsync();

            var result = await userRepository.ListUsers();

            Assert.NotEmpty(result);
            Assert.Equal(user.Name, result.First().Name);
        }

        [Fact]
        public async Task ListUsersByProjectId_ShouldReturnUserList()
        {
            var demurrageContext = DbContextFactory.GetDemurrageContextTest();
            var userRepository = new UserRepository(demurrageContext);
            var user = UserFactory.GetUser();
            var userProject = UserProjectFactory.GetUserProject();

            demurrageContext.User.Add(user);

            demurrageContext.UserProject.Add(userProject);

            await demurrageContext.SaveChangesAsync();

            var result = await userRepository.ListUsersByProjectId(userProject.ProjectId);

            Assert.NotEmpty(result);
            Assert.Equal(user.Name, result.First().Name);
        }

        [Fact]
        public async Task ListUsersByGroupId_ShouldReturnUserList()
        {
            var demurrageContext = DbContextFactory.GetDemurrageContextTest();
            var userRepository = new UserRepository(demurrageContext);
            var user = UserFactory.GetUser();
            var group = GroupFactory.GetGroup();

            demurrageContext.User.Add(user);
            demurrageContext.Group.Add(group);
            await demurrageContext.SaveChangesAsync();

            var userGroup = UserGroupFactory.GetUserGroup(user.Id, group.Id);
            demurrageContext.UserGroup.Add(userGroup);

            await demurrageContext.SaveChangesAsync();

            var result = await userRepository.ListUsersByGroupId(1);

            Assert.NotEmpty(result);
            Assert.Equal(user.Name, result.First().Name);
        }

        [Fact]
        public async Task GetUserByEmailAndPassword_ShouldReturnUserInstance()
        {
            var demurrageContext = DbContextFactory.GetDemurrageContextTest();
            var userRepository = new UserRepository(demurrageContext);
            var user = UserFactory.GetUser();

            demurrageContext.Add(user);
            await demurrageContext.SaveChangesAsync();

            var result = await userRepository.GetUserByEmailAndPassword(user.Email, user.Password);

            Assert.IsType<User>(result);
            Assert.Equal(user.Name, result.Name);
        }

        [Fact]
        public async Task GetUserByEmail_ShouldReturnUserInstance()
        {
            var demurrageContext = DbContextFactory.GetDemurrageContextTest();
            var userRepository = new UserRepository(demurrageContext);
            var user = UserFactory.GetUser();

            demurrageContext.Add(user);
            await demurrageContext.SaveChangesAsync();

            var result = await userRepository.GetUserByEmail(user.Email);

            Assert.IsType<User>(result);
            Assert.Equal(user.Name, result.Name);
        }

        [Fact]
        public async Task GetUserById_ShouldReturnUserInstance()
        {
            var demurrageContext = DbContextFactory.GetDemurrageContextTest();
            var userRepository = new UserRepository(demurrageContext);
            var user = UserFactory.GetUser();

            demurrageContext.Add(user);
            await demurrageContext.SaveChangesAsync();

            var result = await userRepository.GetUserById(user.Id);

            Assert.IsType<User>(result);
            Assert.Equal(user.Name, result.Name);
        }


        [Fact]
        public async Task UpdateUser_ShouldReturnGreaterThanZero()
        {
            var demurrageContext = DbContextFactory.GetDemurrageContextTest();
            var userRepository = new UserRepository(demurrageContext);
            var user = UserFactory.GetUser();

            var updatedUser = UserFactory.GetUser();
            updatedUser.Name = "updated-name";
            updatedUser.Email = "updated-email@test.com";
            updatedUser.Role = "Administrator";

            demurrageContext.User.Add(user);

            await demurrageContext.SaveChangesAsync();

            var result = await userRepository.UpdateUser(user.Id, user, updatedUser);

            var userAfterUpdate = await demurrageContext.User.Where(u => u.Id == user.Id).FirstOrDefaultAsync();

            Assert.Equal(updatedUser.Name, result.Name);
            Assert.Equal(updatedUser.Name, userAfterUpdate.Name);
            Assert.Equal(updatedUser.Email, userAfterUpdate.Email);

        }

        [Fact]
        public async Task UpdateUserPassword_ShouldReturnUserInstance()
        {
            var demurrageContext = DbContextFactory.GetDemurrageContextTest();
            var userRepository = new UserRepository(demurrageContext);
            var user = UserFactory.GetUser();

            demurrageContext.User.Add(user);

            await demurrageContext.SaveChangesAsync();

            var result = await userRepository.UpdateUserPassword(user.Email, "my-new-password");

            Assert.NotNull(result);
            Assert.Equal("my-new-password", result.Password);

        }

        [Fact]
        public async Task DeleteUser_ShouldReturnGreaterThanZero()
        {
            var demurrageContext = DbContextFactory.GetDemurrageContextTest();
            var userRepository = new UserRepository(demurrageContext);
            var user = UserFactory.GetUser();

            demurrageContext.User.Add(user);

            await demurrageContext.SaveChangesAsync();

            var result = await userRepository.DeleteUser(user);

            var userAfterDelete = await demurrageContext.User.Where(u => u.Id == user.Id).FirstOrDefaultAsync();

            Assert.Equal(1, result);
            Assert.Null(userAfterDelete);

        }

        [Fact]
        public async Task RemoveUserFromGroup_ShouldReturnGreaterThanZero()
        {
            var demurrageContext = DbContextFactory.GetDemurrageContextTest();
            var userRepository = new UserRepository(demurrageContext);

            var user = UserFactory.GetUser();
            var group = GroupFactory.GetGroup();
            var project = ProjectFactory.GetProject();
            project.UsersProject.Clear();

            demurrageContext.User.Add(user);
            await demurrageContext.SaveChangesAsync();

            var userProject = UserProjectFactory.GetUserProject(user.Id, project.Id);
            var userGroup = UserGroupFactory.GetUserGroup(user.Id, group.Id);

            project.UsersProject.Add(userProject);
            group.UsersGroup.Add(userGroup);

            project.Group = group;
            project.GroupId = group.Id;

            if (!demurrageContext.Group.Local.Any(g => g.Id == group.Id))
            {
                demurrageContext.Group.Attach(group);
            }

            if (!demurrageContext.Project.Local.Any(p => p.Id == project.Id))
            {
                demurrageContext.Project.Attach(project);
            }

            await demurrageContext.SaveChangesAsync();

            demurrageContext.ChangeTracker.Clear();

            var result = await userRepository.RemoveUserFromGroup(user.Id, group.Id);

            var userGroupAfterDelete = await demurrageContext.UserGroup
                .Where(ug => ug.UserId == user.Id && ug.GroupId == group.Id)
                .FirstOrDefaultAsync();

            Assert.NotEqual(0, result);
            Assert.Null(userGroupAfterDelete);
        }





    }
}

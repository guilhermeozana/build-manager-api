using Marelli.Business.Exceptions;
using Marelli.Business.Services;
using Marelli.Domain.Dtos;
using Marelli.Domain.Entities;
using Marelli.Infra.IRepositories;
using Marelli.Test.Utils.Factories;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace Marelli.Test.Services
{
    public class UserServiceTest
    {

        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IGroupRepository> _groupRepositoryMock;
        private readonly Mock<IProjectRepository> _projectRepositoryMock;
        private readonly Mock<IUserTokenRepository> _userTokenRepositoryMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;

        private UserService userService;

        public UserServiceTest()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _groupRepositoryMock = new Mock<IGroupRepository>();
            _projectRepositoryMock = new Mock<IProjectRepository>();
            _userTokenRepositoryMock = new Mock<IUserTokenRepository>();
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();

            userService = new UserService(_userRepositoryMock.Object, _groupRepositoryMock.Object, _projectRepositoryMock.Object, _userTokenRepositoryMock.Object, _httpContextAccessorMock.Object);
        }

        [Fact]
        public async Task SaveUserToken_ShouldReturnGreaterThanZero()
        {
            _userRepositoryMock.Setup(n => n.SaveUser(It.IsAny<User>())).ReturnsAsync(1);

            var res = await userService.SaveUser(UserFactory.GetUser());

            Assert.Equal(1, res);
        }

        [Fact]
        public async Task SaveUser_ShouldThrowUserAlreadyExistsException()
        {
            _userRepositoryMock.Setup(u => u.GetUserByEmail(It.IsAny<string>())).ReturnsAsync((User)UserFactory.GetUser());

            await Assert.ThrowsAsync<AlreadyExistsException>(async () => await userService.SaveUser(UserFactory.GetUser()));
        }

        [Fact]
        public async Task SaveUser_ShouldThrowInvalidPasswordException()
        {
            _userRepositoryMock.Setup(u => u.GetUserByEmail(It.IsAny<string>())).ReturnsAsync((User)null);

            var user = UserFactory.GetUser();
            user.Password = "123";

            await Assert.ThrowsAsync<InvalidPasswordException>(async () => await userService.SaveUser(user));
        }

        [Fact]
        public async Task ListUsers_ShouldReturnUserResponseList()
        {
            var userListExpected = new List<UserResponse>() { UserFactory.GetUserResponse() };

            _userRepositoryMock.Setup(u => u.ListUsers()).ReturnsAsync(userListExpected);
            _groupRepositoryMock.Setup(g => g.ListGroupsByUser(It.IsAny<int>())).ReturnsAsync(new List<Group> { GroupFactory.GetGroup() });
            _projectRepositoryMock.Setup(p => p.ListProjects(It.IsAny<int>())).ReturnsAsync(new List<Project> { ProjectFactory.GetProject() });

            var res = await userService.ListUsers();

            Assert.NotEmpty(res);
            Assert.Equal(userListExpected.First().Name, res.First().Name);
            Assert.Equal(userListExpected.Count, res.Count);
        }

        [Fact]
        public async Task ListUsersByProjectId_ShouldReturnUserList()
        {

            var userListExpected = new List<User>() { UserFactory.GetUser() };

            _userRepositoryMock.Setup(u => u.ListUsersByProjectId(It.IsAny<int>())).ReturnsAsync(userListExpected);

            var res = await userService.ListUsersByProjectId(1);

            Assert.NotEmpty(res);
            Assert.Equal(userListExpected.First().Name, res.First().Name);
            Assert.Equal(userListExpected.Count, res.Count);
        }

        [Fact]
        public async Task ListUsersByGroupId_ShouldReturnUserList()
        {
            var userListExpected = new List<User>() { UserFactory.GetUser() };

            _userRepositoryMock.Setup(u => u.ListUsersByGroupId(It.IsAny<int>())).ReturnsAsync(userListExpected);

            var res = await userService.ListUsersByGroupId(1);

            Assert.NotEmpty(res);
            Assert.Equal(userListExpected.First().Name, res.First().Name);
            Assert.Equal(userListExpected.Count, res.Count);
        }

        [Fact]
        public async Task GetUserByEmailAndPassword_ShouldReturnUserInstance()
        {

            var userExpected = UserFactory.GetUser();

            _userRepositoryMock.Setup(u => u.GetUserByEmailAndPassword(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(userExpected);

            var res = await userService.GetUserByEmailAndPassword("my-email", "my-password");

            Assert.NotNull(res);
            Assert.Equal(userExpected.Name, res.Name);
        }

        [Fact]
        public async Task GetUserByEmailAndPassword_ShouldThrowNotFoundException()
        {
            var userExpected = UserFactory.GetUser();

            _userRepositoryMock.Setup(u => u.GetUserByEmailAndPassword(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync((User)null);

            await Assert.ThrowsAsync<NotFoundException>(async () => await userService.GetUserByEmailAndPassword("my-email", "my-password"));
        }

        [Fact]
        public async Task GetUserResponseByEmail_ShouldReturnUserResponseInstance()
        {
            var userExpected = UserFactory.GetUserResponse();

            _userRepositoryMock.Setup(u => u.GetUserByEmail(It.IsAny<string>())).ReturnsAsync(UserFactory.GetUser());
            _groupRepositoryMock.Setup(g => g.GetGroupById(It.IsAny<int>())).ReturnsAsync(GroupFactory.GetGroup());
            _projectRepositoryMock.Setup(p => p.ListProjects(It.IsAny<int>())).ReturnsAsync(new List<Project> { ProjectFactory.GetProject() });

            var res = await userService.GetUserResponseByEmail("my-email");

            Assert.NotNull(res);
            Assert.Equal(userExpected.Name, res.Name);
            Assert.Equal(userExpected.GetType(), res.GetType());
        }

        [Fact]
        public async Task GetUserByEmail_ShouldReturnUserInstance()
        {

            var userExpected = UserFactory.GetUser();

            _userRepositoryMock.Setup(u => u.GetUserByEmail(It.IsAny<string>())).ReturnsAsync(userExpected);

            var res = await userService.GetUserByEmail("my-email");

            Assert.NotNull(res);
            Assert.Equal(userExpected.Name, res.Name);
        }

        [Fact]
        public async Task GetUserById_ShouldReturnUserInstance()
        {
            var userExpected = UserFactory.GetUser();

            _userRepositoryMock.Setup(u => u.GetUserById(It.IsAny<int>())).ReturnsAsync(userExpected);

            var res = await userService.GetUserById(1);

            Assert.NotNull(res);
            Assert.Equal(userExpected.Name, res.Name);
        }

        [Fact]
        public async Task GetUserById_ShouldThrowNotFoundException()
        {
            _userRepositoryMock.Setup(u => u.GetUserById(It.IsAny<int>())).ReturnsAsync((User)null);

            await Assert.ThrowsAsync<NotFoundException>(async () => await userService.GetUserById(1));
        }

        [Fact]
        public async Task UpdateUserPassword_ShouldReturnUserInstance()
        {

            var userExpected = UserFactory.GetUser();

            _userRepositoryMock.Setup(u => u.GetUserByEmail(It.IsAny<string>())).ReturnsAsync(UserFactory.GetUser());
            _userRepositoryMock.Setup(u => u.UpdateUserPassword(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(userExpected);

            var res = await userService.UpdateUserPassword("my-email", "my-password123@A");

            Assert.NotNull(res);
            Assert.Equal(userExpected, res);
        }

        [Fact]
        public async Task UpdateUserPassword_ShouldThrowInvalidPasswordException_WhenLessThanEight()
        {

            _userRepositoryMock.Setup(u => u.GetUserByEmail(It.IsAny<string>())).ReturnsAsync(UserFactory.GetUser());

            await Assert.ThrowsAsync<InvalidPasswordException>(async () => await userService.UpdateUserPassword("my-email", "123"));
        }


        [Fact]
        public async Task UpdateUserPassword_ShouldThrowInvalidPasswordException_WhenDoesNotHaveLowerCaseLetter()
        {

            _userRepositoryMock.Setup(u => u.GetUserByEmail(It.IsAny<string>())).ReturnsAsync(UserFactory.GetUser());

            await Assert.ThrowsAsync<InvalidPasswordException>(async () => await userService.UpdateUserPassword("my-email", "MY-INVALID-PASSWORD"));
        }

        [Fact]
        public async Task UpdateUserPassword_ShouldThrowInvalidPasswordException_WhenDoesNotHaveCapitalLetter()
        {

            _userRepositoryMock.Setup(u => u.GetUserByEmail(It.IsAny<string>())).ReturnsAsync(UserFactory.GetUser());

            await Assert.ThrowsAsync<InvalidPasswordException>(async () => await userService.UpdateUserPassword("my-email", "my-invalid-password"));
        }

        [Fact]
        public async Task UpdateUserPassword_ShouldThrowInvalidPasswordException_WhenDoesNotHaveSpecialCharacters()
        {

            _userRepositoryMock.Setup(u => u.GetUserByEmail(It.IsAny<string>())).ReturnsAsync(UserFactory.GetUser());

            await Assert.ThrowsAsync<InvalidPasswordException>(async () => await userService.UpdateUserPassword("my-email", "MyInvalidPassword123"));
        }

        [Fact]
        public async Task UpdateUser_ShouldReturnGreaterThanZero()
        {
            _httpContextAccessorMock.Setup(h => h.HttpContext.Request.Headers["Authorization"]).Returns("my-token");
            _userRepositoryMock.Setup(u => u.GetUserById(It.IsAny<int>())).ReturnsAsync(UserFactory.GetUser());
            _userRepositoryMock.Setup(u => u.UpdateUser(It.IsAny<int>(), It.IsAny<User>(), It.IsAny<User>())).ReturnsAsync(UserFactory.GetUser());

            var res = await userService.UpdateUser(1, UserFactory.GetUser());

            Assert.Equal(1, res);
        }

        [Fact]
        public async Task UpdateUser_ShouldThrowNotFoundException()
        {
            _userRepositoryMock.Setup(u => u.GetUserByEmail(It.IsAny<string>())).ReturnsAsync((User)null);

            await Assert.ThrowsAsync<NotFoundException>(async () => await userService.UpdateUser(1, UserFactory.GetUser()));
        }

        [Fact]
        public async Task DeleteUser_ShouldReturnGreaterThanZero()
        {
            _userRepositoryMock.Setup(u => u.GetUserById(It.IsAny<int>())).ReturnsAsync(UserFactory.GetUser());
            _userRepositoryMock.Setup(u => u.DeleteUser(It.IsAny<User>())).ReturnsAsync(1);

            var res = await userService.DeleteUser(1);

            Assert.Equal(1, res);
        }

        [Fact]
        public async Task DeleteUser_ShouldThrowNotFoundException()
        {

            _userRepositoryMock.Setup(u => u.GetUserById(It.IsAny<int>())).ReturnsAsync((User)null);

            await Assert.ThrowsAsync<NotFoundException>(async () => await userService.DeleteUser(1));
        }


    }
}
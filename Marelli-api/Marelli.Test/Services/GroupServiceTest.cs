using Marelli.Business.Exceptions;
using Marelli.Business.Services;
using Marelli.Domain.Dtos;
using Marelli.Domain.Entities;
using Marelli.Infra.IRepositories;
using Marelli.Test.Utils.Factories;
using Moq;
using Xunit;

namespace Marelli.Test.Services
{
    public class GroupServiceTest
    {
        private readonly Mock<IGroupRepository> _groupRepositoryMock;
        private readonly Mock<IUserRepository> _userRepositoryMock;

        private readonly GroupService _groupService;

        public GroupServiceTest()
        {
            _groupRepositoryMock = new Mock<IGroupRepository>();
            _userRepositoryMock = new Mock<IUserRepository>();

            _groupService = new GroupService(_groupRepositoryMock.Object, _userRepositoryMock.Object);
        }


        [Fact]
        public async Task SaveGroup_ShouldReturnGreaterThanZero()
        {
            _groupRepositoryMock.Setup(g => g.SaveGroup(It.IsAny<GroupRequest>())).ReturnsAsync(1);
            _groupRepositoryMock.Setup(g => g.GetGroupByName(It.IsAny<string>())).ReturnsAsync((Group)null);
            _userRepositoryMock.Setup(g => g.GetUserById(It.IsAny<int>())).ReturnsAsync(UserFactory.GetUser());
            _userRepositoryMock.Setup(g => g.UpdateUser(It.IsAny<int>(), It.IsAny<User>(), It.IsAny<User>())).ReturnsAsync(UserFactory.GetUser());

            var res = await _groupService.SaveGroup(GroupFactory.GetGroupRequest());

            Assert.Equal(1, res);
        }

        [Fact]
        public async Task SaveGroup_ShouldThrowAlreadyExistsException()
        {
            _groupRepositoryMock.Setup(g => g.GetGroupByName(It.IsAny<string>())).ReturnsAsync(GroupFactory.GetGroup());

            await Assert.ThrowsAsync<AlreadyExistsException>(async () => await _groupService.SaveGroup(GroupFactory.GetGroupRequest()));
        }

        [Fact]
        public async Task ListGroup_ShouldReturnGroupList()
        {
            var groupList = new List<Group>() { GroupFactory.GetGroup() };
            var userList = new List<User>() { UserFactory.GetUser() };

            _groupRepositoryMock.Setup(g => g.ListGroups()).ReturnsAsync(groupList);
            _userRepositoryMock.Setup(g => g.ListUsersByGroupId(It.IsAny<int>())).ReturnsAsync(userList);

            var res = await _groupService.ListGroups();

            Assert.NotEmpty(res);
            Assert.Equal(groupList.First().Name, res.First().Name);
            Assert.Equal(userList, res.First().Users);
        }

        [Fact]
        public async Task GetGroupById_ShouldReturnGroupInstance()
        {
            var expectedGroup = GroupFactory.GetGroup();

            _groupRepositoryMock.Setup(g => g.GetGroupById(It.IsAny<int>())).ReturnsAsync(expectedGroup);

            var res = await _groupService.GetGroupById(1);

            Assert.NotNull(res);
            Assert.Equal(expectedGroup.Name, res.Name);
        }

        [Fact]
        public async Task GetGroupById_ShouldThrowNotFoundException()
        {
            _groupRepositoryMock.Setup(g => g.GetGroupById(It.IsAny<int>())).ThrowsAsync(new NotFoundException());

            await Assert.ThrowsAsync<NotFoundException>(async () => await _groupService.GetGroupById(1));
        }

        [Fact]
        public async Task UpdateGroup_ShouldReturnGreaterThanZero()
        {

            _groupRepositoryMock.Setup(g => g.GetGroupById(It.IsAny<int>())).ReturnsAsync(GroupFactory.GetGroup());
            _groupRepositoryMock.Setup(g => g.GetGroupWithSameName(It.IsAny<int>(), It.IsAny<string>())).ReturnsAsync((Group)null);
            _groupRepositoryMock.Setup(g => g.UpdateGroup(It.IsAny<int>(), It.IsAny<Group>(), It.IsAny<GroupRequest>())).ReturnsAsync(1);

            var res = await _groupService.UpdateGroup(1, GroupFactory.GetGroupRequest());

            Assert.Equal(1, res);
        }

        [Fact]
        public async Task UpdateGroup_ShouldThrowNotFoundException()
        {
            _groupRepositoryMock.Setup(g => g.GetGroupById(It.IsAny<int>())).ThrowsAsync(new NotFoundException());

            await Assert.ThrowsAsync<NotFoundException>(async () => await _groupService.UpdateGroup(1, GroupFactory.GetGroupRequest()));
        }

        [Fact]
        public async Task UpdateGroup_ShouldThrowAlreadyExistsException()
        {
            _groupRepositoryMock.Setup(g => g.GetGroupById(It.IsAny<int>())).ReturnsAsync(GroupFactory.GetGroup());
            _groupRepositoryMock.Setup(g => g.GetGroupWithSameName(It.IsAny<int>(), It.IsAny<string>())).ReturnsAsync(GroupFactory.GetGroup());

            await Assert.ThrowsAsync<AlreadyExistsException>(async () => await _groupService.UpdateGroup(1, GroupFactory.GetGroupRequest()));
        }

        [Fact]
        public async Task DeleteGroup_ShouldReturnGreaterThanZero()
        {
            _groupRepositoryMock.Setup(g => g.GetGroupById(It.IsAny<int>())).ReturnsAsync(GroupFactory.GetGroup());
            _groupRepositoryMock.Setup(g => g.DeleteGroup(It.IsAny<Group>())).ReturnsAsync(1);

            var res = await _groupService.DeleteGroup(1);

            Assert.Equal(1, res);
        }

        [Fact]
        public async Task DeleteGroup_ShouldThrowNotFoundException()
        {
            _groupRepositoryMock.Setup(g => g.GetGroupById(It.IsAny<int>())).ThrowsAsync(new NotFoundException());

            await Assert.ThrowsAsync<NotFoundException>(async () => await _groupService.DeleteGroup(1));
        }

    }
}
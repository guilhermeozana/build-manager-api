using Marelli.Domain.Dtos;
using Marelli.Domain.Entities;
using Marelli.Infra.IRepositories;
using Marelli.Infra.Repositories;
using Marelli.Test.Utils.Factories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Xunit;

namespace Marelli.Test.Repositories
{
    public class GroupRepositoryTest
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IMemoryCache> _memoryCacheMock;

        public GroupRepositoryTest()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _memoryCacheMock = new Mock<IMemoryCache>();
        }

        [Fact]
        public async Task SaveGroup_ShouldReturnGreaterThanZero()
        {
            var demurrageContext = DbContextFactory.GetDemurrageContextTest();
            var groupRepository = new GroupRepository(demurrageContext, _userRepositoryMock.Object);

            await demurrageContext.SaveChangesAsync();

            var group = GroupFactory.GetGroupRequest();

            _userRepositoryMock.Setup(u => u.ListUsers()).ReturnsAsync(new List<UserResponse>() { });
            _userRepositoryMock.Setup(u => u.GetUserById(It.IsAny<int>())).ReturnsAsync(UserFactory.GetUser);

            var result = await groupRepository.SaveGroup(group);
            Assert.NotEqual(0, result);
        }

        [Fact]
        public async Task ListGroups_ShouldReturnGroupList()
        {
            var demurrageContext = DbContextFactory.GetDemurrageContextTest();
            var groupRepository = new GroupRepository(demurrageContext, _userRepositoryMock.Object);
            var group = GroupFactory.GetGroup();

            demurrageContext.Add(group);
            await demurrageContext.SaveChangesAsync();

            var result = await groupRepository.ListGroups();
            var groupByName = result.Where(p => p.Name == group.Name).FirstOrDefault();

            Assert.NotEmpty(result);
            Assert.NotNull(groupByName);
        }

        [Fact]
        public async Task GetGroupById_ShouldReturnGroupInstance()
        {
            var demurrageContext = DbContextFactory.GetDemurrageContextTest();
            var groupRepository = new GroupRepository(demurrageContext, _userRepositoryMock.Object);
            var group = GroupFactory.GetGroup();

            demurrageContext.Add(group);
            await demurrageContext.SaveChangesAsync();

            var result = await groupRepository.GetGroupById(group.Id);

            Assert.IsType<Group>(result);
            Assert.Equal(group.Name, result.Name);
        }

        [Fact]
        public async Task GetGroupByName_ShouldReturnGroupInstance()
        {
            var demurrageContext = DbContextFactory.GetDemurrageContextTest();
            var groupRepository = new GroupRepository(demurrageContext, _userRepositoryMock.Object);
            var group = GroupFactory.GetGroup();

            demurrageContext.Add(group);
            await demurrageContext.SaveChangesAsync();

            var result = await groupRepository.GetGroupByName(group.Name);

            Assert.IsType<Group>(result);
            Assert.Equal(group.Name, result.Name);
        }

        [Fact]
        public async Task GetGroupWithSameName_ShouldReturnGroupInstance()
        {
            var demurrageContext = DbContextFactory.GetDemurrageContextTest();
            var groupRepository = new GroupRepository(demurrageContext, _userRepositoryMock.Object);
            var group = GroupFactory.GetGroup();
            var otherGroup = GroupFactory.GetGroup();
            otherGroup.Name = group.Name;

            demurrageContext.Add(group);
            demurrageContext.Add(otherGroup);

            await demurrageContext.SaveChangesAsync();

            var result = await groupRepository.GetGroupWithSameName(group.Id, group.Name);

            Assert.IsType<Group>(result);
            Assert.Equal(group.Name, result.Name);
        }


        [Fact]
        public async Task UpdateGroup_ShouldReturnGreaterThanZero()
        {
            var demurrageContext = DbContextFactory.GetDemurrageContextTest();
            var groupRepository = new GroupRepository(demurrageContext, _userRepositoryMock.Object);
            var group = GroupFactory.GetGroup();

            demurrageContext.Group.Add(group);

            await demurrageContext.SaveChangesAsync();

            var updatedGroup = GroupFactory.GetGroupRequest();

            _userRepositoryMock.Setup(u => u.ListUsersByGroupId(It.IsAny<int>())).ReturnsAsync(new List<User> { UserFactory.GetUser() });
            _userRepositoryMock.Setup(u => u.GetUserById(It.IsAny<int>())).ReturnsAsync(UserFactory.GetUser());

            var cacheEntryMock = new Mock<ICacheEntry>();
            _memoryCacheMock
                .Setup(m => m.CreateEntry(It.IsAny<object>()))
                .Returns(cacheEntryMock.Object);

            var result = await groupRepository.UpdateGroup(group.Id, group, updatedGroup);


            var groupAfterUpdate = await demurrageContext.Group.Where(u => u.Id == group.Id).FirstOrDefaultAsync();

            Assert.NotEqual(0, result);
            Assert.Equal(updatedGroup.Name, groupAfterUpdate.Name);

        }

        [Fact]
        public async Task DeleteGroup_ShouldReturnGreaterThanZero()
        {
            var demurrageContext = DbContextFactory.GetDemurrageContextTest();
            var groupRepository = new GroupRepository(demurrageContext, _userRepositoryMock.Object);
            var group = GroupFactory.GetGroup();

            demurrageContext.Group.Add(group);

            await demurrageContext.SaveChangesAsync();

            var result = await groupRepository.DeleteGroup(group);

            var groupAfterDelete = await demurrageContext.Group.Where(u => u.Id == group.Id).FirstOrDefaultAsync();

            Assert.NotEqual(0, result);
            Assert.Null(groupAfterDelete);

        }

    }
}

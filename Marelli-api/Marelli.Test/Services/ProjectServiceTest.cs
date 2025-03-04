using Marelli.Business.Exceptions;
using Marelli.Business.Services;
using Marelli.Domain.Entities;
using Marelli.Infra.IRepositories;
using Marelli.Test.Utils.Factories;
using Moq;
using Xunit;

namespace Marelli.Test.Services
{
    public class ProjectServiceTest
    {
        private readonly Mock<IProjectRepository> _projectRepositoryMock;
        private readonly Mock<IGroupRepository> _groupRepositoryMock;
        private readonly Mock<IUserRepository> _userRepositoryMock;

        private readonly ProjectService _projectService;

        public ProjectServiceTest()
        {
            _projectRepositoryMock = new Mock<IProjectRepository>();
            _groupRepositoryMock = new Mock<IGroupRepository>();
            _userRepositoryMock = new Mock<IUserRepository>();

            _projectService = new ProjectService(_projectRepositoryMock.Object, _groupRepositoryMock.Object, _userRepositoryMock.Object);
        }

        [Fact]
        public async Task SaveProject_ShouldReturnGreaterThanZero()
        {
            _groupRepositoryMock.Setup(g => g.GetGroupById(It.IsAny<int>())).ReturnsAsync(GroupFactory.GetGroup());
            _projectRepositoryMock.Setup(p => p.ListProjectsByGroupId(It.IsAny<int>())).ReturnsAsync(new List<Project> { });
            _userRepositoryMock.Setup(u => u.GetUserById(It.IsAny<int>())).ReturnsAsync(UserFactory.GetUser());
            _projectRepositoryMock.Setup(p => p.SaveProject(It.IsAny<Project>())).ReturnsAsync(1);

            var res = await _projectService.SaveProject(ProjectFactory.GetProject());

            Assert.Equal(1, res);
        }

        [Fact]
        public async Task SaveProject_ShouldThrowNotFoundException_WhenGroupDoesNotExist()
        {
            var projectList = new List<Project>() { ProjectFactory.GetProject() };

            _groupRepositoryMock.Setup(g => g.GetGroupById(It.IsAny<int>())).ReturnsAsync((Group)null);

            await Assert.ThrowsAsync<NotFoundException>(async () => await _projectService.SaveProject(ProjectFactory.GetProject()));
        }

        [Fact]
        public async Task SaveProject_ShouldThrowAlreadyExistsException()
        {
            var project = ProjectFactory.GetProject();

            _groupRepositoryMock.Setup(g => g.GetGroupById(It.IsAny<int>())).ReturnsAsync(GroupFactory.GetGroup());
            _projectRepositoryMock.Setup(p => p.ListProjectsByGroupId(It.IsAny<int>())).ReturnsAsync(new List<Project>() { project });

            await Assert.ThrowsAsync<AlreadyExistsException>(async () => await _projectService.SaveProject(project));
        }

        [Fact]
        public async Task ListProjects_ShouldReturnProjectList()
        {
            var projectList = new List<Project>() { ProjectFactory.GetProject() };
            var userList = new List<User>() { UserFactory.GetUser() };

            _projectRepositoryMock.Setup(p => p.ListProjects(It.IsAny<int>())).ReturnsAsync(projectList);
            _userRepositoryMock.Setup(u => u.ListUsersByProjectId(It.IsAny<int>())).ReturnsAsync(userList);

            var res = await _projectService.ListProjects(1);

            Assert.NotEmpty(res);
            Assert.Equal(projectList.First().Name, res.First().Name);
        }

        [Fact]
        public async Task ListProjectsByGroupId_ShouldReturnProjectList()
        {
            var projectList = new List<Project>() { ProjectFactory.GetProject() };

            _projectRepositoryMock.Setup(p => p.ListProjectsByGroupId(It.IsAny<int>())).ReturnsAsync(projectList);

            var res = await _projectService.ListProjectsByGroupId(1);

            Assert.NotEmpty(res);
            Assert.Equal(projectList, res);
        }

        [Fact]
        public async Task ListProjectsByName_ShouldReturnProjectList()
        {
            var projectList = new List<Project>() { ProjectFactory.GetProject() };
            var userList = new List<User>() { UserFactory.GetUser() };

            _projectRepositoryMock.Setup(p => p.ListProjectsByName(It.IsAny<string>())).ReturnsAsync(projectList);
            _userRepositoryMock.Setup(u => u.ListUsersByProjectId(It.IsAny<int>())).ReturnsAsync(userList);

            var res = await _projectService.ListProjectsByName("my-project");

            Assert.NotEmpty(res);
            Assert.Equal(projectList.First().Name, res.First().Name);
        }

        [Fact]
        public async Task GetProjectById_ShouldReturnProjectInstance()
        {
            var projectExpected = ProjectFactory.GetProject();

            _projectRepositoryMock.Setup(p => p.GetProjectById(It.IsAny<int>())).ReturnsAsync(projectExpected);

            var res = await _projectService.GetProjectById(1);

            Assert.NotNull(res);
            Assert.Equal(projectExpected.Name, res.Name);
            Assert.Equal(projectExpected.Description, res.Description);
        }

        [Fact]
        public async Task GetProjectById_ShouldThrowNotFoundException()
        {
            _projectRepositoryMock.Setup(p => p.GetProjectById(It.IsAny<int>())).ReturnsAsync((Project)null);

            await Assert.ThrowsAsync<NotFoundException>(async () => await _projectService.GetProjectById(1));
        }

        [Fact]
        public async Task UpdateProject_ShouldReturnGreaterThanZero()
        {
            var project = ProjectFactory.GetProject();

            _projectRepositoryMock.Setup(p => p.GetProjectById(It.IsAny<int>())).ReturnsAsync(project);
            _groupRepositoryMock.Setup(g => g.GetGroupById(It.IsAny<int>())).ReturnsAsync(GroupFactory.GetGroup());
            _projectRepositoryMock.Setup(p => p.ListProjectsByGroupId(It.IsAny<int>())).ReturnsAsync(new List<Project> { });
            _userRepositoryMock.Setup(u => u.GetUserById(It.IsAny<int>())).ReturnsAsync(UserFactory.GetUser());
            _projectRepositoryMock.Setup(p => p.UpdateProject(It.IsAny<int>(), It.IsAny<Project>(), It.IsAny<Project>())).ReturnsAsync(1);

            var res = await _projectService.UpdateProject(1, project);

            Assert.Equal(1, res);
        }

        [Fact]
        public async Task UpdateProject_ShouldThrowAlreadyExistsException_WhenProjectExistsInGroup()
        {
            var project = ProjectFactory.GetProject();

            _projectRepositoryMock.Setup(p => p.GetProjectById(It.IsAny<int>())).ReturnsAsync(project);

            project.Id = 99;

            _projectRepositoryMock.Setup(p => p.ListProjectsByGroupId(It.IsAny<int>())).ReturnsAsync(new List<Project> { project });

            await Assert.ThrowsAsync<NotFoundException>(async () => await _projectService.UpdateProject(1, ProjectFactory.GetProject()));
        }

        [Fact]
        public async Task UpdateProject_ShouldThrowNotFoundException_WhenGroupDoesNotExist()
        {
            var project = ProjectFactory.GetProject();

            _projectRepositoryMock.Setup(p => p.GetProjectById(It.IsAny<int>())).ReturnsAsync(project);
            _groupRepositoryMock.Setup(g => g.GetGroupById(It.IsAny<int>())).ReturnsAsync((Group)null);


            await Assert.ThrowsAsync<NotFoundException>(async () => await _projectService.UpdateProject(1, ProjectFactory.GetProject()));
        }

        [Fact]
        public async Task DeleteProjectById_ShouldReturnGreaterThanZero()
        {
            _projectRepositoryMock.Setup(p => p.GetProjectById(It.IsAny<int>())).ReturnsAsync(ProjectFactory.GetProject());
            _projectRepositoryMock.Setup(p => p.DeleteProjectById(It.IsAny<int>(), It.IsAny<Project>())).ReturnsAsync(1);

            var res = await _projectService.DeleteProjectById(1);

            Assert.Equal(1, res);
        }

    }
}

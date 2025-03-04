using Marelli.Domain.Dtos;
using Marelli.Domain.Entities;
using Marelli.Infra.IRepositories;
using Marelli.Infra.Repositories;
using Marelli.Test.Utils.Factories;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace Marelli.Test.Repositories
{
    public class ProjectRepositoryTest
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IGroupRepository> _groupRepositoryMock;


        public ProjectRepositoryTest()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _groupRepositoryMock = new Mock<IGroupRepository>();
            ;

        }

        [Fact]
        public async Task SaveProject_ShouldReturnGreaterThanZero()
        {
            var demurrageContext = DbContextFactory.GetDemurrageContextTest();
            var projectRepository = new ProjectRepository(demurrageContext, _userRepositoryMock.Object, _groupRepositoryMock.Object);

            var project = ProjectFactory.GetProject();
            _userRepositoryMock.Setup(u => u.ListUsers()).ReturnsAsync(new List<UserResponse>() { });

            var result = await projectRepository.SaveProject(project);

            Assert.NotEqual(0, result);
        }

        [Fact]
        public async Task ListProjects_ShouldReturnProjectList()
        {
            var demurrageContext = DbContextFactory.GetDemurrageContextTest();
            var projectRepository = new ProjectRepository(demurrageContext, _userRepositoryMock.Object, _groupRepositoryMock.Object);
            var project = ProjectFactory.GetProject();

            demurrageContext.Add(project);
            await demurrageContext.SaveChangesAsync();

            var result = await projectRepository.ListProjects(It.IsAny<int>());
            var projectByName = result.Where(p => p.Name == project.Name).FirstOrDefault();

            Assert.NotEmpty(result);
            Assert.NotNull(projectByName);
        }

        [Fact]
        public async Task ListProjectsByGroupId_ShouldReturnProjectList()
        {
            var demurrageContext = DbContextFactory.GetDemurrageContextTest();
            var projectRepository = new ProjectRepository(demurrageContext, _userRepositoryMock.Object, _groupRepositoryMock.Object);
            var project = ProjectFactory.GetProject();

            demurrageContext.Project.Add(project);

            await demurrageContext.SaveChangesAsync();

            var result = await projectRepository.ListProjectsByGroupId(project.Id);
            var projectById = result.Where(p => p.Name == project.Name).FirstOrDefault();

            Assert.NotEmpty(result);
            Assert.NotNull(projectById);
        }

        [Fact]
        public async Task ListProjectsByUserId_ShouldReturnProjectList()
        {
            var demurrageContext = DbContextFactory.GetDemurrageContextTest();
            var projectRepository = new ProjectRepository(demurrageContext, _userRepositoryMock.Object, _groupRepositoryMock.Object);
            var project = ProjectFactory.GetProject();
            var userProject = UserProjectFactory.GetUserProject();

            demurrageContext.Project.Add(project);

            await demurrageContext.SaveChangesAsync();

            var result = await projectRepository.ListProjectsByUserId(1);
            var projectByName = result.Where(p => p.Name == project.Name).FirstOrDefault();

            Assert.NotEmpty(result);
            Assert.NotNull(projectByName);
        }

        [Fact]
        public async Task ListProjectsByName_ShouldReturnProjectList()
        {
            var demurrageContext = DbContextFactory.GetDemurrageContextTest();
            var projectRepository = new ProjectRepository(demurrageContext, _userRepositoryMock.Object, _groupRepositoryMock.Object);
            var project = ProjectFactory.GetProject();

            demurrageContext.Project.Add(project);

            await demurrageContext.SaveChangesAsync();

            var result = await projectRepository.ListProjectsByName(project.Name);

            Assert.NotEmpty(result);
            Assert.Equal(project.Name, result.First().Name);
        }

        [Fact]
        public async Task GetProjectById_ShouldReturnProjectInstance()
        {
            var demurrageContext = DbContextFactory.GetDemurrageContextTest();
            var projectRepository = new ProjectRepository(demurrageContext, _userRepositoryMock.Object, _groupRepositoryMock.Object);
            var project = ProjectFactory.GetProject();

            demurrageContext.Add(project);
            await demurrageContext.SaveChangesAsync();

            var result = await projectRepository.GetProjectById(project.Id);

            Assert.IsType<Project>(result);
            Assert.Equal(project.Name, result.Name);
        }


        [Fact]
        public async Task UpdateProject_ShouldReturnGreaterThanZero()
        {
            var demurrageContext = DbContextFactory.GetDemurrageContextTest();
            var projectRepository = new ProjectRepository(demurrageContext, _userRepositoryMock.Object, _groupRepositoryMock.Object);
            var project = ProjectFactory.GetProject();
            project.UsersProject = new UserProject[] { };

            demurrageContext.Project.Add(project);

            await demurrageContext.SaveChangesAsync();

            var updatedProject = await demurrageContext.Project.FirstOrDefaultAsync();
            updatedProject.Name = "updated-name";

            var result = await projectRepository.UpdateProject(project.Id, project, updatedProject);

            var projectAfterUpdate = await demurrageContext.Project.Where(u => u.Id == project.Id).FirstOrDefaultAsync();

            Assert.NotEqual(0, result);
            Assert.Equal(updatedProject.Name, projectAfterUpdate.Name);

        }

        [Fact]
        public async Task DeleteProject_ShouldReturnGreaterThanZero()
        {
            var demurrageContext = DbContextFactory.GetDemurrageContextTest();
            var projectRepository = new ProjectRepository(demurrageContext, _userRepositoryMock.Object, _groupRepositoryMock.Object);
            var project = ProjectFactory.GetProject();

            demurrageContext.Project.Add(project);

            await demurrageContext.SaveChangesAsync();

            var result = await projectRepository.DeleteProject(project);

            var projectAfterDelete = await demurrageContext.Project.Where(u => u.Id == project.Id).FirstOrDefaultAsync();

            Assert.NotEqual(0, result);
            Assert.Null(projectAfterDelete);

        }

        [Fact]
        public async Task DeleteProjectById_ShouldReturnGreaterThanZero()
        {
            var demurrageContext = DbContextFactory.GetDemurrageContextTest();
            var projectRepository = new ProjectRepository(demurrageContext, _userRepositoryMock.Object, _groupRepositoryMock.Object);
            var project = ProjectFactory.GetProject();

            demurrageContext.Project.Add(project);

            await demurrageContext.SaveChangesAsync();

            var result = await projectRepository.DeleteProjectById(project.Id, project);

            var projectAfterDelete = await demurrageContext.Project.Where(u => u.Id == project.Id).FirstOrDefaultAsync();

            Assert.NotEqual(0, result);
            Assert.Null(projectAfterDelete);

        }

    }
}

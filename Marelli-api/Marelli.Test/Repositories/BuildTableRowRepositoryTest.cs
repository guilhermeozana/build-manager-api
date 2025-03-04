using Marelli.Domain.Entities;
using Marelli.Infra.Repositories;
using Marelli.Test.Utils.Factories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Marelli.Test.Repositories
{
    public class BuildTableRowRepositoryTest
    {

        [Fact]
        public async Task SaveBuildTableAsync_ShouldReturnGreaterThanZero()
        {
            var demurrageContext = DbContextFactory.GetDemurrageContextTest();
            var buildTableRowRepository = new BuildTableRowRepository(demurrageContext);

            var buildTable = BuildTableRowFactory.GetBuildTable();

            var result = await buildTableRowRepository.SaveBuildTableAsync(buildTable);

            Assert.IsType<BuildTableRow>(result);
            Assert.Equal(buildTable.FileName, result.FileName);
        }

        [Fact]
        public async Task ListBuildTableAsync_ShouldReturnBuildTableRowList()
        {
            var demurrageContext = DbContextFactory.GetDemurrageContextTest();
            var buildTableRowRepository = new BuildTableRowRepository(demurrageContext);
            var buildTableRow = BuildTableRowFactory.GetBuildTable();
            var project = ProjectFactory.GetProject();

            demurrageContext.BuildTableRow.Add(buildTableRow);
            demurrageContext.Project.Add(project);

            await demurrageContext.SaveChangesAsync();

            var result = await buildTableRowRepository.ListBuildTableAsync(buildTableRow.UserId);

            Assert.NotEmpty(result);
            Assert.Equal(buildTableRow.FileName, result.First().FileName);
        }

        [Fact]
        public async Task ListBuildTableInProgressAsync_ShouldReturnBuildTableRowList()
        {
            var demurrageContext = DbContextFactory.GetDemurrageContextTest();
            var buildTableRowRepository = new BuildTableRowRepository(demurrageContext);
            var buildTableRow = BuildTableRowFactory.GetBuildTable();
            buildTableRow.Status = "Compiling";
            var project = ProjectFactory.GetProject();

            demurrageContext.BuildTableRow.Add(buildTableRow);
            demurrageContext.Project.Add(project);

            await demurrageContext.SaveChangesAsync();

            var result = await buildTableRowRepository.ListBuildTableInProgressAsync(buildTableRow.UserId);

            Assert.NotEmpty(result);
            Assert.Equal(buildTableRow.FileName, result.First().FileName);
        }

        [Fact]
        public async Task ListBuildTableInQueueAsync_ShouldReturnBuildTableRowList()
        {
            var demurrageContext = DbContextFactory.GetDemurrageContextTest();
            var buildTableRowRepository = new BuildTableRowRepository(demurrageContext);
            var buildTableRowInQueue = BuildTableRowFactory.GetBuildTable();
            buildTableRowInQueue.Status = "In Queue";
            var buildTableRowNotInQueue = BuildTableRowFactory.GetBuildTable();

            demurrageContext.BuildTableRow.Add(buildTableRowInQueue);
            demurrageContext.BuildTableRow.Add(buildTableRowNotInQueue);

            await demurrageContext.SaveChangesAsync();

            var result = await buildTableRowRepository.ListBuildTableInQueueAsync();

            Assert.NotEmpty(result);
            Assert.Single(result);
            Assert.Equal(buildTableRowInQueue.FileName, result.First().FileName);
            Assert.Equal(buildTableRowInQueue.Status, result.First().Status);
        }

        [Fact]
        public async Task ListBuildTableByProjectAsync_ShouldReturnBuildTableRowList()
        {
            var demurrageContext = DbContextFactory.GetDemurrageContextTest();
            var buildTableRowRepository = new BuildTableRowRepository(demurrageContext);
            var buildTableRow = BuildTableRowFactory.GetBuildTable();
            var project = ProjectFactory.GetProject();

            demurrageContext.BuildTableRow.Add(buildTableRow);
            demurrageContext.Project.Add(project);

            await demurrageContext.SaveChangesAsync();

            var result = await buildTableRowRepository.ListBuildTableByProjectAsync(buildTableRow.UserId);

            Assert.NotEmpty(result);
            Assert.Equal(buildTableRow.FileName, result.First().FileName);
        }

        [Fact]
        public async Task GetBuildTableRowAsync_ShouldReturnBuildTableRowInstance()
        {
            var demurrageContext = DbContextFactory.GetDemurrageContextTest();
            var buildTableRowRepository = new BuildTableRowRepository(demurrageContext);
            var buildTableRow = BuildTableRowFactory.GetBuildTable();

            demurrageContext.Add(buildTableRow);
            await demurrageContext.SaveChangesAsync();

            var result = await buildTableRowRepository.GetBuildTableRowAsync(buildTableRow.Id);

            Assert.IsType<BuildTableRow>(result);
            Assert.Equal(buildTableRow.FileName, result.FileName);
        }

        [Fact]
        public async Task GetLastUploadedByUserAsync_ShouldReturnBuildTableRowInstance()
        {
            var demurrageContext = DbContextFactory.GetDemurrageContextTest();
            var buildTableRowRepository = new BuildTableRowRepository(demurrageContext);
            var buildTableRow = BuildTableRowFactory.GetBuildTable();
            var userProject = UserProjectFactory.GetUserProject();
            var user = UserFactory.GetUserWithoutGroup();

            buildTableRow.UserId = userProject.UserId;
            buildTableRow.ProjectId = userProject.ProjectId;

            demurrageContext.Add(buildTableRow);
            demurrageContext.Add(userProject);
            demurrageContext.Add(user);
            await demurrageContext.SaveChangesAsync();

            var result = await buildTableRowRepository.GetLastUploadedByUserAsync(buildTableRow.UserId);

            Assert.IsType<BuildTableRow>(result);
            Assert.Equal(buildTableRow.FileName, result.FileName);
        }

        [Fact]
        public async Task GetLastUploadedByUserProjectsAsync_ShouldReturnBuildTableRowInstance()
        {
            var demurrageContext = DbContextFactory.GetDemurrageContextTest();
            var buildTableRowRepository = new BuildTableRowRepository(demurrageContext);
            var buildTableRow = BuildTableRowFactory.GetBuildTable();
            var userProject = UserProjectFactory.GetUserProject();

            buildTableRow.UserId = userProject.UserId;
            buildTableRow.ProjectId = userProject.ProjectId;

            demurrageContext.Add(buildTableRow);
            demurrageContext.Add(userProject);
            await demurrageContext.SaveChangesAsync();

            var result = await buildTableRowRepository.GetLastUploadedByUserProjectsAsync(buildTableRow.UserId);

            Assert.IsType<BuildTableRow>(result);
            Assert.Equal(buildTableRow.FileName, result.FileName);
        }

        [Fact]
        public async Task GetFirstInQueueAsync_ShouldReturnBuildTableRowInstance()
        {
            var demurrageContext = DbContextFactory.GetDemurrageContextTest();
            var buildTableRowRepository = new BuildTableRowRepository(demurrageContext);
            var buildTableRow = BuildTableRowFactory.GetBuildTable();
            buildTableRow.Status = "In Queue";

            demurrageContext.Add(buildTableRow);

            await demurrageContext.SaveChangesAsync();

            var buildTableRow2 = BuildTableRowFactory.GetBuildTable();
            buildTableRow2.Status = "In Queue";
            buildTableRow2.FileName = "Other FileName";

            demurrageContext.Add(buildTableRow2);

            await demurrageContext.SaveChangesAsync();

            var result = await buildTableRowRepository.GetFirstInQueueAsync(buildTableRow.UserId);

            Assert.IsType<BuildTableRow>(result);
            Assert.Equal(buildTableRow.FileName, result.FileName);
        }


        [Fact]
        public async Task UpdateBuildTableAsync_ShouldReturnGreaterThanZero()
        {
            var demurrageContext = DbContextFactory.GetDemurrageContextTest();
            var buildTableRowRepository = new BuildTableRowRepository(demurrageContext);
            var buildTableRow = BuildTableRowFactory.GetBuildTable();

            demurrageContext.BuildTableRow.Add(buildTableRow);

            await demurrageContext.SaveChangesAsync();

            var updatedBuildTableRow = await demurrageContext.BuildTableRow.FirstOrDefaultAsync();
            updatedBuildTableRow.FileName = "updated-fileName";

            var result = await buildTableRowRepository.UpdateBuildTableAsync(buildTableRow.Id, buildTableRow, updatedBuildTableRow);

            var buildTableRowAfterUpdate = await demurrageContext.BuildTableRow.Where(n => n.Id == buildTableRow.Id).FirstOrDefaultAsync();

            Assert.NotEqual(0, result);
            Assert.Equal(updatedBuildTableRow.FileName, buildTableRowAfterUpdate.FileName);

        }

        [Fact]
        public async Task DeleteBuildTableAsync_ShouldReturnGreaterThanZero()
        {
            var demurrageContext = DbContextFactory.GetDemurrageContextTest();
            var buildTableRowRepository = new BuildTableRowRepository(demurrageContext);
            var buildTableRow = BuildTableRowFactory.GetBuildTable();

            demurrageContext.BuildTableRow.Add(buildTableRow);

            await demurrageContext.SaveChangesAsync();

            var result = await buildTableRowRepository.DeleteBuildTableAsync(buildTableRow);

            var buildTableRowAfterDelete = await demurrageContext.BuildTableRow.Where(u => u.Id == buildTableRow.Id).FirstOrDefaultAsync();

            Assert.NotEqual(0, result);
            Assert.True(buildTableRowAfterDelete.Deleted);

        }

    }
}

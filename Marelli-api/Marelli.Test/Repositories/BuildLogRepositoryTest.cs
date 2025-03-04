using Marelli.Domain.Entities;
using Marelli.Infra.Repositories;
using Marelli.Test.Utils.Factories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Marelli.Test.Repositories
{
    public class BuildLogRepositoryTest
    {

        [Fact]
        public async Task SaveBuildLog_ShouldReturnBuildLogInstance()
        {
            var demurrageContext = DbContextFactory.GetDemurrageContextTest();
            var buildLogRepository = new BuildLogRepository(demurrageContext);

            var buildLog = BuildLogFactory.GetBuildLog();

            var result = await buildLogRepository.SaveBuildLog(buildLog);

            Assert.NotNull(result);
            Assert.Equal(buildLog.Status, result.Status);
        }

        [Fact]
        public async Task ListBuildLog_ShouldReturnBuildLogList()
        {
            var demurrageContext = DbContextFactory.GetDemurrageContextTest();
            var buildLogRepository = new BuildLogRepository(demurrageContext);
            var buildLog = BuildLogFactory.GetBuildLog();

            demurrageContext.Add(buildLog);
            await demurrageContext.SaveChangesAsync();

            var result = await buildLogRepository.ListBuildLog(buildLog.BuildId);

            Assert.NotEmpty(result);
            Assert.Equal(buildLog.Status, result.First().Status);
        }

        [Fact]
        public async Task GetBuildLog_ShouldReturnBuildLogInstance()
        {
            var demurrageContext = DbContextFactory.GetDemurrageContextTest();
            var buildLogRepository = new BuildLogRepository(demurrageContext);
            var buildLog = BuildLogFactory.GetBuildLog();

            demurrageContext.Add(buildLog);
            await demurrageContext.SaveChangesAsync();

            var result = await buildLogRepository.GetBuildLog(buildLog.Id);

            Assert.IsType<BuildLog>(result);
            Assert.Equal(buildLog.Status, result.Status);
        }


        [Fact]
        public async Task UpdateBuildLog_ShouldReturnGreaterThanZero()
        {
            var demurrageContext = DbContextFactory.GetDemurrageContextTest();
            var buildLogRepository = new BuildLogRepository(demurrageContext);
            var buildLog = BuildLogFactory.GetBuildLog();

            demurrageContext.BuildLog.Add(buildLog);

            await demurrageContext.SaveChangesAsync();

            var updatedBuildLog = await demurrageContext.BuildLog.FirstOrDefaultAsync();
            updatedBuildLog.Status = "updated-status";

            var result = await buildLogRepository.UpdateBuildLog(buildLog.Id, buildLog, updatedBuildLog);

            var buildLogAfterUpdate = await demurrageContext.BuildLog.Where(n => n.Id == buildLog.Id).FirstOrDefaultAsync();

            Assert.NotEqual(0, result);
            Assert.Equal(updatedBuildLog.Status, buildLogAfterUpdate.Status);

        }

        [Fact]
        public async Task DeleteBuildLog_ShouldReturnGreaterThanZero()
        {
            var demurrageContext = DbContextFactory.GetDemurrageContextTest();
            var buildLogRepository = new BuildLogRepository(demurrageContext);
            var buildLog = BuildLogFactory.GetBuildLog();

            demurrageContext.BuildLog.Add(buildLog);

            await demurrageContext.SaveChangesAsync();

            var result = await buildLogRepository.DeleteBuildLog(buildLog);

            var buildLogAfterDelete = await demurrageContext.BuildLog.Where(u => u.Id == buildLog.Id).FirstOrDefaultAsync();

            Assert.NotEqual(0, result);
            Assert.Null(buildLogAfterDelete);

        }

    }
}

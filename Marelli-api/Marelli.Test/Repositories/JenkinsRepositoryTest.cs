using Marelli.Domain.Entities;
using Marelli.Infra.Repositories;
using Marelli.Test.Utils.Factories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Marelli.Test.Repositories
{
    public class JenkinsRepositoryTest
    {

        [Fact]
        public async Task GetAllData_ShouldReturnAllJenkinsData()
        {
            var demurrageContext = DbContextFactory.GetDemurrageContextTest();
            var jenkinsRepository = new JenkinsRepository(demurrageContext);

            var buildingState = JenkinsFactory.GetBuildingState();
            var logAndArtifact = JenkinsFactory.GetLogAndArtifact();
            var fileVerify = JenkinsFactory.GetFileVerify();

            demurrageContext.BuildingState.Add(buildingState);
            demurrageContext.LogAndArtifact.Add(logAndArtifact);
            demurrageContext.FileVerify.Add(fileVerify);

            await demurrageContext.SaveChangesAsync();

            var result = await jenkinsRepository.GetAllData(buildingState.UserId, buildingState.ProjectId);

            Assert.NotNull(result);
            Assert.Equal(buildingState.Date, result.BuildingStates.First().Date);
            Assert.Equal(logAndArtifact.FileOutS3BucketLocation, result.LogAndArtifacts.First().FileOutS3BucketLocation);
            Assert.Equal(fileVerify.Filename, result.FileVerifies.First().Filename);
        }

        [Fact]
        public async Task SaveBuildingState_ShouldReturnBuildingStateInstance()
        {
            var demurrageContext = DbContextFactory.GetDemurrageContextTest();
            var jenkinsRepository = new JenkinsRepository(demurrageContext);

            var buildingState = JenkinsFactory.GetBuildingState();

            var result = await jenkinsRepository.SaveBuildingState(buildingState);

            Assert.NotNull(result);
            Assert.Equal(buildingState.Date, result.Date);
        }

        [Fact]
        public async Task ListBuildingState_ShouldReturnBuildingStateList()
        {
            var demurrageContext = DbContextFactory.GetDemurrageContextTest();
            var jenkinsRepository = new JenkinsRepository(demurrageContext);
            var buildingState = JenkinsFactory.GetBuildingState();

            demurrageContext.Add(buildingState);
            await demurrageContext.SaveChangesAsync();

            var result = await jenkinsRepository.ListBuildingState(buildingState.UserId, buildingState.ProjectId);

            Assert.NotEmpty(result);
            Assert.Equal(buildingState.Date, result.First().Date);
        }

        [Fact]
        public async Task GetBuildingStateById_ShouldReturnBuildingStateInstance()
        {
            var demurrageContext = DbContextFactory.GetDemurrageContextTest();
            var jenkinsRepository = new JenkinsRepository(demurrageContext);
            var buildingState = JenkinsFactory.GetBuildingState();

            demurrageContext.Add(buildingState);
            await demurrageContext.SaveChangesAsync();

            var result = await jenkinsRepository.GetBuildingStateById(buildingState.Id);

            Assert.IsType<BuildingState>(result);
            Assert.Equal(buildingState.Date, result.Date);
        }

        [Fact]
        public async Task GetBuildingStateByBuildId_ShouldReturnBuildingStateInstance()
        {
            var demurrageContext = DbContextFactory.GetDemurrageContextTest();
            var jenkinsRepository = new JenkinsRepository(demurrageContext);
            var buildingState = JenkinsFactory.GetBuildingState();

            demurrageContext.Add(buildingState);
            await demurrageContext.SaveChangesAsync();

            var result = await jenkinsRepository.GetBuildingStateByBuildId(buildingState.BuildId);

            Assert.IsType<BuildingState>(result);
            Assert.Equal(buildingState.Date, result.Date);
        }


        [Fact]
        public async Task UpdateBuildingState_ShouldReturnGreaterThanZero()
        {
            var demurrageContext = DbContextFactory.GetDemurrageContextTest();
            var jenkinsRepository = new JenkinsRepository(demurrageContext);
            var buildingState = JenkinsFactory.GetBuildingState();

            demurrageContext.BuildingState.Add(buildingState);

            await demurrageContext.SaveChangesAsync();

            var updatedBuildingState = await demurrageContext.BuildingState.FirstOrDefaultAsync();
            updatedBuildingState.Starting = "Done";

            var result = await jenkinsRepository.UpdateBuildingState(buildingState.Id, buildingState, updatedBuildingState);

            var buildingStateAfterUpdate = await demurrageContext.BuildingState.Where(n => n.Id == buildingState.Id).FirstOrDefaultAsync();

            Assert.NotNull(result);
            Assert.Equal(updatedBuildingState.Starting, buildingStateAfterUpdate.Starting);

        }

        [Fact]
        public async Task DeleteBuildingState_ShouldReturnGreaterThanZero()
        {
            var demurrageContext = DbContextFactory.GetDemurrageContextTest();
            var jenkinsRepository = new JenkinsRepository(demurrageContext);
            var buildingState = JenkinsFactory.GetBuildingState();

            demurrageContext.BuildingState.Add(buildingState);

            await demurrageContext.SaveChangesAsync();

            var result = await jenkinsRepository.DeleteBuildingState(buildingState);

            var buildingStateAfterDelete = await demurrageContext.BuildingState.Where(u => u.Id == buildingState.Id).FirstOrDefaultAsync();

            Assert.NotEqual(0, result);
            Assert.Null(buildingStateAfterDelete);

        }

        //Log And Artifact

        [Fact]
        public async Task SaveLogAndArtifact_ShouldReturnGreaterThanZero()
        {
            var demurrageContext = DbContextFactory.GetDemurrageContextTest();
            var jenkinsRepository = new JenkinsRepository(demurrageContext);

            var logAndArtifact = JenkinsFactory.GetLogAndArtifact();

            var result = await jenkinsRepository.SaveLogAndArtifact(logAndArtifact);

            Assert.NotEqual(0, result);
        }

        [Fact]
        public async Task ListLogAndArtifact_ShouldReturnLogAndArtifactList()
        {
            var demurrageContext = DbContextFactory.GetDemurrageContextTest();
            var jenkinsRepository = new JenkinsRepository(demurrageContext);
            var logAndArtifact = JenkinsFactory.GetLogAndArtifact();

            demurrageContext.Add(logAndArtifact);
            await demurrageContext.SaveChangesAsync();

            var result = await jenkinsRepository.ListLogAndArtifact(logAndArtifact.UserId, logAndArtifact.ProjectId);

            Assert.NotEmpty(result);
            Assert.Equal(logAndArtifact.FileLogS3BucketLocation, result.First().FileLogS3BucketLocation);
        }

        [Fact]
        public async Task GetLogAndArtifactById_ShouldReturnLogAndArtifactInstance()
        {
            var demurrageContext = DbContextFactory.GetDemurrageContextTest();
            var jenkinsRepository = new JenkinsRepository(demurrageContext);
            var logAndArtifact = JenkinsFactory.GetLogAndArtifact();

            demurrageContext.Add(logAndArtifact);
            await demurrageContext.SaveChangesAsync();

            var result = await jenkinsRepository.GetLogAndArtifactById(logAndArtifact.Id);

            Assert.IsType<LogAndArtifact>(result);
            Assert.Equal(logAndArtifact.FileLogS3BucketLocation, result.FileLogS3BucketLocation);
        }

        [Fact]
        public async Task GetLogAndArtifactByBuildId_ShouldReturnLogAndArtifactInstance()
        {
            var demurrageContext = DbContextFactory.GetDemurrageContextTest();
            var jenkinsRepository = new JenkinsRepository(demurrageContext);
            var logAndArtifact = JenkinsFactory.GetLogAndArtifact();

            demurrageContext.Add(logAndArtifact);
            await demurrageContext.SaveChangesAsync();

            var result = await jenkinsRepository.GetLogAndArtifactByBuildId(logAndArtifact.BuildId);

            Assert.IsType<LogAndArtifact>(result);
            Assert.Equal(logAndArtifact.FileLogS3BucketLocation, result.FileLogS3BucketLocation);
        }


        [Fact]
        public async Task UpdateLogAndArtifact_ShouldReturnGreaterThanZero()
        {
            var demurrageContext = DbContextFactory.GetDemurrageContextTest();
            var jenkinsRepository = new JenkinsRepository(demurrageContext);
            var logAndArtifact = JenkinsFactory.GetLogAndArtifact();

            demurrageContext.LogAndArtifact.Add(logAndArtifact);

            await demurrageContext.SaveChangesAsync();

            var updatedLogAndArtifact = await demurrageContext.LogAndArtifact.FirstOrDefaultAsync();
            updatedLogAndArtifact.FileLogS3BucketLocation = "updated-log-location";

            var result = await jenkinsRepository.UpdateLogAndArtifact(logAndArtifact.Id, logAndArtifact, updatedLogAndArtifact);

            var logAndArtifactAfterUpdate = await demurrageContext.LogAndArtifact.Where(n => n.Id == logAndArtifact.Id).FirstOrDefaultAsync();
            Assert.Equal(updatedLogAndArtifact.FileLogS3BucketLocation, logAndArtifactAfterUpdate.FileLogS3BucketLocation);

        }

        [Fact]
        public async Task DeleteLogAndArtifact_ShouldReturnGreaterThanZero()
        {
            var demurrageContext = DbContextFactory.GetDemurrageContextTest();
            var jenkinsRepository = new JenkinsRepository(demurrageContext);
            var logAndArtifact = JenkinsFactory.GetLogAndArtifact();

            demurrageContext.LogAndArtifact.Add(logAndArtifact);

            await demurrageContext.SaveChangesAsync();

            var result = await jenkinsRepository.DeleteLogAndArtifact(logAndArtifact);

            var logAndArtifactAfterDelete = await demurrageContext.LogAndArtifact.Where(u => u.Id == logAndArtifact.Id).FirstOrDefaultAsync();

            Assert.NotEqual(0, result);
            Assert.Null(logAndArtifactAfterDelete);

        }

        //File Verify

        [Fact]
        public async Task SaveFileVerify_ShouldReturnGreaterThanZero()
        {
            var demurrageContext = DbContextFactory.GetDemurrageContextTest();
            var jenkinsRepository = new JenkinsRepository(demurrageContext);

            var fileVerify = JenkinsFactory.GetFileVerify();

            var result = await jenkinsRepository.SaveFileVerify(fileVerify);

            Assert.NotEqual(0, result);
        }

        [Fact]
        public async Task ListFileVerify_ShouldReturnFileVerifyList()
        {
            var demurrageContext = DbContextFactory.GetDemurrageContextTest();
            var jenkinsRepository = new JenkinsRepository(demurrageContext);
            var fileVerify = JenkinsFactory.GetFileVerify();

            demurrageContext.Add(fileVerify);
            await demurrageContext.SaveChangesAsync();

            var result = await jenkinsRepository.ListFileVerify(fileVerify.UserId, fileVerify.ProjectId);

            Assert.NotEmpty(result);
            Assert.Equal(fileVerify.Filename, result.First().Filename);
        }

        [Fact]
        public async Task GetFileVerifyById_ShouldReturnFileVerifyInstance()
        {
            var demurrageContext = DbContextFactory.GetDemurrageContextTest();
            var jenkinsRepository = new JenkinsRepository(demurrageContext);
            var fileVerify = JenkinsFactory.GetFileVerify();

            demurrageContext.Add(fileVerify);
            await demurrageContext.SaveChangesAsync();

            var result = await jenkinsRepository.GetFileVerifyById(fileVerify.Id);

            Assert.IsType<FileVerify>(result);
            Assert.Equal(fileVerify.Filename, result.Filename);
        }

        [Fact]
        public async Task UpdateFileVerify_ShouldReturnGreaterThanZero()
        {
            var demurrageContext = DbContextFactory.GetDemurrageContextTest();
            var jenkinsRepository = new JenkinsRepository(demurrageContext);
            var fileVerify = JenkinsFactory.GetFileVerify();

            demurrageContext.FileVerify.Add(fileVerify);

            await demurrageContext.SaveChangesAsync();

            var updatedFileVerify = await demurrageContext.FileVerify.FirstOrDefaultAsync();
            updatedFileVerify.Filename = "updated-log-location";

            var result = await jenkinsRepository.UpdateFileVerify(fileVerify.Id, fileVerify, updatedFileVerify);

            var fileVerifyAfterUpdate = await demurrageContext.FileVerify.Where(n => n.Id == fileVerify.Id).FirstOrDefaultAsync();
            Assert.Equal(updatedFileVerify.Filename, fileVerifyAfterUpdate.Filename);

        }

        [Fact]
        public async Task DeleteFileVerify_ShouldReturnGreaterThanZero()
        {
            var demurrageContext = DbContextFactory.GetDemurrageContextTest();
            var jenkinsRepository = new JenkinsRepository(demurrageContext);
            var fileVerify = JenkinsFactory.GetFileVerify();

            demurrageContext.FileVerify.Add(fileVerify);

            await demurrageContext.SaveChangesAsync();

            var result = await jenkinsRepository.DeleteFileVerify(fileVerify);

            var fileVerifyAfterDelete = await demurrageContext.FileVerify.Where(u => u.Id == fileVerify.Id).FirstOrDefaultAsync();

            Assert.NotEqual(0, result);
            Assert.Null(fileVerifyAfterDelete);

        }

    }
}

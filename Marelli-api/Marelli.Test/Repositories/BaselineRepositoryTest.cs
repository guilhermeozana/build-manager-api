using Marelli.Domain.Entities;
using Marelli.Infra.Repositories;
using Marelli.Test.Utils.Factories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Marelli.Test.Repositories
{
    public class BaselineRepositoryTest
    {

        [Fact]
        public async Task SaveBaseline_ShouldReturnBaselineInstance()
        {
            var demurrageContext = DbContextFactory.GetDemurrageContextTest();
            var baselineRepository = new BaselineRepository(demurrageContext);

            var baseline = BaselineFactory.GetBaseline();

            var result = await baselineRepository.SaveBaseline(baseline);

            Assert.NotNull(result);
            Assert.Equal(baseline.FileName, result.FileName);
        }

        [Fact]
        public async Task ListBaseline_ShouldReturnBaselineList()
        {
            var demurrageContext = DbContextFactory.GetDemurrageContextTest();
            var baselineRepository = new BaselineRepository(demurrageContext);
            var baseline = BaselineFactory.GetBaseline();

            demurrageContext.Add(baseline);
            await demurrageContext.SaveChangesAsync();

            var result = await baselineRepository.ListBaseline(baseline.ProjectId);

            Assert.NotEmpty(result);
            Assert.Equal(baseline.FileName, result.First().FileName);
        }

        [Fact]
        public async Task GetBaseline_ShouldReturnBaselineInstance()
        {
            var demurrageContext = DbContextFactory.GetDemurrageContextTest();
            var baselineRepository = new BaselineRepository(demurrageContext);
            var baseline = BaselineFactory.GetBaseline();

            demurrageContext.Add(baseline);
            await demurrageContext.SaveChangesAsync();

            var result = await baselineRepository.GetBaseline(baseline.Id);

            Assert.IsType<Baseline>(result);
            Assert.Equal(baseline.FileName, result.FileName);
        }

        [Fact]
        public async Task GetBaselineByProject_ShouldReturnBaselineInstance()
        {
            var demurrageContext = DbContextFactory.GetDemurrageContextTest();
            var baselineRepository = new BaselineRepository(demurrageContext);
            var baseline = BaselineFactory.GetBaseline();
            var project = ProjectFactory.GetProject();

            demurrageContext.Add(project);
            await demurrageContext.SaveChangesAsync();

            baseline.ProjectId = project.Id;
            demurrageContext.Add(baseline);
            await demurrageContext.SaveChangesAsync();

            var result = await baselineRepository.GetBaselineByProject(baseline.Id);

            Assert.IsType<Baseline>(result);
            Assert.Equal(baseline.FileName, result.FileName);
        }


        [Fact]
        public async Task UpdateBaseline_ShouldReturnGreaterThanZero()
        {
            var demurrageContext = DbContextFactory.GetDemurrageContextTest();
            var baselineRepository = new BaselineRepository(demurrageContext);
            var baseline = BaselineFactory.GetBaseline();

            demurrageContext.Baseline.Add(baseline);

            await demurrageContext.SaveChangesAsync();

            var updatedBaseline = await demurrageContext.Baseline.FirstOrDefaultAsync();
            updatedBaseline.FileName = "updated-status";

            var result = await baselineRepository.UpdateBaseline(baseline.Id, baseline, updatedBaseline);

            var baselineAfterUpdate = await demurrageContext.Baseline.Where(n => n.Id == baseline.Id).FirstOrDefaultAsync();

            Assert.NotEqual(0, result);
            Assert.Equal(updatedBaseline.FileName, baselineAfterUpdate.FileName);

        }

        [Fact]
        public async Task DeleteBaseline_ShouldReturnGreaterThanZero()
        {
            var demurrageContext = DbContextFactory.GetDemurrageContextTest();
            var baselineRepository = new BaselineRepository(demurrageContext);
            var baseline = BaselineFactory.GetBaseline();
            var unselectedBaseline = BaselineFactory.GetBaseline();
            unselectedBaseline.Selected = false;

            demurrageContext.Baseline.Add(baseline);
            await demurrageContext.SaveChangesAsync();

            demurrageContext.Baseline.Add(unselectedBaseline);
            await demurrageContext.SaveChangesAsync();

            var result = await baselineRepository.DeleteBaseline(baseline);

            var baselineAfterDelete = await demurrageContext.Baseline.Where(u => u.Id == baseline.Id).FirstOrDefaultAsync();

            Assert.NotEqual(0, result);
            Assert.Null(baselineAfterDelete);

        }

    }
}

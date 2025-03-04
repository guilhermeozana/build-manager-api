using Marelli.Business.Exceptions;
using Marelli.Business.IServices;
using Marelli.Business.Services;
using Marelli.Domain.Entities;
using Marelli.Infra.IRepositories;
using Marelli.Test.Utils.Factories;
using Microsoft.AspNetCore.SignalR;
using Moq;
using Xunit;

namespace Marelli.Test.Services
{
    public class BaselineServiceTest
    {
        private readonly Mock<IBaselineRepository> _baselineRepositoryMock;
        private readonly Mock<IProjectService> _projectServiceMock;
        private readonly BaselineService _baselineService;

        public BaselineServiceTest()
        {
            _baselineRepositoryMock = new Mock<IBaselineRepository>();
            _projectServiceMock = new Mock<IProjectService>();

            _baselineService = new BaselineService(_baselineRepositoryMock.Object, _projectServiceMock.Object);
        }


        [Fact]
        public async Task SaveBaseline_ShouldReturnBaselineInstance()
        {
            var baselineExpected = BaselineFactory.GetBaseline();

            var mockClientProxy = new Mock<IClientProxy>();

            _baselineRepositoryMock.Setup(b => b.ListBaseline(It.IsAny<int>())).ReturnsAsync(new List<Baseline>() { baselineExpected });
            _baselineRepositoryMock.Setup(b => b.SaveBaseline(It.IsAny<Baseline>())).ReturnsAsync(baselineExpected);

            var res = await _baselineService.SaveBaseline(baselineExpected);

            Assert.NotNull(res);
            Assert.Equal(baselineExpected, res);
        }

        [Fact]
        public async Task ListBaseline_ShouldReturnBaselineList()
        {
            var baselineList = new List<Baseline>() { BaselineFactory.GetBaseline() };

            _baselineRepositoryMock.Setup(b => b.ListBaseline(It.IsAny<int>())).ReturnsAsync(baselineList);

            var res = await _baselineService.ListBaseline(1);

            Assert.NotEmpty(res);
            Assert.Equal(baselineList, res);
        }

        [Fact]
        public async Task GetBaseline_ShouldReturnBaselineInstance()
        {
            var baselineExpected = BaselineFactory.GetBaseline();

            _baselineRepositoryMock.Setup(b => b.GetBaseline(It.IsAny<int>())).ReturnsAsync(baselineExpected);

            var res = await _baselineService.GetBaseline(1);

            Assert.NotNull(res);
            Assert.Equal(baselineExpected, res);
        }

        [Fact]
        public async Task GetBaseline_ShouldThrowNotFoundException()
        {
            _baselineRepositoryMock.Setup(b => b.GetBaseline(It.IsAny<int>())).ReturnsAsync((Baseline)null);

            await Assert.ThrowsAsync<NotFoundException>(async () => await _baselineService.GetBaseline(1));
        }

        [Fact]
        public async Task GetBaselineByProject_ShouldReturnBaselineInstance()
        {
            var baselineExpected = BaselineFactory.GetBaseline();

            _baselineRepositoryMock.Setup(b => b.GetBaselineByProject(It.IsAny<int>())).ReturnsAsync(baselineExpected);

            var res = await _baselineService.GetBaselineByProject(1);

            Assert.NotNull(res);
            Assert.Equal(baselineExpected, res);
        }

        [Fact]
        public async Task UpdateBaseline_ShouldReturnGreaterThanZero()
        {
            var baseline = BaselineFactory.GetBaseline();
            var mockClientProxy = new Mock<IClientProxy>();

            _baselineRepositoryMock.Setup(b => b.GetBaseline(It.IsAny<int>())).ReturnsAsync(baseline);
            _baselineRepositoryMock.Setup(b => b.UpdateBaseline(It.IsAny<int>(), It.IsAny<Baseline>(), It.IsAny<Baseline>())).ReturnsAsync(1);
            _baselineRepositoryMock.Setup(b => b.ListBaseline(It.IsAny<int>())).ReturnsAsync(new List<Baseline>() { baseline });


            var res = await _baselineService.UpdateBaseline(1, baseline);

            Assert.Equal(1, res);
        }

        [Fact]
        public async Task UpdateBaseline_ShouldThrowNotFoundException()
        {
            _baselineRepositoryMock.Setup(b => b.GetBaseline(It.IsAny<int>())).ReturnsAsync((Baseline)null);

            await Assert.ThrowsAsync<NotFoundException>(async () => await _baselineService.UpdateBaseline(1, BaselineFactory.GetBaseline()));
        }

        [Fact]
        public async Task DeleteBaseline_ShouldReturnGreaterThanZero()
        {
            _baselineRepositoryMock.Setup(b => b.GetBaseline(It.IsAny<int>())).ReturnsAsync(BaselineFactory.GetBaseline());
            _baselineRepositoryMock.Setup(b => b.DeleteBaseline(It.IsAny<Baseline>())).ReturnsAsync(1);

            var res = await _baselineService.DeleteBaseline(1);

            Assert.Equal(1, res);
        }

        [Fact]
        public async Task DeleteBaseline_ShouldThrowNotFoundException()
        {
            _baselineRepositoryMock.Setup(b => b.GetBaseline(It.IsAny<int>())).ReturnsAsync((Baseline)null);

            await Assert.ThrowsAsync<NotFoundException>(async () => await _baselineService.DeleteBaseline(1));
        }

    }
}
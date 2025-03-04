using Marelli.Business.Exceptions;
using Marelli.Business.Services;
using Marelli.Domain.Entities;
using Marelli.Infra.IRepositories;
using Marelli.Test.Utils.Factories;
using Moq;
using Xunit;

namespace Marelli.Test.Services
{
    public class NewsServiceTest
    {
        private readonly Mock<INewsRepository> _newsRepositoryMock;
        private readonly NewsService _newsService;

        public NewsServiceTest()
        {
            _newsRepositoryMock = new Mock<INewsRepository>();

            _newsService = new NewsService(_newsRepositoryMock.Object);
        }


        [Fact]
        public async Task SaveNews_ShouldReturnGreaterThanZero()
        {
            _newsRepositoryMock.Setup(n => n.SaveNews(It.IsAny<News>())).ReturnsAsync(1);

            var res = await _newsService.SaveNews(NewsFactory.GetNews());

            Assert.Equal(1, res);
        }

        [Fact]
        public async Task ListNews_ShouldReturnNewsList()
        {
            var newsList = new List<News>() { NewsFactory.GetNews() };

            _newsRepositoryMock.Setup(n => n.ListNews()).ReturnsAsync(newsList);

            var res = await _newsService.ListNews();

            Assert.NotEmpty(res);
            Assert.Equal(newsList, res);
        }

        [Fact]
        public async Task GetNewsById_ShouldReturnNewsInstance()
        {
            var expectedNews = NewsFactory.GetNews();

            _newsRepositoryMock.Setup(n => n.GetNewsById(It.IsAny<int>())).ReturnsAsync(expectedNews);

            var res = await _newsService.GetNewsById(1);

            Assert.NotNull(res);
            Assert.Equal(expectedNews, res);
        }

        [Fact]
        public async Task GetNewsById_ShouldThrowNotFoundException()
        {
            _newsRepositoryMock.Setup(n => n.GetNewsById(It.IsAny<int>())).ReturnsAsync((News)null);

            await Assert.ThrowsAsync<NotFoundException>(async () => await _newsService.GetNewsById(1));
        }

        [Fact]
        public async Task UpdateNews_ShouldReturnGreaterThanZero()
        {
            _newsRepositoryMock.Setup(n => n.GetNewsById(It.IsAny<int>())).ReturnsAsync(NewsFactory.GetNews());
            _newsRepositoryMock.Setup(n => n.UpdateNews(It.IsAny<int>(), It.IsAny<News>(), It.IsAny<News>())).ReturnsAsync(1);

            var res = await _newsService.UpdateNews(1, NewsFactory.GetNews());

            Assert.Equal(1, res);
        }

        [Fact]
        public async Task UpdateNews_ShouldThrowNotFoundException()
        {
            _newsRepositoryMock.Setup(n => n.GetNewsById(It.IsAny<int>())).ReturnsAsync((News)null);

            await Assert.ThrowsAsync<NotFoundException>(async () => await _newsService.UpdateNews(1, NewsFactory.GetNews()));
        }

        [Fact]
        public async Task DeleteNews_ShouldReturnGreaterThanZero()
        {
            _newsRepositoryMock.Setup(n => n.GetNewsById(It.IsAny<int>())).ReturnsAsync(NewsFactory.GetNews());
            _newsRepositoryMock.Setup(n => n.DeleteNews(It.IsAny<News>())).ReturnsAsync(1);

            var res = await _newsService.DeleteNews(1);

            Assert.Equal(1, res);
        }

        [Fact]
        public async Task DeleteNews_ShouldThrowNotFoundException()
        {
            _newsRepositoryMock.Setup(n => n.GetNewsById(It.IsAny<int>())).ReturnsAsync((News)null);

            await Assert.ThrowsAsync<NotFoundException>(async () => await _newsService.DeleteNews(1));
        }

    }
}
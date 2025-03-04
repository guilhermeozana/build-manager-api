using Marelli.Api.Controllers;
using Marelli.Business.IServices;
using Marelli.Domain.Entities;
using Marelli.Test.Utils.Factories;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Marelli.Test.Controllers
{
    public class NewsControllerTest
    {
        private readonly Mock<INewsService> _newsServiceMock;
        private readonly NewsController _newsController;

        public NewsControllerTest()
        {
            _newsServiceMock = new Mock<INewsService>();

            _newsController = new NewsController(_newsServiceMock.Object);
        }

        [Fact]
        public async Task SaveNews_ShouldReturnOkResult()
        {
            var news = NewsFactory.GetNews();

            _newsServiceMock.Setup(n => n.SaveNews(news)).ReturnsAsync(1);

            var result = await _newsController.SaveNews(news);

            var okResult = Assert.IsType<OkObjectResult>(result);

            Assert.NotNull(okResult.Value);

            var okResultValue = Assert.IsType<int>(okResult.Value);

            Assert.NotEqual(0, okResultValue);
        }

        [Fact]
        public async Task ListNews_ShouldReturnOkResult()
        {
            var news = NewsFactory.GetNews();

            _newsServiceMock.Setup(n => n.ListNews()).ReturnsAsync(new List<News> { news });

            var result = await _newsController.ListNews();

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);

            var okResultValue = Assert.IsType<List<News>>(okResult.Value);
            Assert.Equal(news.Description, okResultValue.First().Description);
        }

        [Fact]
        public async Task GetNews_ShouldReturnOkResult()
        {
            var news = NewsFactory.GetNews();

            _newsServiceMock.Setup(n => n.GetNewsById(It.IsAny<int>())).ReturnsAsync(news);

            var result = await _newsController.GetNews(1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);

            var okResultValue = Assert.IsType<News>(okResult.Value);
            Assert.Equal(news.Description, okResultValue.Description);
        }

        [Fact]
        public async Task UpdateNews_ShouldReturnReturnOkResult()
        {
            var news = NewsFactory.GetNews();

            _newsServiceMock.Setup(n => n.UpdateNews(It.IsAny<int>(), It.IsAny<News>())).ReturnsAsync(1);

            var result = await _newsController.UpdateNews(news.Id, news);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);

            var okResultValue = Assert.IsType<int>(okResult.Value);
            Assert.NotEqual(0, okResultValue);
        }

        [Fact]
        public async Task DeleteNews_ShouldReturnReturnOkResult()
        {
            _newsServiceMock.Setup(n => n.DeleteNews(It.IsAny<int>())).ReturnsAsync(1);

            var result = await _newsController.DeleteNews(1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);

            var okResultValue = Assert.IsType<int>(okResult.Value);
            Assert.NotEqual(0, okResultValue);
        }

    }
}

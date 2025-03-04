using Marelli.Domain.Entities;
using Marelli.Infra.Repositories;
using Marelli.Test.Utils.Factories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Marelli.Test.Repositories
{
    public class NewsRepositoryTest
    {

        [Fact]
        public async Task SaveNews_ShouldReturnGreaterThanZero()
        {
            var demurrageContext = DbContextFactory.GetDemurrageContextTest();
            var newsRepository = new NewsRepository(demurrageContext);

            var news = NewsFactory.GetNews();

            var result = await newsRepository.SaveNews(news);

            Assert.NotEqual(0, result);
        }

        [Fact]
        public async Task ListNews_ShouldReturnNewsList()
        {
            var demurrageContext = DbContextFactory.GetDemurrageContextTest();
            var newsRepository = new NewsRepository(demurrageContext);
            var news = NewsFactory.GetNews();

            demurrageContext.Add(news);
            await demurrageContext.SaveChangesAsync();

            var result = await newsRepository.ListNews();

            Assert.NotEmpty(result);
            Assert.Equal(news.Description, result.First().Description);
        }

        [Fact]
        public async Task GetNewsById_ShouldReturnNewsInstance()
        {
            var demurrageContext = DbContextFactory.GetDemurrageContextTest();
            var newsRepository = new NewsRepository(demurrageContext);
            var news = NewsFactory.GetNews();

            demurrageContext.Add(news);
            await demurrageContext.SaveChangesAsync();

            var result = await newsRepository.GetNewsById(news.Id);

            Assert.IsType<News>(result);
            Assert.Equal(news.Description, result.Description);
        }


        [Fact]
        public async Task UpdateNews_ShouldReturnGreaterThanZero()
        {
            var demurrageContext = DbContextFactory.GetDemurrageContextTest();
            var newsRepository = new NewsRepository(demurrageContext);
            var news = NewsFactory.GetNews();

            demurrageContext.News.Add(news);

            await demurrageContext.SaveChangesAsync();

            var updatedNews = await demurrageContext.News.FirstOrDefaultAsync();
            updatedNews.Description = "updated-description";

            var result = await newsRepository.UpdateNews(news.Id, news, updatedNews);

            var newsAfterUpdate = await demurrageContext.News.Where(n => n.Id == news.Id).FirstOrDefaultAsync();

            Assert.NotEqual(0, result);
            Assert.Equal(updatedNews.Description, newsAfterUpdate.Description);

        }

        [Fact]
        public async Task DeleteNews_ShouldReturnGreaterThanZero()
        {
            var demurrageContext = DbContextFactory.GetDemurrageContextTest();
            var newsRepository = new NewsRepository(demurrageContext);
            var news = NewsFactory.GetNews();

            demurrageContext.News.Add(news);

            await demurrageContext.SaveChangesAsync();

            var result = await newsRepository.DeleteNews(news);

            var newsAfterDelete = await demurrageContext.News.Where(u => u.Id == news.Id).FirstOrDefaultAsync();

            Assert.NotEqual(0, result);
            Assert.Null(newsAfterDelete);

        }

    }
}

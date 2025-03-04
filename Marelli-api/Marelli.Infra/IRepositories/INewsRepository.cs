using Marelli.Domain.Entities;

namespace Marelli.Infra.IRepositories
{
    public interface INewsRepository
    {
        public Task<int> SaveNews(News entity);

        public Task<List<News>> ListNews();

        public Task<News> GetNewsById(int id);

        public Task<int> UpdateNews(int id, News currentNews, News updatedNews);

        public Task<int> DeleteNews(News news);
    }
}

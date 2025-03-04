using Marelli.Domain.Entities;

namespace Marelli.Business.IServices
{
    public interface INewsService
    {
        public Task<int> SaveNews(News request);

        public Task<List<News>> ListNews();

        public Task<News> GetNewsById(int id);

        public Task<int> UpdateNews(int id, News request);
        public Task<int> DeleteNews(int id);
    }
}

using Marelli.Business.Exceptions;
using Marelli.Business.IServices;
using Marelli.Domain.Entities;
using Marelli.Infra.IRepositories;

namespace Marelli.Business.Services
{
    public class NewsService : INewsService
    {
        private readonly INewsRepository _NewsRepository;

        public NewsService(INewsRepository NewsRepository)
        {
            _NewsRepository = NewsRepository;
        }

        public async Task<int> SaveNews(News request)
        {
            return await _NewsRepository.SaveNews(request);
        }

        public async Task<List<News>> ListNews()
        {
            return await _NewsRepository.ListNews();
        }

        public async Task<News> GetNewsById(int id)
        {
            var news = await _NewsRepository.GetNewsById(id);

            if (news == null)
            {
                throw new NotFoundException($"News not found with ID: {id}.");
            }

            return news;
        }

        public async Task<int> UpdateNews(int id, News request)
        {
            var currentNews = await GetNewsById(id);

            return await _NewsRepository.UpdateNews(id, currentNews, request);
        }

        public async Task<int> DeleteNews(int id)
        {
            var news = await GetNewsById(id);

            return await _NewsRepository.DeleteNews(news);
        }
    }
}

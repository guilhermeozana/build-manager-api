using Marelli.Domain.Entities;
using Marelli.Infra.Context;
using Marelli.Infra.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace Marelli.Infra.Repositories;

public class NewsRepository : INewsRepository
{
    private readonly DemurrageContext _context;

    public NewsRepository(DemurrageContext context)
    {
        _context = context;
    }

    public async Task<int> SaveNews(News entity)
    {
        _context.News.Add(entity);
        return await _context.SaveChangesAsync();
    }

    public async Task<List<News>> ListNews()
    {
        return await _context.News.ToListAsync();
    }

    public async Task<News> GetNewsById(int id)
    {
        return await _context.News
                    .Where(n => n.Id == id)
                    .FirstOrDefaultAsync();
    }

    public async Task<int> UpdateNews(int id, News currentNews, News updatedNews)
    {
        currentNews.ImageUrl = updatedNews.ImageUrl;
        currentNews.Description = updatedNews.Description;

        _context.News.Entry(currentNews).State = EntityState.Modified;
        return await _context.SaveChangesAsync();
    }

    public async Task<int> DeleteNews(News news)
    {
        _context.Remove(news);

        return await _context.SaveChangesAsync();

    }

}

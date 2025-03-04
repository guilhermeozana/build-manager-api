using Marelli.Business.IServices;
using Marelli.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Marelli.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NewsController : Controller
    {
        private readonly INewsService _newsService;

        public NewsController(INewsService newsService)
        {
            _newsService = newsService;
        }

        [HttpPost("Save")]
        public async Task<IActionResult> SaveNews([FromBody] News req)
        {
            var news = await _newsService.SaveNews(req);

            return Ok(news);
        }

        [HttpGet("List")]
        public async Task<IActionResult> ListNews()
        {
            var news = await _newsService.ListNews();

            return Ok(news);
        }

        [HttpGet("Get/{id}")]
        public async Task<IActionResult> GetNews(int id)
        {
            var news = await _newsService.GetNewsById(id);

            return Ok(news);
        }

        [HttpPut("Update/{id}")]
        public async Task<IActionResult> UpdateNews(int id, [FromBody] News req)
        {
            var fileVerify = await _newsService.UpdateNews(id, req);

            return Ok(fileVerify);
        }

        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> DeleteNews(int id)
        {
            var news = await _newsService.DeleteNews(id);

            return Ok(news);
        }
    }
}

using Marelli.Domain.Entities;
using Marelli.Infra.Context;
using Marelli.Test.Integration.Configuration;
using Marelli.Test.Utils.Factories;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Json;
using Testcontainers.PostgreSql;
using Xunit;


namespace Marelli.Test.Integration
{
    [Collection("Integration collection")]
    public class NewsIntegrationTest
    {
        private readonly IntegrationSetupFixture _fixture;
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _httpClient;
        private readonly PostgreSqlContainer _postgreSqlContainer;
        public NewsIntegrationTest(IntegrationSetupFixture fixture)
        {
            _fixture = fixture;
            _factory = fixture.Factory;
            _postgreSqlContainer = fixture.Container;
            _httpClient = fixture.HttpClient;
        }

        [Fact]
        public async Task SaveNews_ShouldReturnGreaterThanZero()
        {
            //Arrange
            var expectedNews = NewsFactory.GetNews();
            expectedNews.Description = "save news description";

            //Act
            var response = await _httpClient.PostAsJsonAsync("/api/News/Save", expectedNews);

            //Assert
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            var saved = JsonConvert.DeserializeObject<int>(responseContent);

            Assert.NotEqual(0, saved);

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();
                var savedNews = await context.News.FirstOrDefaultAsync(n => n.Description.Equals(expectedNews.Description));
                Assert.NotNull(savedNews);
                Assert.Equal(expectedNews.ImageUrl, savedNews.ImageUrl);
            }
        }

        [Fact]
        public async Task ListNews_ShouldReturnNewsList()
        {
            //Arrange
            var expectedNews = NewsFactory.GetNews();

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();
                var savedNews = context.News.Add(expectedNews);
                await context.SaveChangesAsync();
            }

            //Act
            var response = await _httpClient.GetAsync("/api/News/List");

            //Assert
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();

            var newsList = JsonConvert.DeserializeObject<List<News>>(responseContent);
            Assert.NotEmpty(newsList);

            var newsByDescription = newsList.Where(n => n.Description.Equals(expectedNews.Description)).FirstOrDefault();
            Assert.NotNull(newsByDescription);

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();
                var savedNews = await context.News.FirstOrDefaultAsync(n => n.Description.Equals(expectedNews.Description));
                Assert.NotNull(savedNews);
                Assert.Equal(expectedNews.ImageUrl, savedNews.ImageUrl);
            }
        }

        [Fact]
        public async Task GetNews_ShouldReturnNewsInstance()
        {
            //Arrange
            var expectedNews = NewsFactory.GetNews();
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();
                context.News.Add(expectedNews);
                await context.SaveChangesAsync();
            }

            //Act
            var response = await _httpClient.GetAsync($"/api/News/Get/{expectedNews.Id}");

            //Assert
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            var news = JsonConvert.DeserializeObject<News>(responseContent);

            Assert.NotNull(news);
            Assert.Equal(expectedNews.Description, news.Description);

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();
                var savedNews = await context.News.FirstOrDefaultAsync(n => n.Description.Equals(expectedNews.Description));
                Assert.NotNull(savedNews);
                Assert.Equal(expectedNews.ImageUrl, savedNews.ImageUrl);
            }
        }

        [Fact]
        public async Task GetNews_ShouldReturnNotFoundResult()
        {
            //Arrange
            var nonExistentId = -1;

            //Act
            var response = await _httpClient.GetAsync($"/api/News/Get/{nonExistentId}");

            //Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.Contains("not found", responseContent);
        }

        [Fact]
        public async Task UpdateNews_ShouldReturnGreaterThanZero()
        {
            //Arrange
            var currentNews = NewsFactory.GetNews();
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();
                context.News.Add(currentNews);
                await context.SaveChangesAsync();
            }

            var newsRequest = NewsFactory.GetNews();
            newsRequest.Description = "updated news description";

            //Act
            var response = await _httpClient.PutAsJsonAsync($"/api/News/Update/{currentNews.Id}", newsRequest);

            //Assert
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            var updated = JsonConvert.DeserializeObject<int>(responseContent);

            Assert.NotEqual(0, updated);

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();
                var updatedNews = await context.News.FirstOrDefaultAsync(n => n.Id.Equals(currentNews.Id));
                Assert.NotNull(updatedNews);
                Assert.Equal(newsRequest.Description, updatedNews.Description);
                Assert.NotEqual(currentNews.Description, updatedNews.Description);
            }
        }

        [Fact]
        public async Task UpdateNews_ShouldReturnNotFoundResult()
        {
            //Arrange
            var nonExistentId = -1;

            //Act
            var response = await _httpClient.PutAsJsonAsync($"/api/News/Update/{nonExistentId}", NewsFactory.GetNews());

            //Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.Contains("not found", responseContent);
        }

        [Fact]
        public async Task DeleteNews_ShouldReturnGreaterThanZero()
        {
            //Arrange
            var news = NewsFactory.GetNews();
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();
                context.News.Add(news);
                await context.SaveChangesAsync();
            }

            //Act
            var response = await _httpClient.DeleteAsync($"/api/News/Delete/{news.Id}");

            //Assert
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            var updated = JsonConvert.DeserializeObject<int>(responseContent);

            Assert.NotEqual(0, updated);

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();
                var savedNews = await context.News.FirstOrDefaultAsync(n => n.Id.Equals(news.Id));
                Assert.Null(savedNews);
            }
        }

        [Fact]
        public async Task DeleteNews_ShouldReturnNotFoundResult()
        {
            //Arrange
            var nonExistentId = -1;

            //Act
            var response = await _httpClient.DeleteAsync($"/api/News/Delete/{nonExistentId}");

            //Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.Contains("not found", responseContent);
        }
    }


}

using Marelli.Business.Factories;
using Xunit;

namespace Marelli.Business.Tests.Factories
{
    public class CustomHttpClientFactoryTests
    {
        [Fact]
        public void GetHttpClient_ShouldReturnHttpClient()
        {
            // Arrange
            var factory = new CustomHttpClientFactory();

            // Act
            var httpClient = factory.GetHttpClient();

            // Assert
            Assert.NotNull(httpClient);
            Assert.IsType<HttpClient>(httpClient);
        }
    }
}

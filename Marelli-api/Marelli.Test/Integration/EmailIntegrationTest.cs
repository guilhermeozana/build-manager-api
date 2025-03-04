using MailKit.Security;
using Marelli.Test.Integration.Configuration;
using Microsoft.AspNetCore.Mvc.Testing;
using MimeKit;
using Moq;
using Xunit;


namespace Marelli.Test.Integration
{
    [Collection("Integration collection")]
    public class EmailIntegrationTests
    {
        private readonly IntegrationSetupFixture _fixture;
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _httpClient;

        public EmailIntegrationTests(IntegrationSetupFixture fixture)
        {
            _fixture = fixture;
            _factory = fixture.Factory;
            _httpClient = fixture.HttpClient;
        }

        [Fact]
        public async Task SendEmail_ShouldReturnOkResultMessage()
        {
            //Arrange
            _fixture.EmailClientMock.Setup(e => e.ConnectAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<SecureSocketOptions>(), It.IsAny<CancellationToken>()));
            _fixture.EmailClientMock.Setup(e => e.AuthenticateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()));
            _fixture.EmailClientMock.Setup(e => e.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<CancellationToken>()));

            //Act
            var response = await _httpClient.PostAsync($"/api/Email/Send/test/test/test", null);

            //Assert
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();

            Assert.Equal("Email has been sent successfully.", responseContent);
            _fixture.EmailClientMock.Verify(e => e.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<CancellationToken>()), Times.Once);

        }
    }


}

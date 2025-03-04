using Amazon.S3;
using Marelli.Business.Factories;
using Marelli.Business.IClients;
using Marelli.Business.IServices;
using Marelli.Infra.Context;
using Marelli.Test.Utils;
using Marelli.Test.Utils.Factories;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Testcontainers.PostgreSql;
using Xunit;

namespace Marelli.Test.Integration.Configuration
{
    [CollectionDefinition("Integration collection")]
    public class IntegrationSetupFixture : IAsyncLifetime
    {
        public IConfiguration Configuration { get; }
        public Mock<ISecretsManagerService> SecretsManagerServiceMock { get; }
        public Mock<IEmailClient> EmailClientMock { get; }
        public Mock<IAwsClient> AwsClientMock { get; }
        public Mock<ICustomHttpClientFactory> CustomHttpClientFactoryMock { get; set; }
        public Mock<HttpMessageHandler> HttpMessageHandlerMock { get; }
        public Mock<IAmazonS3> AmazonS3ClientMock { get; }
        public PostgreSqlContainer Container { get; private set; }
        public WebApplicationFactory<Program> Factory { get; private set; }
        public string TestToken { get; private set; }
        public HttpClient HttpClient { get; private set; }

        public IntegrationSetupFixture()
        {
            Configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            TestToken = TokenUtils.GenerateTestToken(Configuration["AppSettings:Secret"]);

            SecretsManagerServiceMock = new Mock<ISecretsManagerService>();
            EmailClientMock = new Mock<IEmailClient>();
            AwsClientMock = new Mock<IAwsClient>();
            AmazonS3ClientMock = new Mock<IAmazonS3>();
            CustomHttpClientFactoryMock = new Mock<ICustomHttpClientFactory>();
            HttpMessageHandlerMock = new Mock<HttpMessageHandler>();

            CustomHttpClientFactoryMock.Setup(c => c.GetHttpClient()).Returns(new HttpClient(HttpMessageHandlerMock.Object));

            Container = new PostgreSqlBuilder()
                .WithDatabase("test_db")
                .WithUsername("test_user")
                .WithPassword("test_password")
                .Build();

            Factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureServices(services =>
                    {
                        var descriptor = services.SingleOrDefault(
                            d => d.ServiceType == typeof(DbContextOptions<DemurrageContext>));
                        if (descriptor != null)
                        {
                            services.Remove(descriptor);
                        }

                        services.AddSingleton<ISecretsManagerService>(s => SecretsManagerServiceMock.Object);
                        services.AddScoped<IEmailClient>(e => EmailClientMock.Object);
                        services.AddSingleton<IAwsClient>(e => AwsClientMock.Object);
                        services.AddSingleton<IAmazonS3>(e => AmazonS3ClientMock.Object);
                        services.AddScoped<ICustomHttpClientFactory>(c => CustomHttpClientFactoryMock.Object);

                        services.AddSignalR();

                        SecretsManagerServiceMock.Setup(s => s.GetDbConnectionString())
                            .ReturnsAsync(Container.GetConnectionString());

                        services.AddDbContext<DemurrageContext>(options =>
                        {
                            options.UseNpgsql(Container.GetConnectionString());
                        });
                    });
                });
        }

        public async Task InitializeAsync()
        {
            await Container.StartAsync();

            using var scope = Factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<DemurrageContext>();
            await context.Database.MigrateAsync();

            HttpClient = await CreateHttpClient();
        }

        private async Task<HttpClient> CreateHttpClient()
        {
            var httpClient = Factory.CreateClient();
            var token = "";

            using (var scope = Factory.Services.CreateScope())
            {
                var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();
                var tokenResponse = await tokenService.GenerateAccessToken(UserFactory.GetUser());
                token = tokenResponse.AccessToken;
            }

            httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            return httpClient;

        }

        public async Task DisposeAsync()
        {
            await Container.StopAsync();
        }
    }
}

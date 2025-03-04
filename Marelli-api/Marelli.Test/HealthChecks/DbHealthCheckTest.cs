using Marelli.Api.HealthChecks.Marelli.Api.HealthChecks;
using Marelli.Infra.Context;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Moq;
using Xunit;

namespace Marelli.Test.HealthChecks
{
    public class DbHealthCheckTest
    {
        private readonly DbHealthCheck _dbHealthCheck;

        [Fact]
        public async Task CheckHealthAsync_ShouldReturnHealthy_WhenDatabaseIsAccessible()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockDbContext = new Mock<DemurrageContext>();
            var mockDatabase = new Mock<DatabaseFacade>(mockDbContext.Object);

            mockDbContext.Setup(x => x.Database).Returns(mockDatabase.Object);
            mockDatabase.Setup(x => x.CanConnectAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);

            var scopeMock = new Mock<IServiceScope>();
            scopeMock.Setup(x => x.ServiceProvider).Returns(mockServiceProvider.Object);

            var scopeFactoryMock = new Mock<IServiceScopeFactory>();
            scopeFactoryMock.Setup(x => x.CreateScope()).Returns(scopeMock.Object);

            mockServiceProvider.Setup(x => x.GetService(typeof(IServiceScopeFactory))).Returns(scopeFactoryMock.Object);
            mockServiceProvider.Setup(x => x.GetService(typeof(DemurrageContext))).Returns(mockDbContext.Object);

            _dbHealthCheck = new DbHealthCheck(mockServiceProvider.Object);

            // Act
            var result = await _dbHealthCheck.CheckHealthAsync(new HealthCheckContext(), CancellationToken.None);

            // Assert
            Assert.Equal(HealthStatus.Healthy, result.Status);
            Assert.Equal("Banco de dados está funcionando.", result.Description);
        }

        [Fact]
        public async Task CheckHealthAsync_ShouldReturnUnhealthy_WhenDatabaseIsNotAccessible()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockDbContext = new Mock<DemurrageContext>();
            var mockDatabase = new Mock<DatabaseFacade>(mockDbContext.Object);

            mockDbContext.Setup(x => x.Database).Returns(mockDatabase.Object);
            mockDatabase.Setup(x => x.CanConnectAsync(It.IsAny<CancellationToken>())).ReturnsAsync(false);

            var scopeMock = new Mock<IServiceScope>();
            scopeMock.Setup(x => x.ServiceProvider).Returns(mockServiceProvider.Object);

            var scopeFactoryMock = new Mock<IServiceScopeFactory>();
            scopeFactoryMock.Setup(x => x.CreateScope()).Returns(scopeMock.Object);

            mockServiceProvider.Setup(x => x.GetService(typeof(IServiceScopeFactory))).Returns(scopeFactoryMock.Object);
            mockServiceProvider.Setup(x => x.GetService(typeof(DemurrageContext))).Returns(mockDbContext.Object);

            _dbHealthCheck = new DbHealthCheck(mockServiceProvider.Object);

            // Act
            var result = await _dbHealthCheck.CheckHealthAsync(new HealthCheckContext(), CancellationToken.None);

            // Assert
            Assert.Equal(HealthStatus.Unhealthy, result.Status);
            Assert.Equal("Não foi possível conectar ao banco de dados.", result.Description);
        }

        [Fact]
        public async Task CheckHealthAsync_ShouldReturnUnhealthy_WhenExceptionIsThrown()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockDbContext = new Mock<DemurrageContext>();
            var mockDatabase = new Mock<DatabaseFacade>(mockDbContext.Object);

            mockDbContext.Setup(x => x.Database).Returns(mockDatabase.Object);
            mockDatabase.Setup(x => x.CanConnectAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("Erro de conexão"));

            var scopeMock = new Mock<IServiceScope>();
            scopeMock.Setup(x => x.ServiceProvider).Returns(mockServiceProvider.Object);

            var scopeFactoryMock = new Mock<IServiceScopeFactory>();
            scopeFactoryMock.Setup(x => x.CreateScope()).Returns(scopeMock.Object);

            mockServiceProvider.Setup(x => x.GetService(typeof(IServiceScopeFactory))).Returns(scopeFactoryMock.Object);
            mockServiceProvider.Setup(x => x.GetService(typeof(DemurrageContext))).Returns(mockDbContext.Object);

            _dbHealthCheck = new DbHealthCheck(mockServiceProvider.Object);

            // Act
            var result = await _dbHealthCheck.CheckHealthAsync(new HealthCheckContext(), CancellationToken.None);

            // Assert
            Assert.Equal(HealthStatus.Unhealthy, result.Status);
            Assert.Contains("Falha na conexão com o banco de dados", result.Description);
        }
    }
}

using Marelli.Business.IServices;
using Marelli.Business.Services;
using Marelli.Domain.Entities;
using Marelli.Test.Utils.Factories;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Marelli.Test.Services;

public class BuildStatusCheckerServiceTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldProcessBuildsCorrectly()
    {
        // Arrange
        var buildTableRowServiceMock = new Mock<IBuildTableRowService>();
        var jenkinsServiceMock = new Mock<IJenkinsService>();
        var serviceScopeMock = new Mock<IServiceScope>();
        var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
        var serviceProviderMock = new Mock<IServiceProvider>();

        var builds = new List<BuildTableRow> { BuildTableRowFactory.GetBuildTable() };

        buildTableRowServiceMock
            .Setup(s => s.ListAllInProgressBuildsOlderThan(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(builds);

        buildTableRowServiceMock
            .Setup(s => s.UpdateBuildTable(It.IsAny<int>(), It.IsAny<BuildTableRow>()))
            .ReturnsAsync(1);

        jenkinsServiceMock
            .Setup(s => s.GetBuildingStateByBuildId(It.IsAny<int>()))
            .ReturnsAsync((BuildingState)null);

        var scopedServiceProviderMock = new Mock<IServiceProvider>();
        scopedServiceProviderMock
            .Setup(sp => sp.GetService(typeof(IBuildTableRowService)))
            .Returns(buildTableRowServiceMock.Object);

        scopedServiceProviderMock
            .Setup(sp => sp.GetService(typeof(IJenkinsService)))
            .Returns(jenkinsServiceMock.Object);

        serviceScopeMock
            .Setup(s => s.ServiceProvider)
            .Returns(scopedServiceProviderMock.Object);

        serviceScopeFactoryMock
            .Setup(sf => sf.CreateScope())
            .Returns(serviceScopeMock.Object);

        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IServiceScopeFactory)))
            .Returns(serviceScopeFactoryMock.Object);

        var service = new BuildStatusCheckerService(serviceProviderMock.Object);

        // Act
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(1));
        await service.StartAsync(cancellationTokenSource.Token);

        // Assert
        buildTableRowServiceMock.Verify(s => s.ListAllInProgressBuildsOlderThan(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce());
        buildTableRowServiceMock.Verify(s => s.UpdateBuildTable(It.IsAny<int>(), It.IsAny<BuildTableRow>()), Times.Exactly(builds.Count));
        jenkinsServiceMock.Verify(s => s.GetBuildingStateByBuildId(It.IsAny<int>()), Times.Exactly(builds.Count));
    }


    [Fact]
    public async Task ExecuteAsync_ShouldDoNothingWhenNoBuildsFound()
    {
        // Arrange
        var buildTableRowServiceMock = new Mock<IBuildTableRowService>();
        var jenkinsServiceMock = new Mock<IJenkinsService>();
        var serviceScopeMock = new Mock<IServiceScope>();
        var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
        var serviceProviderMock = new Mock<IServiceProvider>();

        buildTableRowServiceMock
            .Setup(s => s.ListAllInProgressBuildsOlderThan(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<BuildTableRow>());

        var scopedServiceProviderMock = new Mock<IServiceProvider>();
        scopedServiceProviderMock
            .Setup(sp => sp.GetService(typeof(IBuildTableRowService)))
            .Returns(buildTableRowServiceMock.Object);

        scopedServiceProviderMock
            .Setup(sp => sp.GetService(typeof(IJenkinsService)))
            .Returns(jenkinsServiceMock.Object);

        serviceScopeMock
            .Setup(s => s.ServiceProvider)
            .Returns(scopedServiceProviderMock.Object);

        serviceScopeFactoryMock
            .Setup(sf => sf.CreateScope())
            .Returns(serviceScopeMock.Object);

        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IServiceScopeFactory)))
            .Returns(serviceScopeFactoryMock.Object);

        var service = new BuildStatusCheckerService(serviceProviderMock.Object);

        // Act
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(1));
        await service.StartAsync(cancellationTokenSource.Token);

        // Assert
        buildTableRowServiceMock.Verify(s => s.ListAllInProgressBuildsOlderThan(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce());
        buildTableRowServiceMock.Verify(s => s.UpdateBuildTable(It.IsAny<int>(), It.IsAny<BuildTableRow>()), Times.Never());
        jenkinsServiceMock.Verify(s => s.GetBuildingStateByBuildId(It.IsAny<int>()), Times.Never());
        jenkinsServiceMock.Verify(s => s.UpdateBuildingState(It.IsAny<int>(), It.IsAny<BuildingState>()), Times.Never());
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowExceptionWhenListAllInProgressBuildsOlderThanFails()
    {
        // Arrange
        var buildTableRowServiceMock = new Mock<IBuildTableRowService>();
        var jenkinsServiceMock = new Mock<IJenkinsService>();
        var serviceScopeMock = new Mock<IServiceScope>();
        var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
        var serviceProviderMock = new Mock<IServiceProvider>();

        buildTableRowServiceMock
            .Setup(s => s.ListAllInProgressBuildsOlderThan(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        var scopedServiceProviderMock = new Mock<IServiceProvider>();
        scopedServiceProviderMock
            .Setup(sp => sp.GetService(typeof(IBuildTableRowService)))
            .Returns(buildTableRowServiceMock.Object);

        scopedServiceProviderMock
            .Setup(sp => sp.GetService(typeof(IJenkinsService)))
            .Returns(jenkinsServiceMock.Object);

        serviceScopeMock
            .Setup(s => s.ServiceProvider)
            .Returns(scopedServiceProviderMock.Object);

        serviceScopeFactoryMock
            .Setup(sf => sf.CreateScope())
            .Returns(serviceScopeMock.Object);

        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IServiceScopeFactory)))
            .Returns(serviceScopeFactoryMock.Object);

        var service = new BuildStatusCheckerService(serviceProviderMock.Object);

        // Act
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(1));

        // Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => service.StartAsync(cancellationTokenSource.Token));
        Assert.Equal("Error while executing build status checker", exception.Message);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldHandleTaskCanceledExceptionGracefully()
    {
        // Arrange
        var buildTableRowServiceMock = new Mock<IBuildTableRowService>();
        var jenkinsServiceMock = new Mock<IJenkinsService>();
        var serviceScopeMock = new Mock<IServiceScope>();
        var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
        var serviceProviderMock = new Mock<IServiceProvider>();

        var builds = new List<BuildTableRow> { BuildTableRowFactory.GetBuildTable() };

        buildTableRowServiceMock
            .Setup(s => s.ListAllInProgressBuildsOlderThan(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(builds);

        buildTableRowServiceMock
            .Setup(s => s.UpdateBuildTable(It.IsAny<int>(), It.IsAny<BuildTableRow>()))
            .ReturnsAsync(1);

        jenkinsServiceMock
            .Setup(s => s.GetBuildingStateByBuildId(It.IsAny<int>()))
            .ReturnsAsync((BuildingState)null);

        var scopedServiceProviderMock = new Mock<IServiceProvider>();
        scopedServiceProviderMock
            .Setup(sp => sp.GetService(typeof(IBuildTableRowService)))
            .Returns(buildTableRowServiceMock.Object);

        scopedServiceProviderMock
            .Setup(sp => sp.GetService(typeof(IJenkinsService)))
            .Returns(jenkinsServiceMock.Object);

        serviceScopeMock
            .Setup(s => s.ServiceProvider)
            .Returns(scopedServiceProviderMock.Object);

        serviceScopeFactoryMock
            .Setup(sf => sf.CreateScope())
            .Returns(serviceScopeMock.Object);

        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IServiceScopeFactory)))
            .Returns(serviceScopeFactoryMock.Object);

        var service = new BuildStatusCheckerService(serviceProviderMock.Object);

        // Act
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.CancelAfter(TimeSpan.FromMilliseconds(500));
        await service.StartAsync(cancellationTokenSource.Token);

        // Assert
        buildTableRowServiceMock.Verify(s => s.ListAllInProgressBuildsOlderThan(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce());
        buildTableRowServiceMock.Verify(s => s.UpdateBuildTable(It.IsAny<int>(), It.IsAny<BuildTableRow>()), Times.AtLeastOnce());
        jenkinsServiceMock.Verify(s => s.UpdateBuildingState(It.IsAny<int>(), It.IsAny<BuildingState>()), Times.Never());
    }


}

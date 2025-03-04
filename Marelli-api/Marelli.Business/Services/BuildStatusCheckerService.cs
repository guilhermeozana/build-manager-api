using Marelli.Business.IServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Marelli.Business.Services;

public class BuildStatusCheckerService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly TimeSpan _interval;

    public BuildStatusCheckerService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _interval = TimeSpan.FromMinutes(25);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var buildTableRowService = scope.ServiceProvider.GetRequiredService<IBuildTableRowService>();
                    var jenkinsService = scope.ServiceProvider.GetRequiredService<IJenkinsService>();
                    var buildsOlderThan = await buildTableRowService.ListAllInProgressBuildsOlderThan(1, stoppingToken);

                    foreach (var build in buildsOlderThan)
                    {
                        build.Status = "Failed";
                        await buildTableRowService.UpdateBuildTable(build.Id, build);

                        var buildingStateByBuildId = await jenkinsService.GetBuildingStateByBuildId(build.Id);

                        if (buildingStateByBuildId != null)
                        {
                            await jenkinsService.UpdateBuildingState(buildingStateByBuildId.Id, buildingStateByBuildId);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error while executing build status checker", ex);
            }

            try
            {
                await Task.Delay(_interval, stoppingToken);
            }
            catch (TaskCanceledException)
            {
                throw new Exception("BuildStatusCheckerService is stopping.");
            }
        }
    }
}
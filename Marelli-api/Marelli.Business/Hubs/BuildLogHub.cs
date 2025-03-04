namespace Marelli.Business.Hubs
{
    using Marelli.Domain.Entities;
    using Microsoft.AspNetCore.SignalR;
    using System.Threading.Tasks;
    public class BuildLogHub : Hub
    {
        public async Task SendBuildLogs(List<BuildLog> buildLogs)
        {
            await Clients.All.SendAsync("ReceiveBuildLogs", buildLogs);
        }
    }
}

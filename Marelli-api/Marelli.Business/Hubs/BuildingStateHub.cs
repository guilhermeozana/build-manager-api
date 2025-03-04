namespace Marelli.Business.Hubs
{
    using Marelli.Domain.Entities;
    using Microsoft.AspNetCore.SignalR;
    using System.Threading.Tasks;

    public class BuildStateHub : Hub
    {
        public async Task SendBuildState(BuildingState state)
        {
            await Clients.All.SendAsync("ReceiveBuildState", state);
        }
    }
}

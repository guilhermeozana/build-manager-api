namespace Marelli.Business.Hubs
{
    using Marelli.Domain.Entities;
    using Microsoft.AspNetCore.SignalR;
    using System.Threading.Tasks;
    public class BuildTableHub : Hub
    {
        public async Task SendBuildTableRows(List<BuildTableRow> buildTables)
        {
            await Clients.All.SendAsync("ReceiveBuildTableRows", buildTables);
        }
    }
}

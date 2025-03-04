using Microsoft.AspNetCore.SignalR;

namespace Marelli.Business.Hubs
{
    public class UploadProgressHub : Hub
    {
        public async Task SendProgress(int progress)
        {
            await Clients.All.SendAsync("ReceiveProgress", progress);
        }
    }

}

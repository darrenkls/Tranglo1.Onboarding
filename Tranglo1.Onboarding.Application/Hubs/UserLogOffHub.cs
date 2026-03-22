using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.Hubs
{
    public class UserLogOffHub : Hub
    {
        private readonly ILogger<UserLogOffHub> _logger = null;

        public UserLogOffHub(ILogger<UserLogOffHub> logger)
        {
            _logger = logger;
        }

        public async Task JoinGroup(int businessProfileCode)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, businessProfileCode.ToString());
        }

        public async Task RemoveFromGroup(int businessProfileCode)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, businessProfileCode.ToString());
        }

        public override Task OnConnectedAsync()
        {
            _logger.LogInformation($"{Context.UserIdentifier} connected as {Context.ConnectionId}");
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            _logger.LogInformation($"{Context.UserIdentifier} disconnected as {Context.ConnectionId}");
            return base.OnDisconnectedAsync(exception);
        }
    }
}

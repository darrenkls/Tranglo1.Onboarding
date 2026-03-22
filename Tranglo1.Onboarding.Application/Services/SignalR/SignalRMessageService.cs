using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Application.Hubs;

namespace Tranglo1.Onboarding.Application.Services.SignalR
{
    public class SignalRMessageService
    {
        private readonly IHubContext<UserLogOffHub> _userLogOffHub;

        public SignalRMessageService(IHubContext<UserLogOffHub> userLogOffHub)
        {
            _userLogOffHub = userLogOffHub;
        }

        public async Task RedoBusinessDeclarationLogOffAlert(int businessProfileCode)
        {
            await _userLogOffHub.Clients.Group(businessProfileCode.ToString()).SendAsync("redoBusinessDeclarationLogOffAlert");
        }

        public async Task NonRedoBusinessDeclarationLogOffAlert(int businessProfileCode)
        {
            await _userLogOffHub.Clients.Group(businessProfileCode.ToString()).SendAsync("nonRedoBusinessDeclarationLogOffAlert");
        }
    }
}

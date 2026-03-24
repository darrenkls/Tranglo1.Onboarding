using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Tranglo1.Onboarding.Application.Command
{
    public class ResendInvitationCommand : IRequest<IdentityResult>
    {
        public string InviteeEmail { get; set; }
    }
}

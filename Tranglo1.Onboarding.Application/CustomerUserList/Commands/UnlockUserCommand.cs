using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Tranglo1.Onboarding.Application.CustomerUserList.Commands
{
    public class UnlockUserCommand : IRequest<IdentityResult>
    {
        public string Email { get; set; }
    }

    public class UnlockUserCommandHandler
    {
    }
}

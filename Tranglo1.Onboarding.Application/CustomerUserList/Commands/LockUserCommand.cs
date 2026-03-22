using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Tranglo1.Onboarding.Application.CustomerUserList.Commands
{
    public class LockUserCommand : IRequest<IdentityResult>
    {
        public string Email { get; set; }
    }
}

using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Tranglo1.Onboarding.Application.CustomerUserList.Commands
{
    public class VerifyCustomerUserEmailCommand : IRequest<IdentityResult>
    {
        public string UserId { get; set; }
        public string Token { get; set; }
    }
}

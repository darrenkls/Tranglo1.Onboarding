using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Tranglo1.Onboarding.Application.CustomerUserList.Commands
{
    public class RequestResetPasswordCommand : IRequest<Result<IdentityResult>>
    {
        public string Email { get; set; }
    }
}

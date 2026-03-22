using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Tranglo1.Onboarding.Application.CustomerUserList.Commands
{
    public class ValidateUserPasswordCommand : IRequest<Result<SignInResult>>
    {
        public string Username { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        public string Password { get; set; }

        public string Country { get; set; }
        public bool RememberLogin { get; set; }
        public string ReturnUrl { get; set; }
    }
}

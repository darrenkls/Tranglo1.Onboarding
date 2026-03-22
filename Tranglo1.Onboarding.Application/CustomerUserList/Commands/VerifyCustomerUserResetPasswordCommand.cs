using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Tranglo1.Onboarding.Application.CustomerUserList.Commands
{
    public class VerifyCustomerUserResetPasswordCommand : IRequest<IdentityResult>
    {
        public string Email { get; set; }
        public string Token { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        public string NewPassword { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        public string ConfirmPassword { get; set; }
    }
}

using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Tranglo1.Onboarding.Application.CustomerUserList.Commands
{
    public class InviteePasswordVerificationCommand : IRequest<IdentityResult>
    {
        public string LoginId { get; set; }
        public string Name { get; set; }
        public string ResetPasswordToken { get; set; }
        public string EmailConfirmationToken { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        public string NewPassword { get; set; }
    }
}

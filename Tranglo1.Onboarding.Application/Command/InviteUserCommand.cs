using MediatR;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace Tranglo1.Onboarding.Application.Command
{
    public class InviteUserCommand : IRequest<IdentityResult>
    {
        public int UserEnvironmentCode { get; set; }
        public string InviterEmail { get; set; }
        public int BusinessProfileCode { get; set; }
        public string InviteeFullName { get; set; }
        public string InviteeEmail { get; set; }
        public int InviteeRoleCode { get; set; }
        public List<string> InviteeRoleCodeList { get; set; }
        public string LoginId { get; set; }
        public string Timezone { get; set; }
        public int SolutionCode { get; set; }
    }
}

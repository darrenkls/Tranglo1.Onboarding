using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Tranglo1.Onboarding.Domain.Entities;

namespace Tranglo1.Onboarding.Application.CustomerUserList.Commands
{
    public class RegisterCustomerUserWithCodeCommand : IRequest<IdentityResult>
    {
        public string RegistryCode { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string RecaptchaToken { get; set; }
        public long PartnerRegistrationLeadsOriginCode { get; set; } = PartnerRegistrationLeadsOrigin.Website.Id;
        public string OtherLeadsOrigin { get; set; }
        public ModelStateDictionary ModelState { get; set; }
    }
}

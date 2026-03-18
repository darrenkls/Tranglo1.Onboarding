using System;
using Tranglo1.Onboarding.Domain.ExternalServices.Compliance.Models;

namespace Tranglo1.Onboarding.Domain.ExternalServices.Compliance.Models.Responses
{
    public class NameScreenerResponse
    {
        public Guid Reference { get; set; }
        public Summary Summary { get; set; }
    }
}

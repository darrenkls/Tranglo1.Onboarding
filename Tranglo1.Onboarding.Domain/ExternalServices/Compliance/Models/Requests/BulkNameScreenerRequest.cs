using System;
using Tranglo1.Onboarding.Domain.Entities;

namespace Tranglo1.Onboarding.Domain.ExternalServices.Compliance.Models.Requests
{
    public class BulkNameScreenerRequest : NameScreenerRequest
    {
        public string ClientReference { get; private set; }

        public BulkNameScreenerRequest(ScreeningEntityType entityType, string fullName, DateTime? dateOfBirth, string nationality, string gender, string clientReference) 
            : base(entityType, fullName, dateOfBirth, nationality, gender)
        {
            ClientReference = clientReference;
        }
    }
}

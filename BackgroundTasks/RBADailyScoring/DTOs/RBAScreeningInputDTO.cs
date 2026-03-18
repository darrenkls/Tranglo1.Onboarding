using System;
using System.Collections.Generic;
using Tranglo1.Onboarding.Domain.Entities.RBAAggregate;

namespace Tranglo1.RBADailyScoring.DTOs
{
    public class RBAScreeningInputDTO
    {
        public int BusinessProfileCode { get; set; }
        public long? IndustrySector { get; set; }
        public string CollectionTier { get; set; }
        public string Nationality { get; set; }
        public DateTime? IncorporationDate { get; set; }
        public string IncorporationCountry { get; set; }
        public long? IncorporationType { get; set; }
        public long? CustomerCategory { get; set; }
        public bool IsPEP { get; set; }
        public bool EnforcementActionTakenByRegulator { get; set; }
        public List<RBA> RBAList { get; set; }

        public RBAScreeningInputDTO(int businessProfileCode, long? businessNature, string collectionTierName, string nationality, DateTime? dateOfIncorporation, string country, long? incorporationCompanyTypeCode, long customerTypeCode, bool isPEP, bool isEnforcementAction, List<RBA> rbaResultList)
        {
            BusinessProfileCode = businessProfileCode;
            IndustrySector = businessNature;
            CollectionTier = collectionTierName;
            Nationality = nationality;
            IncorporationDate = dateOfIncorporation;
            IncorporationCountry = country;
            IncorporationType = incorporationCompanyTypeCode;
            CustomerCategory = customerTypeCode;
            IsPEP = isPEP;
            EnforcementActionTakenByRegulator = isEnforcementAction;
            RBAList = rbaResultList;
        }
    }
}

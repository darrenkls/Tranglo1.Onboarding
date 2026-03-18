using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Text;
using Tranglo1.Onboarding.Domain.Common;

namespace Tranglo1.Onboarding.Domain.Entities.RBAAggregate
{
    public class RBAScreeningInput : AggregateRoot<long>
    {
        public long BusinessProfileCode { get; set; }
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


        private RBAScreeningInput() { }

        public RBAScreeningInput(long businessProfileCode, BusinessNature businessNature, string collectionTier, string nationality, DateTime? incorporationDate, string incorporationCountry, long? incorporationType,
            long? customerCategory, bool isPEP, bool enforcementActionTakenByRegulator, List<RBA> rbaList)
        {
            this.BusinessProfileCode = businessProfileCode;
            this.IndustrySector = businessNature.Id;
            this.CollectionTier = collectionTier;
            this.Nationality = nationality;
            IncorporationDate = incorporationDate;
            IncorporationCountry = incorporationCountry;
            IncorporationType = incorporationType;
            CustomerCategory = customerCategory;
            IsPEP = isPEP;
            EnforcementActionTakenByRegulator = enforcementActionTakenByRegulator;
            RBAList = rbaList;
        }
    }
}

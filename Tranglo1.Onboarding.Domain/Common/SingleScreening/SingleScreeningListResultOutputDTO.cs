using System;

namespace Tranglo1.Onboarding.Domain.Common.SingleScreening
{
    public class SingleScreeningListResultOutputDTO
    {
        public string ClientReference { set; get; }
        public Guid Reference { set; get; }
        public Summary Summary { set; get; }
    }

    public class Summary
    {
        public int SanctionList { set; get; }
        public int PEP { set; get; }
        public int SOE { set; get; }
        public int AdverseMedia { set; get; }
        public int Enforcement { get; set; }
        public int AssociatedEntity { get; set; }

        public bool HasComplianceHit()
        {
            return SanctionList > 0 || PEP > 0 || SOE > 0 || AdverseMedia > 0 || Enforcement > 0 || AssociatedEntity > 0;
        }
    }
}

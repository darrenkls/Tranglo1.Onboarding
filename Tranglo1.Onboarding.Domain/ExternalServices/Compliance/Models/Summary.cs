namespace Tranglo1.Onboarding.Domain.ExternalServices.Compliance.Models
{
    public class Summary
    {
        public int Registrations { get; set; }
        public int AssociatedEntity { get; set; }
        public int SanctionList { set; get; }
        public int PEP { set; get; }
        public int SOE { set; get; }
        public int AdverseMedia { set; get; }
        public int Enforcement { set; get; }

        public bool HasComplianceHit()
        {
            return AssociatedEntity > 0 || 
                SanctionList > 0 || 
                PEP > 0 || 
                SOE > 0 || 
                AdverseMedia > 0 ||
                Enforcement > 0;
        }
    }
}

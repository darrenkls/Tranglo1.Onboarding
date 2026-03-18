namespace Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate
{
    public class KYCBusinessSummary
    {
        public bool IsBusinessDeclarationCompleted { get; set; }
        public bool IsBusinessProfileCompleted { get; set; }
        public bool IsTransactionEvaluationCompleted { get; set; }
        public bool IsLicenseInfoCompleted { get; set; }
        public bool IsOwnershipCompleted { get; set; }
        public bool IsVerificationCompleted { get; set; }
        public bool IsDocumentationCompleted { get; set; }
        public bool IsAMLCompleted { get; set; }
        public bool IsCoInfoCompleted { get; set; }
        public bool IsDeclarationInfoCompleted { get; set; }

        public bool IsShareholderCompleted { get; set; }
        public bool IsBoardOfDirectorCompleted { get; set; }
        public bool IsAuthorisedPersonsCompleted { get; set; }
        public bool IsUltimateBeneficialOwnerCompleted { get; set; }
        public bool IsPrincipalOfficerCompleted { get; set; }

        public bool IsCompanyDetailCompleted { get; set; }
        public bool IsAddressCompleted { get; set; }
        public bool IsCompanyContactCompleted { get; set; }
        public bool IsContactPersonCompleted { get; set; }
    }
}

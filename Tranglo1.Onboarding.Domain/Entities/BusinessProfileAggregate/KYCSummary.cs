using System;
using System.Collections.Generic;
using System.Text;

namespace Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate
{
    public class KYCSummary
    {
        // public bool isBusinessProfileCompleted { get; set; }
        public bool isBusinessProfileCompleted =>
            BusinessProfileSummary?.IsAllInfoCompleted() ?? false;

        public bool isDocumentationCompleted { get; set; }

        public bool isLicenseInfoCompleted { get; set; }

        public bool isOwnershipCompleted =>
            OwnershipSummary?.IsAllInfoCompleted() ?? false;

        public bool isAMLCompleted { get; set; }
        public bool isCoInfoCompleted { get; set; }
        public bool isDeclarationInfoCompleted { get; set; }

        public KYCBusinessProfileSummary BusinessProfileSummary { get; set; }
        public KYCOwnershipSummary OwnershipSummary { get; set; }

    }

    public class KYCBusinessProfileSummary
    {
        public bool IsCompanyDetailsCompleted { get; set; }
        public bool IsAddressCompleted { get; set; }
        public bool IsContactPersonCompleted { get; set; }

        public bool IsAllInfoCompleted()
        {
            return this.IsCompanyDetailsCompleted && this.IsAddressCompleted && this.IsContactPersonCompleted;
        }
    }

    public class KYCOwnershipSummary
    {
        public bool IsShareholderCompleted { get; set; }
        public bool IsUltimateBeneficialOwnerCompleted { get; set; }
        public bool IsBoardOfDirectorCompleted { get; set; }
        public bool IsPrincipalOfficerCompleted { get; set; }
        public bool IsAuthorisedPersonCompleted { get; set; }
        public bool IsLicensedParentCompanyCompleted { get; set; }

        public bool IsAllInfoCompleted()
        {
            return IsShareholderCompleted && this.IsUltimateBeneficialOwnerCompleted && this.IsBoardOfDirectorCompleted
                && this.IsPrincipalOfficerCompleted && this.IsAuthorisedPersonCompleted && this.IsLicensedParentCompanyCompleted;
        }
    }

}

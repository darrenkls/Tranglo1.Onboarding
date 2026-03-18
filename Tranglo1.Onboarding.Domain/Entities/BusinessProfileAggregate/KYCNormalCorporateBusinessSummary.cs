using System;
using System.Collections.Generic;
using System.Text;

namespace Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate
{
    public class KYCNormalCorporateBusinessSummary
    {
        public bool isBusinessProfileCompleted { get; set; }
        public bool isLicenseInfoCompleted { get; set; }
        public bool isOwnershipCompleted { get; set; }
        public bool isDocumentationCompleted { get; set; }
        public bool isBusinessUserDeclarationInfoCompleted { get; set; }
    }
}

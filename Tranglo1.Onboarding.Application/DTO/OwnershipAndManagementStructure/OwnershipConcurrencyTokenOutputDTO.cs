using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.OwnershipAndManagementStructure
{
    public class OwnershipConcurrencyTokenOutputDTO
    {
        public Guid? ReviewConcurrencyToken { get; set; }
        public Guid? ReviewAndFeedbackConcurrencyToken { get; set; }
        public Guid? AffiliatesAndSubsidiariesConcurrencyToken { get; set; }
        public Guid? AuthorisedPersonConcurrencyToken { get; set; }
        public Guid? BoardOfDirectorConcurrencyToken { get; set; }
        public Guid? CompanyLegalEntityConcurrencyToken { get; set; }
        public Guid? CompanyShareholderConcurrencyToken { get; set; }
        public Guid? IndividualLegalEntityConcurrencyToken { get; set; }
        public Guid? IndividualShareholderConcurrencyToken { get; set; }
        public Guid? ParentHoldingsConcurrencyToken { get; set; }
        public Guid? PrimaryOfficerConcurrencyToken { get; set; }
        public Guid? LegalEntityConcurrencyToken { get; set; }
        public Guid? ShareholderConcurrencyToken { get; set; }
        public Guid? AMLCFTQuestionnaireConcurrencyToken { get; set; }
        public Guid? BusinessProfileConcurrencyToken { get; set; }
        public Guid? TransactionEvalConcurrencyToken { get; set; }
    }
}

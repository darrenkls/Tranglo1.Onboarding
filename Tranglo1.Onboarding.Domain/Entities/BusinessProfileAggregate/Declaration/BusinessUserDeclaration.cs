using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate.Declaration
{
    public class BusinessUserDeclaration : Entity
    {
        public int BusinessProfileCode { get; set; }
        public bool? IsNotRemittancePartner { get; set; }
        public bool? IsAuthorized { get; set; }
        public bool? IsInformationTrue { get; set; }
        public bool? IsAgreedTermsOfService { get; set; }
        public bool? IsDeclareTransactionTax { get; set; }
        public bool? IsAllApplicationAccurate { get; set; }
        public Guid? DocumentId { get; set; }
        public string SigneeName { get; set; }
        public string Designation { get; set; }
        public Guid? BusinessUserDeclarationConcurrencyToken { get; set; }

        private BusinessUserDeclaration()
        {

        }

        public BusinessUserDeclaration(int businessProfileCode, bool? isNotRemittancePartner, bool? isAuthorized, bool? isInformationTrue,
            bool? isAgreedTermsOfService, bool? isDeclareTransactionTax,bool? isAllApplicationAccurate, Guid? documentID, string signeeName, string designation,
            Guid? businessUserDeclarationConcurrencyToken)
        {
            BusinessProfileCode = businessProfileCode;
            IsNotRemittancePartner = isNotRemittancePartner;
            IsAuthorized = isAuthorized;
            IsInformationTrue = isInformationTrue;
            IsAgreedTermsOfService = isAgreedTermsOfService;
            IsDeclareTransactionTax = isDeclareTransactionTax;
            IsAllApplicationAccurate = isAllApplicationAccurate;
            DocumentId = documentID;
            SigneeName = signeeName;
            Designation = designation;
            BusinessUserDeclarationConcurrencyToken = businessUserDeclarationConcurrencyToken;
        }
    }

  
}

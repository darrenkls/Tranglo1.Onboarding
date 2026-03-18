using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tranglo1.Onboarding.Domain.Entities
{
    public class Declaration: Entity
    {
        public BusinessProfile BusinessProfile { get; set; }
        public bool? IsAuthorized { get; set; }
        public bool? IsInformationTrue { get; set; }
        public bool? IsAgreedTermsOfService { get; set; }
        public bool? IsDeclareTransactionTax { get; set; }
        public bool? IsAllApplicationAccurate { get; set; }
        public bool IsShowOldUI { get; set; }
        public Guid? DocumentId { get; set; }
        public string SigneeName { get; set; }
        public string Designation { get; set; }

        public Declaration(bool isTCRevampFeature)
        {
            this.IsShowOldUI = isTCRevampFeature ? false : true;
        }

        public Declaration()
        {
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.Declaration
{
    public class BusinessUserDeclarationOutputDTO
    {
        public long BusinessProfileCode { get; set; }
        public long? BusinessUserDeclarationCode { get; set; }
        public bool? IsNotRemittancePartner { get; set; }
        public bool? IsAuthorized { get; set; }
        public bool? IsInformationTrue { get; set; }
        public bool? IsAgreedTermsOfService { get; set; }
        public bool? IsDeclareTransactionTax { get; set; }
        public bool? IsAllApplicationAccurate { get; set; }
        public string SigneeName { get; set; }
        public string Designation { get; set; }
        public Guid? BusinessUserDeclarationConcurrencyToken { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.Declaration
{
    public class GetBusinessUserDeclarationOutputDTO
    {
        public long? BusinessProfileCode { get; set; }
        public long? BusinessUserDeclarationCode { get; set; }
        public bool? isNotRemittancePartner { get; set; }
        public bool? isAuthorized { get; set; }
        public bool? isInformationTrue { get; set; }
        public bool? isAgreeTermsOfService { get; set; }
        public bool? isDeclareTransactionTax { get; set; }
        public bool? IsAllApplicationAccurate { get; set; }
        public string? SigneeName { get; set; }
        public string? Designation { get; set; }
        public Guid? DocumentId { get; set; }
        public string FileName { get; set; }
        public long? BusinessKYCSubmissionStatusCode { get; set; }
        public string BusinessKYCSubmissionStatusDescription { get; set; }
        public DateTime? ReviewConcurrentLastModified { get; set; }
        public Guid? ReviewConcurrencyToken { get; set; }
        public Guid? BusinessUserDeclarationConcurrencyToken { get; set; }

    }
}

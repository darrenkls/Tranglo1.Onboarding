using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.Declaration
{
    public class DeclarationOutputDTO
    {
        public long? BusinessProfileCode { get; set; }
        public long? DeclarationCode { get; set; }
        public bool? isAuthorized { get; set; }
        public bool? isInformationTrue { get; set; }
        public bool? isAgreeTermsOfService { get; set; }
        public bool? isDeclareTransactionTax { get; set; }
        public string? SigneeName { get; set; }
        public string? Designation { get; set; }
        public Guid? DocumentId { get; set; }
        public string FileName { get; set; }
    }
}

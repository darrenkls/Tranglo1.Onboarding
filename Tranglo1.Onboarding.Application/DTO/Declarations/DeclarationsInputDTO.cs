using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.Declarations
{
    public class DeclarationsInputDTO
    {
        public bool? IsAuthorized { get; set; }
        public bool? IsInformationTrue { get; set; }
        public bool? IsAgreedTermsOfService { get; set; }
        public bool? IsDeclareTransactionTax { get; set; }
        public bool? IsAllApplicationAccurate { get; set; }

        [MaxLength(150, ErrorMessage = "Name of Signee for user declaration maximum length is 150 character")]
        public string SigneeName { get; set; }

        [MaxLength(150, ErrorMessage = "Designation for user declaration maximum length is 150 character")]
        public string Designation { get; set; }
    }
}

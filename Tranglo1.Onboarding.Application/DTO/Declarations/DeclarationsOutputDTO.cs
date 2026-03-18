using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.Declarations
{
    public class DeclarationsOutputDTO
    {
        public long Id { get; set; }
        public bool? IsAuthorized { get; set; }
        public bool? IsInformationTrue { get; set; }
        public bool? IsAgreedTermsOfService { get; set; }
        public bool? IsDeclareTransactionTax { get; set; }
        public bool? IsAllApplicationAccurate { get; set; }
        public bool IsShowOldUI { get; set; }
        public string SigneeName { get; set; }
        public string Designation { get; set; }
        public Guid DocumentId { get; set; }
        public string FileName { get; set; }
        public DateTime? ReviewConcurrentLastModified { get; set; }
        public Guid? ReviewConcurrencyToken { get; set; }
    }
}

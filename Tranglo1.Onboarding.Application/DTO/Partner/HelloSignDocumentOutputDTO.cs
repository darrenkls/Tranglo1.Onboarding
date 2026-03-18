using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.Partner
{
    public class HelloSignDocumentOutputDTO
    {
        public long HelloSignDocumentId { get; set; }
        public string DocumentName { get; set; }
        public long PartnerCode { get; set; }
        public string SentBy { get; set; }
        public DateTime SentDate { get; set; }
        public bool IsRemoved { get; set; }
        public string RemovedBy { get; set; }
        public DateTime? RemovedDate { get; set; }
    }
}

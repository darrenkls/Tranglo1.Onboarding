using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.Documentation
{
    public class ReleaseDocumentsUploadHistoryOutputDTO
    {
        public int BusinessProfileCode { get; set; }
        public Guid DocumentId { get; set; }
        public string File { get; set; }
        public string UploadedBy { get; set; }
        public DateTime? UploadedAt { get; set; }
    }
}

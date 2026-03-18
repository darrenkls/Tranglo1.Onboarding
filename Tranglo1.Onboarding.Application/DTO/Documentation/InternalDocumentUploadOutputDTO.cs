using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.Documentation
{
    public class InternalDocumentUploadOutputDTO
    {
        public int BusinessProfileCode { get; set; }
        public Guid DocumentId { get; set; }
        public string File { get; set; }
        public string UploadedBy { get; set; }
        public DateTime? UploadedAt { get; set; }
        public string DeletedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
        public bool IsRemoved { get; set; }
        public bool IsDisplay { get; set; }

    }
}

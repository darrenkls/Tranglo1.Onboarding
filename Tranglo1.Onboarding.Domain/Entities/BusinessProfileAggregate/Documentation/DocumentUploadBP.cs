using System;
using System.Collections.Generic;
using System.Text;

namespace Tranglo1.Onboarding.Domain.Entities
{
    public class DocumentUploadBP
    {
        public DocumentCategoryBP DocumentCategoryBP { get; set; }
        public long DocumentCategoryBPCode { get; set; }
        public Guid DocumentId { get; set; }
        public bool IsVerified { get; set; }
    }
}

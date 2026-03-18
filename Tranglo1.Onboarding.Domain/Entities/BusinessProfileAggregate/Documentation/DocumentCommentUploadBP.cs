using System;
using System.Collections.Generic;
using System.Text;

namespace Tranglo1.Onboarding.Domain.Entities
{
   public class DocumentCommentUploadBP 
    {
       public DocumentCommentBP DocumentCommentBP { get; set; }
        public long DocumentCommentBPCode { get; set; }
        public Guid DocumentId { get; set; }
    
    }
}

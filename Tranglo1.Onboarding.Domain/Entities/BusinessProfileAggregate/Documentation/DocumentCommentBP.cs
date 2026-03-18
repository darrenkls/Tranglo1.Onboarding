using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tranglo1.Onboarding.Domain.Entities
{
    public class DocumentCommentBP: Entity
    {
        public DocumentCategoryBP DocumentCategoryBP { get; set; }
        public long DocumentCategoryBPCode { get; set; }
        public string Comment { get; set; }
        public bool IsExternal { get; set; }

      
    }
}

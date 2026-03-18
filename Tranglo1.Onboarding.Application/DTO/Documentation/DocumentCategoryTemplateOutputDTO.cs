using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.Documentation
{
    public class DocumentCategoryTemplateOutputDTO
    {
        public int DocumentCategoryCode { get; set; }
        public Guid DocumentId { get; set; }
        public string FileName { get; set; }
    }
}

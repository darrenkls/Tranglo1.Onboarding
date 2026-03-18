using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.Documentation
{
    public class BusinessDocumentCategoryListOutputDTO
    {
        public int BusinessDocumentGroupCategoryCode { get; set; }
        public string GroupCategoryDescription { get; set; }
        public string Description { get; set; }
        public long CategoryId { get; set; }
        public bool IsAMLCFT { get; set; }
        public int DocumentCategoryStatusCode { get; set; }
        public string DocumentCategoryStatusDescription { get; set; }
        public string BusinessCategoryTooltipDescription { get; set; }
        public int DocumentCategoryGroupCode { get; set; }
        public string TooltipDescription { get; set; }
        public Guid[] TemplateIds { get; set; }
        public string[] FileName { get; set; }
    }
}

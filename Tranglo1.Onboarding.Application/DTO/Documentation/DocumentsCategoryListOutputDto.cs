using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.Queries
{
    public class DocumentsCategoryListOutputDto
    {
        public string Description { get; set; }
        public long CategoryId { get; set; }
        public Guid[] TemplateIds { get; set; }
        public bool IsAMLCFT { get; set; }
        public int DocumentCategoryStatusCode { get; set; }
        public string DocumentCategoryStatusDescription { get; set; }
        public string AdminCategoryTooltipDescription { get; set; }
        public string CustomerCategoryTooltipDescription { get; set; }
        public int SequenceNo { get; set; }
        public int GroupCategoryCode { get; set; }
        public string GroupCategoryDesc { get; set; }
        public int GroupSequence { get; set; }

    }
}

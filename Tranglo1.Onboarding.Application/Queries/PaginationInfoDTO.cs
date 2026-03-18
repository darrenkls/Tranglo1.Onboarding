using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.Queries
{
    public class PaginationInfoDTO
    {
        public int RowCount { get; set; }
        public int PageSize { get; set; }
        public int PageIndex { get; set; }
    }
}
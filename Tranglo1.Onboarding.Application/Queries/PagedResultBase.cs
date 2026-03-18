using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.Queries
{
    public class PagedResultBase
    {
        public int CurrentPage { get; set; }
        public int PageCount
        {
            get
            {
                return (RowCount + PageSize - 1) / PageSize;
            }

        }
        public int PageSize { get; set; }
        public int RowCount { get; set; }
        public int FirstRowOnPage
        {

            get { return (CurrentPage - 1) * PageSize + 1; }
        }
        public int LastRowOnPage
        {
            get { return Math.Min(CurrentPage * PageSize, RowCount); }
        }
    }
    public class PagedResult<T> : PagedResultBase where T : class
    {
        public IEnumerable<T> Results { get; set; }

        public PagedResult()
        {
            Results = new List<T>();
        }
    }
}

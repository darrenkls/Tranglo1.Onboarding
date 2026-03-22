namespace Tranglo1.Onboarding.Domain.Common
{
    public enum SortDirection
    {
        Ascending,
        Descending
    }

    public class PagingOptions
    {
        public PagingOptions()
        {
            this.PageIndex = 0;
            this.PageSize = 25;
        }

        /// <summary>
        /// get or set the page index. This is a zero-based value.
        /// </summary>
        public int PageIndex { get; set; }

        /// <summary>
        /// get or set the page size. Default value is 25
        /// </summary>
        public int PageSize { get; set; }

        string sortExpression;
        public string SortExpression
        {
            get { return sortExpression; }
            set { sortExpression = value?.Trim(); }
        }

        public SortDirection Direction { get; set; }

        public static PagingOptions Default
        {
            get
            {
                return new PagingOptions()
                {
                    PageIndex = 0,
                    PageSize = 10,
                    Direction = SortDirection.Ascending
                };
            }
        }
    }
}

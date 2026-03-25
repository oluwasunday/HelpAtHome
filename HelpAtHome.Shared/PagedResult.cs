namespace HelpAtHome.Shared
{
    public class PagedResult<T>
    {
        public IEnumerable<T> Items { get; set; }
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
        public bool HasNext => Page < TotalPages;
        public bool HasPrevious => Page > 1;
        public PagedResult(IEnumerable<T> items, int total, int page, int pageSize)
        { Items = items; TotalCount = total; Page = page; PageSize = pageSize; }
    }

}

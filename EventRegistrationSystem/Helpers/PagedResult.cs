namespace EventRegistrationSystem.Repositories
{
    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();      // items on current page
        public int Page { get; set; }                    // current page number (1-based)
        public int PageSize { get; set; }                // how many items per page
        public int TotalCount { get; set; }              // total items across ALL pages

        // Calculated properties
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasPreviousPage => Page > 1;
        public bool HasNextPage => Page < TotalPages;
    }
}   
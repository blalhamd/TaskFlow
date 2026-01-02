namespace TaskFlow.Shared.Common
{
    public class PagesResult<T>
    {
        public PagesResult(IEnumerable<T> items, int pageIndex, int pageSize, int totalCount)
        {
            SetItems(items);
            SetPageIndex(pageIndex);
            SetPageSize(pageSize);
            SetTotalCount(totalCount);
        }

        public IEnumerable<T>? Items { get; private set; }
        public int TotalCount { get; private set; }
        public int PageNumber { get; private set; }
        public int PageSize { get; private set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasForward => TotalPages > PageNumber; // 5 pages and stand in 3
        public bool HasPrevious => PageNumber > 1;

        private void SetItems(IEnumerable<T> items)
        {
            Items = items ?? Enumerable.Empty<T>().ToList();
        }

        private void SetPageIndex(int pageNumber)
        {
            if (pageNumber <= 0)
                throw new ArgumentOutOfRangeException(nameof(pageNumber), "page index can't be less than or equal zero.");
            PageNumber = pageNumber;
        }

        private void SetPageSize(int pageSize)
        {
            if (pageSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(pageSize), "page size can't be less than or equal zero");
            PageSize = pageSize;
        }

        private void SetTotalCount(int totalCount)
        {
            if (totalCount < 0)
                throw new ArgumentOutOfRangeException(nameof(totalCount), "total count can't be less than zero");
            TotalCount = totalCount;
        }
    }
}


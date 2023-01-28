

using Microsoft.EntityFrameworkCore;

namespace API.Helpers
{
    public class PageList<T> : List<T>
    {
        public PageList(IEnumerable<T> items, int count, int pageNumber, int pageSize) 
        {
            CurrentPage = pageNumber;
            TotalPages = (int) Math.Ceiling(count / (double) pageSize);
            PageSize = pageSize;
            TotalCount = count ;
            AddRange(items);
        }

        public int CurrentPage { get; }
        public int TotalPages { get; }
        public int PageSize { get; }
        public int TotalCount { get; }

        public static async Task<PageList<T>> CreateAsync(IQueryable<T> source, int pageNumber, int pageSize)
        {
            var count = await source.CountAsync();
            var items = await source.Skip((pageNumber - 1) * pageSize ).Take(pageSize).ToListAsync();
            return new PageList<T>(items, count, pageNumber, pageSize);
        }
    }
}

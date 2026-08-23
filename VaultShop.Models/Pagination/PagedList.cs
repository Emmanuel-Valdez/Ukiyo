namespace VaultShop.Models.Pagination;

public class PagedList<T> : List<T>
{
	public int PageIndex { get; }
	public int PageSize { get; }
	public int TotalCount { get; }
	public int TotalPages { get; }
	public bool HasPreviousPage => PageIndex > 1;
	public bool HasNextPage => PageIndex < TotalPages;

	public PagedList(List<T> items, int totalCount, int pageIndex, int pageSize)
		: base(items)
	{
		PageSize = Math.Max(1, pageSize);
		TotalCount = totalCount;
		PageIndex = Math.Max(1, pageIndex);
		TotalPages = totalCount == 0 ? 0 : Math.Max(1, (int)Math.Ceiling(totalCount / (double)PageSize));
	}

	public static PagedList<T> Create(IEnumerable<T> source, int pageIndex, int pageSize)
	{
		var items = source as ICollection<T> ?? source.ToList();
		pageSize = Math.Max(1, pageSize);
		var pageItems = items.Skip((Math.Max(1, pageIndex) - 1) * pageSize).Take(pageSize).ToList();
		return new PagedList<T>(pageItems, items.Count, pageIndex, pageSize);
	}
}

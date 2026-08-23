using VaultShop.Models.Pagination;

namespace VaultShop.Web.Tests;

public sealed class PagedListTests
{
	[Fact]
	public void Create_ExactDivision_ComputesTotalPages()
	{
		var page = PagedList<int>.Create(Source(24), 1, 12);

		Assert.Equal(2, page.TotalPages);
		Assert.Equal(24, page.TotalCount);
		Assert.True(page.HasNextPage);
		Assert.False(page.HasPreviousPage);
	}

	[Fact]
	public void Create_Remainder_RoundsTotalPagesUp()
	{
		var page = PagedList<int>.Create(Source(25), 3, 12);

		Assert.Equal(3, page.TotalPages);
		Assert.Single(page);
		Assert.Equal(25, page.Single());
		Assert.False(page.HasNextPage);
		Assert.True(page.HasPreviousPage);
	}

	[Fact]
	public void Create_SlicesRequestedPage()
	{
		var page = PagedList<int>.Create(Source(30), 2, 12);

		Assert.Equal(Enumerable.Range(13, 12), page);
	}

	[Fact]
	public void Create_PageIndexBelowOne_ClampsToOne()
	{
		var page = PagedList<int>.Create(Source(15), 0, 12);

		Assert.Equal(1, page.PageIndex);
		Assert.Equal(Enumerable.Range(1, 12), page);
	}

	[Fact]
	public void Create_EmptySource_YieldsNoPages()
	{
		var page = PagedList<int>.Create([], 1, 12);

		Assert.Empty(page);
		Assert.Equal(0, page.TotalPages);
		Assert.Equal(0, page.TotalCount);
		Assert.False(page.HasNextPage);
		Assert.False(page.HasPreviousPage);
	}

	private static List<int> Source(int count) => Enumerable.Range(1, count).ToList();
}

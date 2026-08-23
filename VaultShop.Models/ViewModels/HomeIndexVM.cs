using VaultShop.Models.Pagination;

namespace VaultShop.Models.ViewModels
{
	public class HomeIndexVM
	{
		public PagedList<Product> Products { get; set; } = PagedList<Product>.Create([], 1, 12);
		public IEnumerable<Product> FeaturedProducts { get; set; } = [];
		public IEnumerable<Category> Categories { get; set; } = [];
	}
}

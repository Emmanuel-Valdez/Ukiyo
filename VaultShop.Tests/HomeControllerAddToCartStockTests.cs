using System.Linq.Expressions;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using VaultShop.DataAccess.Repository.IRepository;
using VaultShop.Models;
using VaultShop.Web.Areas.Customer.Controllers;
using VaultShop.Web.Services.Pagination;

namespace VaultShop.Web.Tests
{
	public class HomeControllerAddToCartStockTests
	{
		[Fact]
		public void DetailsPost_ExceedsStock_RedirectsWithErrorAndDoesNotAdd()
		{
			var (controller, unitOfWorkMock, cartMock) = CreateController();
			cartMock.Setup(c => c.Get(It.IsAny<Expression<Func<ShoppingCart, bool>>>(), null, false))
				.Returns(new ShoppingCart { ProductId = 1, Count = 2 });

			var result = controller.Details(new ShoppingCart { ProductId = 1, Count = 1 });

			var redirect = Assert.IsType<RedirectToActionResult>(result);
			Assert.Equal(nameof(HomeController.Details), redirect.ActionName);
			Assert.Equal("NotEnoughStock", controller.TempData["error"]);
			cartMock.Verify(c => c.Add(It.IsAny<ShoppingCart>()), Times.Never);
			cartMock.Verify(c => c.Update(It.IsAny<ShoppingCart>()), Times.Never);
			unitOfWorkMock.Verify(u => u.Save(), Times.Never);
		}

		private static (HomeController Controller, Mock<IUnitOfWork> UnitOfWorkMock, Mock<IShoppingCartRepository> CartMock) CreateController()
		{
			var unitOfWorkMock = new Mock<IUnitOfWork>();
			var productMock = new Mock<IProductRepository>();
			var cartMock = new Mock<IShoppingCartRepository>();
			unitOfWorkMock.SetupGet(u => u.Product).Returns(productMock.Object);
			unitOfWorkMock.SetupGet(u => u.ShoppingCart).Returns(cartMock.Object);
			productMock.Setup(p => p.Get(It.IsAny<Expression<Func<Product, bool>>>(), null, false))
				.Returns(new Product { Id = 1, StockQuantity = 2 });

			var localizerMock = new Mock<IStringLocalizer<HomeController>>();
			localizerMock.Setup(x => x[It.IsAny<string>()]).Returns((string name) => new LocalizedString(name, name));

			var httpContext = new DefaultHttpContext
			{
				User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, "user-1")], "TestAuth"))
			};

			var controller = new HomeController(
				NullLogger<HomeController>.Instance,
				unitOfWorkMock.Object,
				localizerMock.Object,
				Options.Create(new PaginationOptions()))
			{
				ControllerContext = new ControllerContext { HttpContext = httpContext },
				TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>())
			};

			return (controller, unitOfWorkMock, cartMock);
		}
	}
}

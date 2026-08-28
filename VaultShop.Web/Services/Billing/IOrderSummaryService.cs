using System.Security.Claims;
using VaultShop.Models.ViewModels;

namespace VaultShop.Web.Services.Billing
{
	public interface IOrderSummaryService
	{
		OrderSummaryViewModel? GetSummary(int orderId, ClaimsPrincipal user);
	}
}

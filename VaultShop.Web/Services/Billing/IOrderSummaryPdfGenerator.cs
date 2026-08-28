using VaultShop.Models.ViewModels;

namespace VaultShop.Web.Services.Billing
{
	public interface IOrderSummaryPdfGenerator
	{
		byte[] Generate(OrderSummaryViewModel summary);
	}
}

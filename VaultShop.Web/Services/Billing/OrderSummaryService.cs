using System.Security.Claims;
using VaultShop.DataAccess.Repository.IRepository;
using VaultShop.Models;
using VaultShop.Models.ViewModels;
using VaultShop.Utility;

namespace VaultShop.Web.Services.Billing
{
	public sealed class OrderSummaryService : IOrderSummaryService
	{
		private readonly IUnitOfWork _unitOfWork;

		public OrderSummaryService(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public OrderSummaryViewModel? GetSummary(int orderId, ClaimsPrincipal user)
		{
			var orderHeader = _unitOfWork.OrderHeader.Get(
				o => o.Id == orderId,
				includeProperties: "ApplicationUser,Company");

			if (orderHeader == null || !UserCanAccessOrder(orderHeader, user))
				return null;

			var orderDetails = _unitOfWork.OrderDetail.GetAll(
				d => d.OrderHeaderId == orderId,
				includeProperties: "Product");

			return MapToViewModel(orderHeader, orderDetails);
		}

		private static OrderSummaryViewModel MapToViewModel(OrderHeader o, IEnumerable<OrderDetail> details)
		{
			return new OrderSummaryViewModel
			{
				OrderId = o.Id,
				OrderDate = o.OrderDate,
				OrderStatus = o.OrderStatus,
				PaymentStatus = o.PaymentStatus,
				PaymentMethod = o.PaymentMethod,
				PaymentDueDate = o.PaymentDueDate,

				CustomerName = o.ApplicationUser?.Name ?? string.Empty,

				CompanyName = o.Company?.Name,
				RazonSocial = o.RazonSocialSnapshot,
				DomicilioFiscal = o.DomicilioFiscalSnapshot,
				Cuit = o.CuitSnapshot,

				ShippingName = o.Name,
				ShippingStreetAddress = o.StreetAddress,
				ShippingCity = o.City,
				ShippingState = o.State,
				ShippingPostalCode = o.PostalCode,
				ShippingPhoneNumber = o.PhoneNumber,

				Items = details.Select(d => new OrderSummaryItemViewModel
				{
					ProductName = d.Product?.Name ?? string.Empty,
					UnitPrice = d.Price,
					Quantity = d.Count,
				}).ToList(),

				OrderTotal = o.OrderTotal,
			};
		}

		private bool UserCanAccessOrder(OrderHeader orderHeader, ClaimsPrincipal user)
		{
			if (user.IsInRole(SD.Role_Admin) || user.IsInRole(SD.Role_Employee))
				return true;

			var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
			if (string.IsNullOrEmpty(userId))
				return false;

			if (orderHeader.ApplicationUserId == userId && orderHeader.CompanyId.GetValueOrDefault() == 0)
				return true;

			var currentUser = _unitOfWork.ApplicationUser.Get(u => u.Id == userId);
			return currentUser?.CompanyId.GetValueOrDefault() > 0 && orderHeader.CompanyId == currentUser.CompanyId;
		}
	}
}

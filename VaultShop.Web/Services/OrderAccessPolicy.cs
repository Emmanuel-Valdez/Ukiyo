using System.Security.Claims;
using VaultShop.DataAccess.Repository.IRepository;
using VaultShop.Models;
using VaultShop.Utility;

namespace VaultShop.Web.Services;

public sealed class OrderAccessPolicy
{
	private readonly IUnitOfWork _unitOfWork;

	public OrderAccessPolicy(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

	public bool CanAccess(OrderHeader orderHeader, ClaimsPrincipal user)
	{
		// ponytail: Employee is admin-equivalent for orders by design; scope here if that changes.
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

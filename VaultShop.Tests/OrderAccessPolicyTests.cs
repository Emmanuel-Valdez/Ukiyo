using System.Linq.Expressions;
using System.Security.Claims;
using Moq;
using VaultShop.DataAccess.Repository.IRepository;
using VaultShop.Models;
using VaultShop.Utility;
using VaultShop.Web.Services;
using Xunit;

namespace VaultShop.Web.Tests
{
	public class OrderAccessPolicyTests
	{
		[Fact]
		public void Admin_CanAccessAnyOrder()
		{
			var policy = CreatePolicy(out var uow);
			var order = new OrderHeader { ApplicationUserId = "other", CompanyId = 0 };

			Assert.True(policy.CanAccess(order, CreatePrincipal("admin-1", SD.Role_Admin)));
			uow.ApplicationUserMock.Verify(x => x.Get(It.IsAny<Expression<Func<ApplicationUser, bool>>>(), It.IsAny<string?>(), It.IsAny<bool>()), Times.Never);
		}

		[Fact]
		public void Employee_CanAccessAnyOrder()
		{
			var policy = CreatePolicy(out var uow);
			var order = new OrderHeader { ApplicationUserId = "other", CompanyId = 99 };

			Assert.True(policy.CanAccess(order, CreatePrincipal("emp-1", SD.Role_Employee)));
			uow.ApplicationUserMock.Verify(x => x.Get(It.IsAny<Expression<Func<ApplicationUser, bool>>>(), It.IsAny<string?>(), It.IsAny<bool>()), Times.Never);
		}

		[Fact]
		public void Anonymous_ReturnsFalse()
		{
			var policy = CreatePolicy(out _);
			var order = new OrderHeader { ApplicationUserId = "other", CompanyId = 0 };

			Assert.False(policy.CanAccess(order, new ClaimsPrincipal(new ClaimsIdentity())));
		}

		[Fact]
		public void Customer_OwnOrder_ReturnsTrue()
		{
			var policy = CreatePolicy(out _);
			var order = new OrderHeader { ApplicationUserId = "user-1", CompanyId = 0 };

			Assert.True(policy.CanAccess(order, CreatePrincipal("user-1")));
		}

		[Fact]
		public void Customer_ForeignOrder_ReturnsFalse()
		{
			var policy = CreatePolicy(out _);
			var order = new OrderHeader { ApplicationUserId = "user-2", CompanyId = 0 };

			Assert.False(policy.CanAccess(order, CreatePrincipal("user-1")));
		}

		[Fact]
		public void CompanyUser_SameCompany_ReturnsTrue()
		{
			var policy = CreatePolicy(out var uow, new ApplicationUser { Id = "user-1", CompanyId = 7 });
			var order = new OrderHeader { ApplicationUserId = "x", CompanyId = 7 };

			Assert.True(policy.CanAccess(order, CreatePrincipal("user-1")));
		}

		[Fact]
		public void CompanyUser_CrossCompany_ReturnsFalse()
		{
			var policy = CreatePolicy(out var uow, new ApplicationUser { Id = "user-1", CompanyId = 9 });
			var order = new OrderHeader { ApplicationUserId = "x", CompanyId = 7 };

			Assert.False(policy.CanAccess(order, CreatePrincipal("user-1")));
		}

		[Fact]
		public void CompanyUser_WithoutCompanyId_CannotAccessForeignCompanyOrder()
		{
			var policy = CreatePolicy(out var uow, new ApplicationUser { Id = "user-1", CompanyId = null });
			var order = new OrderHeader { ApplicationUserId = "user-2", CompanyId = 7 };

			Assert.False(policy.CanAccess(order, CreatePrincipal("user-1")));
		}

		private static OrderAccessPolicy CreatePolicy(out TestUnitOfWork uow, ApplicationUser? resolvedUser = null)
		{
			uow = new TestUnitOfWork();
			if (resolvedUser != null)
			{
				uow.ApplicationUserMock
					.Setup(x => x.Get(It.IsAny<Expression<Func<ApplicationUser, bool>>>(), It.IsAny<string?>(), It.IsAny<bool>()))
					.Returns(resolvedUser);
			}

			return new OrderAccessPolicy(uow.Mock.Object);
		}

		private static ClaimsPrincipal CreatePrincipal(string userId, string? role = null)
		{
			var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, userId) };
			if (role != null)
				claims.Add(new Claim(ClaimTypes.Role, role));
			return new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
		}

		private sealed class TestUnitOfWork
		{
			public Mock<IUnitOfWork> Mock { get; } = new();
			public Mock<IApplicationUserRepository> ApplicationUserMock { get; } = new();

			public TestUnitOfWork()
			{
				Mock.Setup(x => x.ApplicationUser).Returns(ApplicationUserMock.Object);
			}
		}
	}
}

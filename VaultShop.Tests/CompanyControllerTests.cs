using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Localization;
using Moq;
using VaultShop.DataAccess.Repository.IRepository;
using VaultShop.Models;
using VaultShop.Utility;
using VaultShop.Web.Areas.Admin.Controllers;
using Xunit;

namespace VaultShop.Web.Tests
{
	public class CompanyControllerTests
	{
		[Fact]
		public void Upsert_PostCreate_WithValidModel_SavesAndRedirects()
		{
			var uow = CreateUnitOfWork();
			var controller = CreateController(uow);

			var result = controller.Upsert(CreateValidCompany(id: 0));

			var redirect = Assert.IsType<RedirectToActionResult>(result);
			Assert.Equal("Index", redirect.ActionName);
			uow.CompanyMock.Verify(x => x.Add(It.IsAny<Company>()), Times.Once);
			uow.Mock.Verify(x => x.Save(), Times.Once);
			uow.CompanyMock.Verify(x => x.Update(It.IsAny<Company>()), Times.Never);
		}

		[Fact]
		public void Upsert_PostEdit_WithValidModel_UpdatesAndRedirects()
		{
			var existing = CreateValidCompany(id: 5);
			var uow = CreateUnitOfWork(existing);
			var controller = CreateController(uow);

			var posted = CreateValidCompany(id: 5);
			posted.RazonSocial = "Razón Social Editada";

			var result = controller.Upsert(posted);

			Assert.IsType<RedirectToActionResult>(result);
			uow.CompanyMock.Verify(x => x.Update(It.IsAny<Company>()), Times.Once);
			uow.Mock.Verify(x => x.Save(), Times.Once);
			uow.CompanyMock.Verify(x => x.Add(It.IsAny<Company>()), Times.Never);
		}

		[Fact]
		public void Upsert_PostWithModelErrors_ReturnsViewAndDoesNotPersist()
		{
			var uow = CreateUnitOfWork();
			var controller = CreateController(uow);
			controller.ModelState.AddModelError("RazonSocial", "Required");

			var result = controller.Upsert(CreateValidCompany(id: 0));

			Assert.IsType<ViewResult>(result);
			uow.CompanyMock.Verify(x => x.Add(It.IsAny<Company>()), Times.Never);
			uow.CompanyMock.Verify(x => x.Update(It.IsAny<Company>()), Times.Never);
			uow.Mock.Verify(x => x.Save(), Times.Never);
		}

		[Fact]
		public void Company_WithMissingFiscalFields_FailsValidation()
		{
			var company = CreateValidCompany(id: 1);
			company.RazonSocial = string.Empty;

			var results = Validate(company);

			Assert.Contains(results, r => r.MemberNames.Contains(nameof(Company.RazonSocial)));
		}

		[Fact]
		public void Company_WithAllRequiredFields_PassesValidation()
		{
			var company = CreateValidCompany(id: 1);

			var results = Validate(company);

			Assert.Empty(results);
		}

		private static Company CreateValidCompany(int id)
		{
			return new Company
			{
				Id = id,
				Name = "Textiles SA",
				StreetAddress = "Av. Corrientes 1234",
				City = "CABA",
				State = "CABA",
				PostalCode = "1043",
				PhoneNumber = "+54 11 8765-4321",
				RazonSocial = "Textiles SA SRL",
				DomicilioFiscal = "Av. Corrientes 1234, CABA",
				Cuit = "30-71234567-9"
			};
		}

		private static List<ValidationResult> Validate(Company company)
		{
			var results = new List<ValidationResult>();
			var context = new ValidationContext(company);
			Validator.TryValidateObject(company, context, results, validateAllProperties: true);
			return results;
		}

		private static CompanyController CreateController(TestUnitOfWork uow)
		{
			var localizer = new Mock<IStringLocalizer<CompanyController>>();
			localizer.Setup(x => x[It.IsAny<string>()]).Returns((string name) => new LocalizedString(name, name));
			var controller = new CompanyController(uow.Mock.Object, localizer.Object);
			controller.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
			return controller;
		}

		private static TestUnitOfWork CreateUnitOfWork(Company? existing = null)
		{
			var uow = new TestUnitOfWork();
			if (existing != null)
			{
				uow.CompanyMock
					.Setup(x => x.Get(It.IsAny<System.Linq.Expressions.Expression<Func<Company, bool>>>(), It.IsAny<string?>(), It.IsAny<bool>()))
					.Returns(existing);
			}

			return uow;
		}

		private sealed class TestUnitOfWork
		{
			public Mock<IUnitOfWork> Mock { get; } = new();
			public Mock<ICompanyRepository> CompanyMock { get; } = new();

			public TestUnitOfWork()
			{
				Mock.Setup(x => x.Company).Returns(CompanyMock.Object);
			}
		}
	}
}

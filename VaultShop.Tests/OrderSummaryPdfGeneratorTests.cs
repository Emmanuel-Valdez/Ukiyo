using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Moq;
using VaultShop.Models.ViewModels;
using VaultShop.Web.Services.Billing;
using VaultShop.Web.Services.Branding;

namespace VaultShop.Web.Tests
{
	public class OrderSummaryPdfGeneratorTests
	{
		[Fact]
		public void Generate_WithValidSummary_ReturnsPdfWithMagicHeader()
		{
			var localizerMock = new Mock<IStringLocalizer<OrderSummaryPdfGenerator>>();
			localizerMock
				.Setup(x => x[It.IsAny<string>()])
				.Returns((string name) => new LocalizedString(name, name));

			var branding = Options.Create(new BrandingOptions { PublicName = "TestStore" });
			var generator = new OrderSummaryPdfGenerator(localizerMock.Object, branding);
			var summary = CreateSampleSummary();

			var pdf = generator.Generate(summary);

			Assert.NotNull(pdf);
			Assert.True(pdf.Length > 100, "Generated PDF should contain more than a trivial number of bytes.");
			Assert.Equal((byte)'%', pdf[0]);
			Assert.Equal((byte)'P', pdf[1]);
			Assert.Equal((byte)'D', pdf[2]);
			Assert.Equal((byte)'F', pdf[3]);
		}

		private static OrderSummaryViewModel CreateSampleSummary()
		{
			return new OrderSummaryViewModel
			{
				OrderId = 42,
				OrderDate = new DateTime(2026, 8, 20, 10, 0, 0, DateTimeKind.Utc),
				OrderStatus = "Pending",
				PaymentStatus = "Pending",
				PaymentMethod = "Stripe",
				CustomerName = "Juan Perez",
				ShippingName = "Juan Perez",
				ShippingStreetAddress = "Calle Falsa 123",
				ShippingCity = "Buenos Aires",
				ShippingState = "Buenos Aires",
				ShippingPostalCode = "1234",
				ShippingPhoneNumber = "+54 11 1234-5678",
				Items =
				[
					new() { ProductName = "Remera B�sica", UnitPrice = 1500m, Quantity = 2 },
					new() { ProductName = "Gorra Classic", UnitPrice = 800m, Quantity = 1 }
				],
				OrderTotal = 3800m
			};
		}
	}
}

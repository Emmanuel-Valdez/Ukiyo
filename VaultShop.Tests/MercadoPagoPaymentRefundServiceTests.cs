using System.Net;
using System.Net.Http.Headers;
using System.Text;
using VaultShop.Web.Services.Payments;

namespace VaultShop.Web.Tests
{
	public class MercadoPagoPaymentRefundServiceTests
	{
		[Fact]
		public async Task RefundPaymentIntentAsync_PostsToCorrectEndpoint()
		{
			var handler = new StubHttpMessageHandler((request, _) =>
			{
				Assert.Equal(HttpMethod.Post, request.Method);
				Assert.Equal("https://api.mercadopago.com/v1/payments/payment_mp/refunds", request.RequestUri?.ToString());
				Assert.Equal("Bearer", request.Headers.Authorization?.Scheme);
				Assert.Equal("test-token", request.Headers.Authorization?.Parameter);
				Assert.Equal("{}", request.Content!.ReadAsStringAsync().GetAwaiter().GetResult());

				return new HttpResponseMessage(HttpStatusCode.OK);
			});

			var service = CreateService(handler);

			await service.RefundPaymentIntentAsync("payment_mp");
		}

		[Fact]
		public async Task RefundPaymentIntentAsync_NonSuccess_ThrowsWithStatusAndBody()
		{
			var handler = new StubHttpMessageHandler((_, _) => new HttpResponseMessage(HttpStatusCode.BadRequest)
			{
				Content = new StringContent("{\"error\":\"invalid\"}", Encoding.UTF8, "application/json")
			});

			var service = CreateService(handler);

			var ex = await Assert.ThrowsAsync<HttpRequestException>(() => service.RefundPaymentIntentAsync("payment_mp"));
			Assert.Contains("400", ex.Message);
			Assert.Contains("invalid", ex.Message);
		}

		[Fact]
		public async Task RefundPaymentIntentAsync_Success_NoThrow()
		{
			var handler = new StubHttpMessageHandler((_, _) => new HttpResponseMessage(HttpStatusCode.OK)
			{
				Content = new StringContent("{\"id\":123,\"status\":\"approved\"}", Encoding.UTF8, "application/json")
			});

			var service = CreateService(handler);

			await service.RefundPaymentIntentAsync("payment_mp");
		}

		private static MercadoPagoPaymentRefundService CreateService(HttpMessageHandler handler)
		{
			var httpClient = new HttpClient(handler)
			{
				BaseAddress = new Uri("https://api.mercadopago.com")
			};
			httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "test-token");
			return new MercadoPagoPaymentRefundService(new StubHttpClientFactory(httpClient));
		}
	}
}

using System.Text;
using Stripe;

namespace VaultShop.Web.Services.Payments
{
	public interface IPaymentRefundService
	{
		Task RefundPaymentIntentAsync(string paymentIntentId);
	}

	public sealed class StripePaymentRefundService : IPaymentRefundService
	{
		private readonly RefundService _refundService = new();

		public async Task RefundPaymentIntentAsync(string paymentIntentId)
		{
			await _refundService.CreateAsync(new RefundCreateOptions
			{
				Reason = RefundReasons.RequestedByCustomer,
				PaymentIntent = paymentIntentId
			});
		}
	}

	public sealed class MercadoPagoPaymentRefundService : IPaymentRefundService
	{
		private readonly IHttpClientFactory _httpClientFactory;

		public MercadoPagoPaymentRefundService(IHttpClientFactory httpClientFactory)
		{
			_httpClientFactory = httpClientFactory;
		}

		public async Task RefundPaymentIntentAsync(string paymentIntentId)
		{
			var client = MercadoPagoHttp.CreateConfiguredClient(_httpClientFactory);
			// ponytail: 30s cap, no retry - Mercado Pago has no idempotency key, retrying a refund POST risks double-refund.
			client.Timeout = TimeSpan.FromSeconds(30);
			using var response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Post, $"/v1/payments/{Uri.EscapeDataString(paymentIntentId)}/refunds")
			{
				Content = new StringContent("{}", Encoding.UTF8, "application/json")
			});
			var body = await response.Content.ReadAsStringAsync();
			MercadoPagoHttp.EnsureSuccess(response, body, "refund payment");
		}
	}
}

using System.Text;
using Stripe;

namespace VaultShop.Web.Services.Payments
{
	public interface IPaymentRefundService
	{
		void RefundPaymentIntent(string paymentIntentId);
	}

	public sealed class StripePaymentRefundService : IPaymentRefundService
	{
		private readonly RefundService _refundService = new();

		public void RefundPaymentIntent(string paymentIntentId)
		{
			_refundService.Create(new RefundCreateOptions
			{
				Reason = RefundReasons.RequestedByCustomer,
				PaymentIntent = paymentIntentId
			});
		}
	}

	public sealed class MercadoPagoRefundService : IPaymentRefundService
	{
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly ILogger<MercadoPagoRefundService> _logger;

		public MercadoPagoRefundService(IHttpClientFactory httpClientFactory, ILogger<MercadoPagoRefundService> logger)
		{
			_httpClientFactory = httpClientFactory;
			_logger = logger;
		}

		public void RefundPaymentIntent(string paymentIntentId)
		{
			var client = _httpClientFactory.CreateClient(MercadoPagoPaymentSessionService.HttpClientName);
			// ponytail: 30s cap, no retry - Mercado Pago has no idempotency key, retrying a refund POST risks double-refund.
			client.Timeout = TimeSpan.FromSeconds(30);
			using var response = client.SendAsync(new HttpRequestMessage(HttpMethod.Post, $"/v1/payments/{Uri.EscapeDataString(paymentIntentId)}/refunds")
			{
				Content = new StringContent("{}", Encoding.UTF8, "application/json")
			}).GetAwaiter().GetResult();
			var body = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
			if (!response.IsSuccessStatusCode)
			{
				_logger.LogError("Mercado Pago refund failed with status {StatusCode}: {Body}", (int)response.StatusCode, body);
			}
			response.EnsureSuccessStatusCode();
		}
	}
}

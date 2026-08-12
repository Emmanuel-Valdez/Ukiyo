using System.Net;
using System.Net.Http.Headers;

namespace VaultShop.Web.Services.Payments
{
	internal static class MercadoPagoHttp
	{
		internal const string HttpClientName = "MercadoPago";
		internal const string IdempotencyKeyHeader = "X-Idempotency-Key";

		internal static void AddIdempotencyKey(HttpRequestMessage request)
		{
			request.Headers.TryAddWithoutValidation(IdempotencyKeyHeader, Guid.NewGuid().ToString("N"));
		}

		internal static HttpClient CreateConfiguredClient(IHttpClientFactory httpClientFactory)
		{
			var client = httpClientFactory.CreateClient(HttpClientName);
			if (client.DefaultRequestHeaders.Authorization is null || string.IsNullOrWhiteSpace(client.DefaultRequestHeaders.Authorization.Parameter))
			{
				throw new InvalidOperationException("Missing required Payments:MercadoPagoAccessToken configuration.");
			}

			return client;
		}

		internal static void EnsureSuccess(HttpResponseMessage response, string body, string operation)
		{
			if (response.IsSuccessStatusCode)
			{
				return;
			}

			throw new HttpRequestException($"Mercado Pago {operation} failed with status code {(int)response.StatusCode}: {body}", null, response.StatusCode);
		}
	}
}

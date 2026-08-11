namespace VaultShop.Web.Tests
{
	internal sealed class StubHttpClientFactory(HttpClient client) : IHttpClientFactory
	{
		public HttpClient CreateClient(string name)
		{
			Assert.Equal("MercadoPago", name);
			return client;
		}
	}

	internal sealed class StubHttpMessageHandler(Func<HttpRequestMessage, CancellationToken, HttpResponseMessage> handler) : HttpMessageHandler
	{
		protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			return Task.FromResult(handler(request, cancellationToken));
		}
	}
}

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using Moq;
using VaultShop.Web.Services.ImageStorage;

namespace VaultShop.Web.Tests;

public class StorageHealthCheckTests
{
	private static StorageHealthCheck CreateCheck(string provider, IServiceProvider services)
	{
		var config = new ConfigurationBuilder()
			.AddInMemoryCollection(new Dictionary<string, string?> { ["ImageStorage:Provider"] = provider })
			.Build();
		return new StorageHealthCheck(config, services);
	}

	private static ServiceProvider BuildMinioServices(Action<Mock<IMinioClient>>? setupClient = null, string bucketName = "test-bucket")
	{
		var mockClient = new Mock<IMinioClient>();
		setupClient?.Invoke(mockClient);

		var services = new ServiceCollection();
		services.AddSingleton(mockClient.Object);
		services.AddSingleton(Options.Create(new MinioStorageOptions { BucketName = bucketName }));
		return services.BuildServiceProvider();
	}

	[Fact]
	public async Task LocalProvider_ReturnsHealthy()
	{
		using var services = BuildMinioServices();
		var check = CreateCheck("Local", services);

		var result = await check.CheckHealthAsync(new HealthCheckContext());

		Assert.Equal(HealthStatus.Healthy, result.Status);
		Assert.Equal("Local storage", result.Description);
	}

	[Fact]
	public async Task NullProvider_DefaultsToHealthy()
	{
		using var services = BuildMinioServices();
		var check = CreateCheck("", services);

		var result = await check.CheckHealthAsync(new HealthCheckContext());

		Assert.Equal(HealthStatus.Healthy, result.Status);
	}

	[Fact]
	public async Task MinIO_NullClient_ReturnsUnhealthy()
	{
		using var services = new ServiceCollection().BuildServiceProvider();
		var check = CreateCheck("MINIO", services);

		var result = await check.CheckHealthAsync(new HealthCheckContext());

		Assert.Equal(HealthStatus.Unhealthy, result.Status);
		Assert.Contains("not configured", result.Description);
	}

	[Fact]
	public async Task MinIO_NullOptions_ReturnsUnhealthy()
	{
		var mockClient = new Mock<IMinioClient>();
		using var services = new ServiceCollection().AddSingleton(mockClient.Object).BuildServiceProvider();
		var check = CreateCheck("MINIO", services);

		var result = await check.CheckHealthAsync(new HealthCheckContext());

		Assert.Equal(HealthStatus.Unhealthy, result.Status);
		Assert.Contains("not configured", result.Description);
	}

	[Fact]
	public async Task MinIO_BucketExists_ReturnsHealthy()
	{
		using var services = BuildMinioServices(c =>
			c.Setup(x => x.BucketExistsAsync(It.IsAny<BucketExistsArgs>(), It.IsAny<CancellationToken>()))
			 .ReturnsAsync(true));
		var check = CreateCheck("MINIO", services);

		var result = await check.CheckHealthAsync(new HealthCheckContext());

		Assert.Equal(HealthStatus.Healthy, result.Status);
		Assert.Equal("MinIO reachable", result.Description);
	}

	[Fact]
	public async Task MinIO_BucketNotFound_ReturnsUnhealthy()
	{
		using var services = BuildMinioServices(
			c => c.Setup(x => x.BucketExistsAsync(It.IsAny<BucketExistsArgs>(), It.IsAny<CancellationToken>()))
				  .ReturnsAsync(false));
		var check = CreateCheck("minio", services);

		var result = await check.CheckHealthAsync(new HealthCheckContext());

		Assert.Equal(HealthStatus.Unhealthy, result.Status);
		Assert.Contains("test-bucket", result.Description);
		Assert.Contains("not found", result.Description);
	}

	[Fact]
	public async Task MinIO_ThrowsException_ReturnsUnhealthy()
	{
		using var services = BuildMinioServices(c =>
			c.Setup(x => x.BucketExistsAsync(It.IsAny<BucketExistsArgs>(), It.IsAny<CancellationToken>()))
			 .ThrowsAsync(new InvalidOperationException("connection refused")));
		var check = CreateCheck("MINIO", services);

		var result = await check.CheckHealthAsync(new HealthCheckContext());

		Assert.Equal(HealthStatus.Unhealthy, result.Status);
		Assert.Equal("MinIO unreachable", result.Description);
		Assert.NotNull(result.Exception);
	}
}

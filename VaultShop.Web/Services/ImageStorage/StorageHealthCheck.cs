using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;

namespace VaultShop.Web.Services.ImageStorage;

public sealed class StorageHealthCheck : IHealthCheck
{
	private readonly IConfiguration _configuration;
	private readonly IServiceProvider _services;

	public StorageHealthCheck(IConfiguration configuration, IServiceProvider services)
	{
		_configuration = configuration;
		_services = services;
	}

	public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
	{
		var provider = (_configuration["ImageStorage:Provider"] ?? "Local").Trim().ToUpperInvariant();
		if (provider != "MINIO")
		{
			return HealthCheckResult.Healthy("Local storage");
		}

		var client = _services.GetService<IMinioClient>();
		var options = _services.GetService<IOptions<MinioStorageOptions>>()?.Value;
		if (client is null || options is null)
		{
			return HealthCheckResult.Unhealthy("MinIO client is not configured");
		}

		try
		{
			var bucketExistsArgs = new BucketExistsArgs().WithBucket(options.BucketName);
			var exists = await client.BucketExistsAsync(bucketExistsArgs, cancellationToken);
			return exists
				? HealthCheckResult.Healthy("MinIO reachable")
				: HealthCheckResult.Unhealthy($"MinIO bucket '{options.BucketName}' not found");
		}
		catch (Exception ex)
		{
			return HealthCheckResult.Unhealthy("MinIO unreachable", ex);
		}
	}
}

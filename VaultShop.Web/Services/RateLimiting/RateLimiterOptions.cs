namespace VaultShop.Web.Services.RateLimiting;

public sealed class RateLimiterOptions
{
    public int GlobalPermitLimit { get; set; } = 100;
    public int GlobalWindowSeconds { get; set; } = 60;
    public int GlobalQueueLimit { get; set; } = 0;

    public int LoginPermitLimit { get; set; } = 10;
    public int LoginWindowSeconds { get; set; } = 60;
    public int LoginQueueLimit { get; set; } = 0;

    public TimeSpan GlobalWindow => TimeSpan.FromSeconds(GlobalWindowSeconds);
    public TimeSpan LoginWindow => TimeSpan.FromSeconds(LoginWindowSeconds);
}

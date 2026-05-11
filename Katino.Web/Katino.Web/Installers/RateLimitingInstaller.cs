using System.Threading.RateLimiting;

namespace Katino.Web.Installers;

public class RateLimitingInstaller : IInstaller
{
    public void InstallServices(IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection("RateLimiting");

        int globalPermitLimit = section.GetValue("GlobalPermitLimit", 100);
        int globalWindowSeconds = section.GetValue("GlobalWindowSeconds", 60);

        int authPermitLimit = section.GetValue("AuthPermitLimit", 5);
        int authWindowSeconds = section.GetValue("AuthWindowSeconds", 60);
        int authSegmentsPerWindow = section.GetValue("AuthSegmentsPerWindow", 4);

        services.AddRateLimiter(options =>
        {
            // 429 Too Many Requests
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            // Applied automatically to ALL endpoints — no attribute needed on controllers.
            // Use [DisableRateLimiting] on a specific endpoint to opt out.
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = globalPermitLimit,
                        Window = TimeSpan.FromSeconds(globalWindowSeconds),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0
                    }));

            // Per-IP sliding-window: each IP gets its own stricter counter for login brute-force protection
            options.AddPolicy("auth", context =>
                RateLimitPartition.GetSlidingWindowLimiter(
                    partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    factory: _ => new SlidingWindowRateLimiterOptions
                    {
                        PermitLimit = authPermitLimit,
                        Window = TimeSpan.FromSeconds(authWindowSeconds),
                        SegmentsPerWindow = authSegmentsPerWindow,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0
                    }));

            options.OnRejected = async (context, cancellationToken) =>
            {
                var logger = context.HttpContext.RequestServices
                    .GetRequiredService<ILogger<RateLimitingInstaller>>();

                var ip = context.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                var path = context.HttpContext.Request.Path;
                logger.LogWarning("Rate limit exceeded. IP: {IP}, Path: {Path}", ip, path);

                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    context.HttpContext.Response.Headers.RetryAfter =
                        ((int)retryAfter.TotalSeconds).ToString();
                }

                await context.HttpContext.Response.WriteAsJsonAsync(new
                {
                    error = "Too many requests. Please try again later.",
                    retryAfterSeconds = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var ra)
                        ? (int?)((int)ra.TotalSeconds) : null
                }, cancellationToken);
            };
        });
    }
}

using TegWallet.Application.Interfaces.Core;

namespace TegWallet.CoreApi.BackgroundServices;

public class RateLockCleanupService(
    IServiceProvider serviceProvider,
    ILogger<RateLockCleanupService> logger)
    : BackgroundService
{
    private readonly TimeSpan _cleanupInterval = TimeSpan.FromHours(6); // Run every 6 hours

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = serviceProvider.CreateScope();
                var rateLockRepository = scope.ServiceProvider.GetRequiredService<IRateLockRepository>();

                var cleanedCount = await rateLockRepository.CleanupExpiredRateLocksAsync();

                if (cleanedCount > 0)
                {
                    logger.LogInformation("Cleaned up {Count} expired rate locks", cleanedCount);
                }

                await Task.Delay(_cleanupInterval, stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred during rate lock cleanup");
                await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken); // Retry after 30 minutes on error
            }
        }
    }
}
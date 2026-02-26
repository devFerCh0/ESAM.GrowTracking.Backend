using ESAM.GrowTracking.Application.Commons.Settings;
using ESAM.GrowTracking.Application.Interfaces.Infrastructure.Services;
using ESAM.GrowTracking.Application.Interfaces.Percistence.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ESAM.GrowTracking.Application.Services
{
    public class PurgeExpiredTokensHostedService(ILogger<PurgeExpiredTokensHostedService> logger, IServiceScopeFactory scopeFactory, IOptions<CleanupSetting> options, IDateTimeService dateTimeService) : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
        private readonly ILogger<PurgeExpiredTokensHostedService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly CleanupSetting _cleanupSetting = options?.Value ?? throw new ArgumentNullException(nameof(options));
        private readonly IDateTimeService _dateTimeService = dateTimeService ?? throw new ArgumentNullException(nameof(dateTimeService));

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("PurgeExpiredTokensHostedService starting. InitialDelay={InitialDelay}, Interval={Interval}, BatchSize={BatchSize}", _cleanupSetting.InitialDelay, _cleanupSetting.Interval, 
                _cleanupSetting.BatchSize);
            try
            {
                if (_cleanupSetting.InitialDelay > TimeSpan.Zero)
                    await Task.Delay(_cleanupSetting.InitialDelay, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                return;
            }
            var asTracking = false;
            while (!stoppingToken.IsCancellationRequested)
            {
                var utcNow = _dateTimeService.UtcNow;
                try
                {
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var blacklistedAccessTokenTemporaryRepository = scope.ServiceProvider.GetRequiredService<IBlacklistedAccessTokenTemporaryRepository>();
                        var blacklistedAccessTokenPermanentRepository = scope.ServiceProvider.GetRequiredService<IBlacklistedAccessTokenPermanentRepository>();
                        var blacklistedRefreshTokenRepository = scope.ServiceProvider.GetRequiredService<IBlacklistedRefreshTokenRepository>();
                        var userSessionRefreshTokenRepository = scope.ServiceProvider.GetRequiredService<IUserSessionRefreshTokenRepository>();
                        _logger.LogInformation("Purge job started at {StartedAt}.", utcNow);
                        var tempDeleted = await blacklistedAccessTokenTemporaryRepository.PurgeExpiredBlacklistedAccessTokensTemporaryAsync(batchSize: _cleanupSetting.BatchSize, utcNow: utcNow, asTracking: asTracking, 
                            cancellationToken: stoppingToken);
                        _logger.LogInformation("Deleted {Count} BlacklistedAccessTokensTemporary entries.", tempDeleted);
                        var permDeleted = await blacklistedAccessTokenPermanentRepository.PurgeExpiredBlacklistedAccessTokensPermanentAsync(batchSize: _cleanupSetting.BatchSize, utcNow: utcNow, asTracking: asTracking,
                            cancellationToken: stoppingToken);
                        _logger.LogInformation("Deleted {Count} BlacklistedAccessTokensPermanent entries.", permDeleted);
                        var blacklistedRefreshDeleted = await blacklistedRefreshTokenRepository.PurgeExpiredBlacklistedRefreshTokensAsync(batchSize: _cleanupSetting.BatchSize, utcNow: utcNow, asTracking: asTracking,
                            cancellationToken: stoppingToken);
                        _logger.LogInformation("Deleted {Count} BlacklistedRefreshTokens entries.", blacklistedRefreshDeleted);
                        var userSessionRefreshDeleted = await userSessionRefreshTokenRepository.PurgeExpiredUserSessionRefreshTokensAsync(batchSize: _cleanupSetting.BatchSize, utcNow: utcNow, asTracking: asTracking,
                            cancellationToken: stoppingToken);
                        _logger.LogInformation("Deleted {Count} UserSessionRefreshTokens entries.", userSessionRefreshDeleted);
                        _logger.LogInformation("Purge job finished at {FinishedAt}. Totals: Temporary={Temp}, Permanent={Perm}, BlacklistedRefresh={BR}, UserSessionRefresh={USR}", utcNow, tempDeleted, permDeleted, 
                            blacklistedRefreshDeleted, userSessionRefreshDeleted);
                    };
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error ejecutando el job de purga de tokens expirada.");
                }
                try
                {
                    var elapsed = _dateTimeService.UtcNow - utcNow;
                    var delay = _cleanupSetting.Interval - elapsed;
                    if (delay < TimeSpan.Zero)
                        delay = TimeSpan.Zero;
                    await Task.Delay(delay, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
            _logger.LogInformation("PurgeExpiredTokensHostedService stopping.");
        }
    }
}
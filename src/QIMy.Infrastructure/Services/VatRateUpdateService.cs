using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QIMy.Core.Entities;
using QIMy.Core.Interfaces;
using QIMy.Infrastructure.Data;

namespace QIMy.Infrastructure.Services;

/// <summary>
/// Background service that automatically updates VAT rates from Vatlayer API
/// Runs once per day and logs all changes
/// </summary>
public class VatRateUpdateService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<VatRateUpdateService> _logger;
    private readonly TimeSpan _updateInterval = TimeSpan.FromDays(1); // Run daily

    public VatRateUpdateService(
        IServiceProvider serviceProvider,
        ILogger<VatRateUpdateService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("VatRateUpdateService started - will update VAT rates every {Interval}", _updateInterval);

        // Wait 1 minute after startup before first run
        await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await UpdateVatRatesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating VAT rates");
            }

            // Wait for next update interval
            await Task.Delay(_updateInterval, stoppingToken);
        }
    }

    private async Task UpdateVatRatesAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting VAT rates update from Vatlayer API");

        using var scope = _serviceProvider.CreateScope();
        var vatlayerService = scope.ServiceProvider.GetRequiredService<IVatlayerService>();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        try
        {
            // Fetch latest rates from Vatlayer
            var apiResponse = await vatlayerService.GetVatRatesAsync(cancellationToken);

            if (apiResponse == null || !apiResponse.Success)
            {
                _logger.LogWarning("Failed to fetch VAT rates from Vatlayer API");
                return;
            }

            var changesCount = 0;
            var now = DateTime.UtcNow;

            foreach (var kvp in apiResponse.Rates)
            {
                var countryCode = kvp.Key;
                var apiRate = kvp.Value;

                // Process Standard Rate
                changesCount += await ProcessRateAsync(
                    context,
                    countryCode,
                    apiRate.CountryName,
                    TaxRateType.Standard,
                    apiRate.StandardRate,
                    now,
                    changesCount,
                    cancellationToken);

                // Process Reduced Rates if available
                if (apiRate.ReducedRate.HasValue && apiRate.ReducedRate.Value > 0)
                {
                    changesCount += await ProcessRateAsync(
                        context,
                        countryCode,
                        apiRate.CountryName,
                        TaxRateType.Reduced,
                        apiRate.ReducedRate.Value,
                        now,
                        changesCount,
                        cancellationToken);
                }

                if (apiRate.SuperReducedRate.HasValue && apiRate.SuperReducedRate.Value > 0)
                {
                    changesCount += await ProcessRateAsync(
                        context,
                        countryCode,
                        apiRate.CountryName,
                        TaxRateType.SuperReduced,
                        apiRate.SuperReducedRate.Value,
                        now,
                        changesCount,
                        cancellationToken);
                }

                if (apiRate.ParkingRate.HasValue && apiRate.ParkingRate.Value > 0)
                {
                    changesCount += await ProcessRateAsync(
                        context,
                        countryCode,
                        apiRate.CountryName,
                        TaxRateType.Parking,
                        apiRate.ParkingRate.Value,
                        now,
                        changesCount,
                        cancellationToken);
                }
            }

            await context.SaveChangesAsync(cancellationToken);

            if (changesCount > 0)
            {
                _logger.LogWarning("⚠️ VAT RATES CHANGED: {Count} rate(s) updated! Check VatRateChangeLogs table.", changesCount);
                // TODO: Send email/Slack notification to admin
            }
            else
            {
                _logger.LogInformation("✅ VAT rates checked - no changes detected");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing VAT rates update");
            throw;
        }
    }

    private async Task<int> ProcessRateAsync(
        ApplicationDbContext context,
        string countryCode,
        string countryName,
        TaxRateType rateType,
        decimal newRate,
        DateTime now,
        int changesCount,
        CancellationToken cancellationToken)
    {
        // Find current active rate for this country and type
        var currentRate = await context.TaxRates
            .Where(tr => tr.CountryCode == countryCode)
            .Where(tr => tr.RateType == rateType)
            .Where(tr => tr.EffectiveUntil == null) // Currently active
            .FirstOrDefaultAsync(cancellationToken);

        if (currentRate == null)
        {
            // No existing rate - create new one
            var newTaxRate = new TaxRate
            {
                CountryCode = countryCode,
                CountryName = countryName,
                Name = $"{rateType} VAT ({countryCode})",
                Rate = newRate,
                RateType = rateType,
                EffectiveFrom = now,
                EffectiveUntil = null,
                IsDefault = rateType == TaxRateType.Standard,
                Source = "VatlayerAPI",
                Notes = $"Automatically created from Vatlayer API on {now:yyyy-MM-dd}"
            };

            context.TaxRates.Add(newTaxRate);

            // Log change
            context.VatRateChangeLogs.Add(new VatRateChangeLog
            {
                CountryCode = countryCode,
                CountryName = countryName,
                RateType = rateType,
                OldRate = null,
                NewRate = newRate,
                ChangeDate = now,
                Reason = "New rate added via Vatlayer API",
                Source = "VatlayerAPI",
                IsNotified = false
            });

            changesCount++;
            _logger.LogInformation("➕ New rate added: {CountryCode} {RateType} = {Rate}%", countryCode, rateType, newRate);
            return 1;
        }
        else if (currentRate.Rate != newRate)
        {
            // Rate changed - close old rate and create new one
            currentRate.EffectiveUntil = now.AddSeconds(-1); // End old rate
            currentRate.UpdatedAt = now;

            var newTaxRate = new TaxRate
            {
                CountryCode = countryCode,
                CountryName = countryName,
                Name = $"{rateType} VAT ({countryCode})",
                Rate = newRate,
                RateType = rateType,
                EffectiveFrom = now,
                EffectiveUntil = null,
                IsDefault = currentRate.IsDefault,
                BusinessId = currentRate.BusinessId,
                Source = "VatlayerAPI",
                Notes = $"Rate changed from {currentRate.Rate}% to {newRate}% via Vatlayer API"
            };

            context.TaxRates.Add(newTaxRate);

            // Log change
            context.VatRateChangeLogs.Add(new VatRateChangeLog
            {
                CountryCode = countryCode,
                CountryName = countryName,
                RateType = rateType,
                OldRate = currentRate.Rate,
                NewRate = newRate,
                ChangeDate = now,
                Reason = "Rate change detected by Vatlayer API",
                Source = "VatlayerAPI",
                IsNotified = false
            });

            changesCount++;
            _logger.LogWarning("🔄 Rate changed: {CountryCode} {RateType} from {OldRate}% to {NewRate}%",
                countryCode, rateType, currentRate.Rate, newRate);
            return 1;
        }

        // Rate unchanged, no action needed
        return 0;
    }
}

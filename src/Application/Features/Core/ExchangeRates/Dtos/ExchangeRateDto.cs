using TegWallet.Domain.Entity.Core;
using TegWallet.Domain.ValueObjects;

namespace TegWallet.Application.Features.Core.ExchangeRates.Dtos;

public class ExchangeRateDto
{
    public Guid Id { get; set; }
    public Currency BaseCurrency { get; set; } = null!;
    public Currency TargetCurrency { get; set; } = null!;
    public decimal MarketRate { get; set; }
    public decimal EffectiveRate { get; set; }
    public decimal Margin { get; set; }
    public RateType Type { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public bool IsActive { get; set; }
    public string Source { get; set; } = string.Empty;

    // Additional properties for better client information
    public string? ClientGroupName { get; set; }
    public string? ClientName { get; set; }
    public string RateTypeDescription { get; set; } = string.Empty;

    // Calculated properties
    public string CurrencyPair => $"{BaseCurrency.Code}/{TargetCurrency.Code}";
    public decimal MarginPercentage => Margin * 100;
    public bool IsExpired => EffectiveTo.HasValue && EffectiveTo.Value < DateTime.UtcNow;
    public bool IsFutureRate => EffectiveFrom > DateTime.UtcNow;
    public bool IsCurrent => IsActive && !IsExpired && !IsFutureRate;

    // Display-friendly properties
    public string DisplayEffectiveRate => EffectiveRate.ToString("N6");
    public string DisplayMarketRate => MarketRate.ToString("N6");
    public string DisplayMargin => $"{MarginPercentage:N2}%";
}
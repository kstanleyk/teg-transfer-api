
using TegWallet.Application.Features.Core.Currencies.Dto;

namespace TegWallet.Application.Features.Core.ExchangeRates.Dtos;

public class ClientWithExchangeRateDto
{
    public Guid ClientId { get; set; }
    public Guid WalletId { get; set; }

    // Client Group Info
    public Guid? ClientGroupId { get; set; }
    public string? ClientGroupName { get; set; }

    // Flattened Exchange Rate Info
    public Guid? ExchangeRateId { get; set; }
    public string? ExchangeRateType { get; set; }
    public CurrencyDto? ExchangeRateBaseCurrency { get; set; }
    public CurrencyDto? ExchangeRateTargetCurrency { get; set; }
    public decimal MarketRate { get; set; }
    public decimal EffectiveRate { get; set; }
    public decimal? Margin { get; set; }
    public DateTime? ExchangeRateEffectiveFrom { get; set; }
    public DateTime? ExchangeRateEffectiveTo { get; set; }
    public bool? IsExchangeRateActive { get; set; }
    public string? ExchangeRateSource { get; set; }
    public string? ExchangeRateDescription { get; set; }
    public string? ExchangeRateInverseDescription { get; set; }
    public string? ExchangeRateShortDescription { get; set; }
    public string? ExchangeRateInverseShortDescription { get; set; }

    public decimal? MarginPercentage => Margin * 100;
    public string CurrencyPair => $"{ExchangeRateBaseCurrency?.Code}/{ExchangeRateTargetCurrency?.Code}";
    public string DisplayEffectiveRate => EffectiveRate.ToString("N6");
    public string DisplayMarketRate => MarketRate.ToString("N6");
    public string DisplayMargin => $"{MarginPercentage:N2}%";
}

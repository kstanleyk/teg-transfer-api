using TegWallet.Domain.ValueObjects;

namespace TegWallet.Application.Features.Core.ExchangeRates.Dtos;

public class CurrencyPairEffectivenessDto
{
    public Currency BaseCurrency { get; set; } = null!;
    public Currency TargetCurrency { get; set; } = null!;
    public string CurrencyPair => $"{BaseCurrency.Code}/{TargetCurrency.Code}";
    public int TransactionCount { get; set; }
    public decimal TotalVolume { get; set; }
    public decimal AverageEffectiveRate { get; set; }
    public decimal AverageMargin { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal MarketSharePercentage { get; set; }
}
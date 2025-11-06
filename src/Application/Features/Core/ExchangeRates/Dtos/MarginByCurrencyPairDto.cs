using TegWallet.Domain.ValueObjects;

namespace TegWallet.Application.Features.Core.ExchangeRates.Dtos;

public class MarginByCurrencyPairDto
{
    public Currency BaseCurrency { get; set; } = null!;
    public Currency TargetCurrency { get; set; } = null!;
    public string CurrencyPair => $"{BaseCurrency.Code}/{TargetCurrency.Code}";
    public decimal AverageMargin { get; set; }
    public decimal MinMargin { get; set; }
    public decimal MaxMargin { get; set; }
    public decimal StandardDeviation { get; set; }
    public int TransactionCount { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal RevenuePerTransaction { get; set; }
    public decimal MarketSharePercentage { get; set; }
}
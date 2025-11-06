using TegWallet.Domain.Entity.Core;

namespace TegWallet.Application.Features.Core.ExchangeRates.Dtos;

public class MarginByRateTypeDto
{
    public RateType RateType { get; set; }
    public string RateTypeName { get; set; } = string.Empty;
    public decimal AverageMargin { get; set; }
    public decimal MinMargin { get; set; }
    public decimal MaxMargin { get; set; }
    public int TransactionCount { get; set; }
    public decimal TotalVolume { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal PercentageOfTotalRevenue { get; set; }
}
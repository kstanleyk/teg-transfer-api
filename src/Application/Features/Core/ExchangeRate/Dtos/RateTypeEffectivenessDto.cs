using TegWallet.Domain.Entity.Core;

namespace TegWallet.Application.Features.Core.ExchangeRate.Dtos;

public class RateTypeEffectivenessDto
{
    public RateType RateType { get; set; }
    public string RateTypeName { get; set; } = string.Empty;
    public int TransactionCount { get; set; }
    public decimal TotalVolume { get; set; }
    public int ClientCount { get; set; }
    public decimal PercentageOfTotalVolume { get; set; }
    public decimal AverageMargin { get; set; }
    public decimal TotalRevenue { get; set; }
}
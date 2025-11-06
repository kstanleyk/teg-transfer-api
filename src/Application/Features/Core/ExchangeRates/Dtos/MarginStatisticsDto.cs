namespace TegWallet.Application.Features.Core.ExchangeRates.Dtos;

public class MarginStatisticsDto
{
    public decimal OverallAverageMargin { get; set; }
    public decimal WeightedAverageMargin { get; set; } // Weighted by transaction volume
    public decimal MedianMargin { get; set; }
    public decimal MarginStandardDeviation { get; set; }
    public decimal MarginCoefficientOfVariation { get; set; } // Standard deviation / Mean
    public decimal TotalRevenue { get; set; }
    public int TotalTransactions { get; set; }
    public decimal AverageRevenuePerTransaction { get; set; }
    public decimal TopQuartileMargin { get; set; } // 75th percentile
    public decimal BottomQuartileMargin { get; set; } // 25th percentile
    public decimal MarginRange => TopQuartileMargin - BottomQuartileMargin;
}
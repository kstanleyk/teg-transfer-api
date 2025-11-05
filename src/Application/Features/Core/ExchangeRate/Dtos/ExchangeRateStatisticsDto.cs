namespace TegWallet.Application.Features.Core.ExchangeRate.Dtos;

public class ExchangeRateStatisticsDto
{
    public decimal AverageMarketRate { get; set; }
    public decimal AverageEffectiveRate { get; set; }
    public decimal MinMarketRate { get; set; }
    public decimal MaxMarketRate { get; set; }
    public decimal MinEffectiveRate { get; set; }
    public decimal MaxEffectiveRate { get; set; }
    public decimal StandardDeviation { get; set; }
    public int TotalChanges { get; set; }
    public decimal AverageMargin { get; set; }
}
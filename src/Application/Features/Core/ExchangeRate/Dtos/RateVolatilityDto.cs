namespace TegWallet.Application.Features.Core.ExchangeRate.Dtos;

public class RateVolatilityDto
{
    public decimal VolatilityPercentage { get; set; }
    public decimal AverageDailyChange { get; set; }
    public int HighVolatilityDays { get; set; }
    public decimal MaxSingleDayChange { get; set; }
    public decimal StabilityIndex { get; set; } // 0-100 scale
}
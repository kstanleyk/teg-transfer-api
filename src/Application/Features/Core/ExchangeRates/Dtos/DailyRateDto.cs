namespace TegWallet.Application.Features.Core.ExchangeRates.Dtos;

public class DailyRateDto
{
    public DateTime Date { get; set; }
    public decimal OpeningRate { get; set; }
    public decimal ClosingRate { get; set; }
    public decimal HighRate { get; set; }
    public decimal LowRate { get; set; }
    public decimal AverageRate { get; set; }
    public int TransactionCount { get; set; }
}
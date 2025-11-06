namespace TegWallet.Application.Features.Core.ExchangeRate.Dtos;

public class MarginTrendDto
{
    public DateTime Period { get; set; } // Could be daily, weekly, monthly
    public string PeriodLabel { get; set; } = string.Empty;
    public decimal AverageMargin { get; set; }
    public decimal MinMargin { get; set; }
    public decimal MaxMargin { get; set; }
    public int TransactionCount { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal MarginChangeFromPrevious { get; set; } // Percentage change
}
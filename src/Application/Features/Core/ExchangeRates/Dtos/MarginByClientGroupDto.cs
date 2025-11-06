namespace TegWallet.Application.Features.Core.ExchangeRates.Dtos;

public class MarginByClientGroupDto
{
    public Guid? ClientGroupId { get; set; }
    public string ClientGroupName { get; set; } = string.Empty;
    public int ClientCount { get; set; }
    public decimal AverageMargin { get; set; }
    public decimal MinMargin { get; set; }
    public decimal MaxMargin { get; set; }
    public int TransactionCount { get; set; }
    public decimal TotalVolume { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageTransactionSize { get; set; }
    public decimal RevenuePerClient { get; set; }
}
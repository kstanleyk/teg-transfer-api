namespace TegWallet.Application.Features.Core.ExchangeRate.Dtos;

public class OverallEffectivenessMetricsDto
{
    public int TotalTransactions { get; set; }
    public decimal TotalVolume { get; set; }
    public int TotalClients { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal OverallAverageMargin { get; set; }
    public decimal AverageTransactionSize { get; set; }
    public decimal IndividualRateUtilizationRate { get; set; } // % of clients using individual rates
    public decimal GroupRateUtilizationRate { get; set; } // % of clients using group rates
    public MostActiveCurrencyPairDto? MostActiveCurrencyPair { get; set; }
}
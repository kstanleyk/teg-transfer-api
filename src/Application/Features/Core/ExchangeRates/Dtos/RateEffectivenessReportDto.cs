namespace TegWallet.Application.Features.Core.ExchangeRates.Dtos;

public class RateEffectivenessReportDto
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public IReadOnlyList<RateTypeEffectivenessDto> RateTypeBreakdown { get; set; } = [];
    public IReadOnlyList<CurrencyPairEffectivenessDto> TopCurrencyPairs { get; set; } = [];
    public IReadOnlyList<ClientGroupRateUsageDto> ClientGroupUsage { get; set; } = [];
    public OverallEffectivenessMetricsDto OverallMetrics { get; set; } = new();
}
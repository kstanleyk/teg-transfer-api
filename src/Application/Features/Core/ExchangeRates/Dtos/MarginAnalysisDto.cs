namespace TegWallet.Application.Features.Core.ExchangeRates.Dtos;

public class MarginAnalysisDto
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public IReadOnlyList<MarginTrendDto> MarginTrends { get; set; } = [];
    public IReadOnlyList<MarginByCurrencyPairDto> MarginsByCurrencyPair { get; set; } = [];
    public IReadOnlyList<MarginByClientGroupDto> MarginsByClientGroup { get; set; } = [];
    public IReadOnlyList<MarginByRateTypeDto> MarginsByRateType { get; set; } = [];
    public MarginStatisticsDto OverallStatistics { get; set; } = new();
    public IReadOnlyList<AnomalyDetectionDto> MarginAnomalies { get; set; } = [];
}
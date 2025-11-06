namespace TegWallet.Application.Features.Core.ExchangeRate.Dtos;

public class AnomalyDetectionDto
{
    public DateTime DetectedAt { get; set; }
    public string AnomalyType { get; set; } = string.Empty; // "High Margin", "Low Margin", "Volatility Spike"
    public string Description { get; set; } = string.Empty;
    public decimal ExpectedValue { get; set; }
    public decimal ActualValue { get; set; }
    public decimal DeviationPercentage { get; set; }
    public string CurrencyPair { get; set; } = string.Empty;
    public Guid? ClientGroupId { get; set; }
    public string Severity { get; set; } = string.Empty; // "Low", "Medium", "High", "Critical"
}
namespace TegWallet.Application.Features.Core.ExchangeRate.Dtos;

public class ExchangeRateHistoryDto
{
    public DateTime ChangedAt { get; set; }
    public decimal PreviousMarketRate { get; set; }
    public decimal NewMarketRate { get; set; }
    public decimal PreviousEffectiveRate { get; set; }
    public decimal NewEffectiveRate { get; set; }
    public decimal MarketRateChange { get; set; }
    public decimal EffectiveRateChange { get; set; }
    public string ChangeType { get; set; } = string.Empty;
    public string ChangedBy { get; set; } = string.Empty;
}
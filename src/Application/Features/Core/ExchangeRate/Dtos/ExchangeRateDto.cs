using TegWallet.Domain.Entity.Core;
using TegWallet.Domain.ValueObjects;

namespace TegWallet.Application.Features.Core.ExchangeRate.Dtos;

public class ExchangeRateDto
{
    public Guid Id { get; set; }
    public Currency BaseCurrency { get; set; } = null!;
    public Currency TargetCurrency { get; set; } = null!;
    public decimal MarketRate { get; set; }
    public decimal EffectiveRate { get; set; }
    public decimal Margin { get; set; }
    public RateType Type { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public bool IsActive { get; set; }
    public string Source { get; set; } = string.Empty;
}
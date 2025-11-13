using TegWallet.Domain.Entity.Core;

namespace TegWallet.Application.Features.Core.ExchangeRates.Dtos;

public class ActiveExchangeRateDto
{
    public Guid Id { get; set; }
    public string BaseCurrencyCode { get; set; } = string.Empty;
    public string TargetCurrencyCode { get; set; } = string.Empty;
    public decimal BaseCurrencyValue { get; set; }
    public decimal TargetCurrencyValue { get; set; }
    public decimal EffectiveRate { get; set; }
    public decimal Margin { get; set; }
    public RateType Type { get; set; }
    public Guid? ClientId { get; set; }
    public Guid? ClientGroupId { get; set; }
    public string? ClientName { get; set; }
    public string? ClientGroupName { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public string Source { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
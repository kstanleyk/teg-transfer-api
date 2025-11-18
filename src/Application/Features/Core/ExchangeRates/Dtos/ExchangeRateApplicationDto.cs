using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TegWallet.Application.Features.Core.ExchangeRates.Dtos;

public record ExchangeRateApplicationDto
{
    public ExchangeRateDto? ExchangeRate { get; init; }
    public ExchangeRateTierDto? AppliedTier { get; init; }
    public string RateType { get; init; } = string.Empty; // "Hierarchical" or "Tiered"
    public bool IsTieredRate { get; init; }
    public decimal EffectiveRate { get; init; }
    public decimal EffectiveMargin { get; init; }
    public decimal MinimumAmount { get; init; }
}
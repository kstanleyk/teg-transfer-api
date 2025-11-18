using TegWallet.Domain.Abstractions;

namespace TegWallet.Domain.Entity.Core;

public class ExchangeRateTier : Entity<Guid>
{
    public Guid ExchangeRateId { get; private set; }
    public ExchangeRate ExchangeRate { get; private set; }

    public decimal MinAmount { get; private set; }
    public decimal MaxAmount { get; private set; }
    public decimal Rate { get; private set; } // Override rate for this tier
    public decimal Margin { get; private set; } // Override margin for this tier

    public DateTime CreatedAt { get; private set; }
    public string CreatedBy { get; private set; }

    // Domain methods
    public static ExchangeRateTier Create(
        Guid exchangeRateId,
        decimal minAmount,
        decimal maxAmount,
        decimal rate,
        decimal margin,
        string createdBy)
    {
        return new ExchangeRateTier
        {
            Id = Guid.NewGuid(),
            ExchangeRateId = exchangeRateId,
            MinAmount = minAmount,
            MaxAmount = maxAmount,
            Rate = rate,
            Margin = margin,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow
        };
    }
}
using Agrovet.Domain.Exceptions;

namespace Agrovet.Domain.ValueObjects;

public class MonetaryAmount
{
    public MonetaryAmount(decimal amount)
    {
        if (amount < 0)
            throw new DomainException("Amount cannot be negative");

        Value = amount;
    }

    public decimal Value { get; }

    public static implicit operator decimal(MonetaryAmount m) => m.Value;
}
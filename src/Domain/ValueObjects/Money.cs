namespace TegWallet.Domain.ValueObjects;

public record Money
{
    public decimal Amount { get; init; }
    public Currency Currency { get; init; } = null!;

    // EF Core requires a parameterless constructor
    private Money()
    {
        Amount = 0;
        // Currency will be set by EF Core
    }

    public Money(decimal amount, Currency currency)
    {
        if (amount < 0) throw new ArgumentException("Money amount cannot be negative");
        Amount = amount;
        Currency = currency ?? throw new ArgumentNullException(nameof(currency));
    }

    public static Money operator +(Money a, Money b)
    {
        if (a.Currency != b.Currency)
            throw new InvalidOperationException("Cannot add different currencies");
        return new Money(a.Amount + b.Amount, a.Currency);
    }

    public static Money operator -(Money a, Money b)
    {
        if (a.Currency != b.Currency)
            throw new InvalidOperationException("Cannot subtract different currencies");
        return new Money(a.Amount - b.Amount, a.Currency);
    }
}
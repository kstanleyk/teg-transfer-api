using System.Runtime.CompilerServices;
using Transfer.Domain.Exceptions;
using Transfer.Domain.ValueObjects;

namespace Transfer.Domain.Entity;

public static class DomainGuards
{
    public static void AgainstNullOrWhiteSpace(string? value,
        [CallerArgumentExpression("value")] string? paramName = null)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException($"{paramName} cannot be null or whitespace.");
    }

    public static void AgainstDefault<T>(T value, [CallerArgumentExpression("value")] string? paramName = null)
        where T : struct
    {
        if (value.Equals(default(T)))
            throw new DomainException($"{paramName} must be specified.");
    }

    public static void AgainstNull<T>(T? value, [CallerArgumentExpression("value")] string? paramName = null)
        where T : class
    {
        if (value is null)
            throw new DomainException($"{paramName} cannot be null.");
    }

    // Overload for value objects that are reference types (like Money)
    public static void AgainstNull(object? value, [CallerArgumentExpression("value")] string? paramName = null)
    {
        if (value is null)
            throw new DomainException($"{paramName} cannot be null.");
    }

    public static void AgainstCondition(bool condition, string message,
        [CallerArgumentExpression("condition")] string? conditionExpression = null)
    {
        if (condition)
            throw new DomainException($"{message} (Condition: {conditionExpression})");
    }

    // Additional guard methods that might be useful for your domain
    public static void AgainstNegative(decimal value, [CallerArgumentExpression("value")] string? paramName = null)
    {
        if (value < 0)
            throw new DomainException($"{paramName} cannot be negative.");
    }

    public static void AgainstNegativeOrZero(decimal value, [CallerArgumentExpression("value")] string? paramName = null)
    {
        if (value <= 0)
            throw new DomainException($"{paramName} must be greater than zero.");
    }

    public static void AgainstOutOfRange(decimal value, decimal min, decimal max,
        [CallerArgumentExpression("value")] string? paramName = null)
    {
        if (value < min || value > max)
            throw new DomainException($"{paramName} must be between {min} and {max}.");
    }

    public static void AgainstInvalidCurrency(Money money, Currency expectedCurrency,
        [CallerArgumentExpression("money")] string? paramName = null)
    {
        AgainstNull(money, paramName);

        if (money.Currency != expectedCurrency)
            throw new DomainException($"{paramName} must be in {expectedCurrency.Code} currency.");
    }
}

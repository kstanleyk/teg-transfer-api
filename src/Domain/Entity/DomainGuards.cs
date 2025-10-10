using System.Runtime.CompilerServices;
using Transfer.Domain.Exceptions;

namespace Transfer.Domain.Entity;

public static class DomainGuards
{
    public static void AgainstNullOrWhiteSpace(string? value,
        [CallerArgumentExpression("value")] string? paramName = null)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new DomainException($"{paramName} cannot be null or whitespace.");
    }

    public static void AgainstDefault<T>(T value, [CallerArgumentExpression("value")] string? paramName = null)
        where T : struct
    {
        if (value.Equals(default(T))) throw new DomainException($"{paramName} must be specified.");
    }

    public static void AgainstCondition(bool condition, string message,
        [CallerArgumentExpression("condition")] string? conditionExpression = null)
    {
        if (condition) throw new DomainException($"{message} (Condition: {conditionExpression})");
    }
}

using System.Text.RegularExpressions;
using Transfer.Domain.Exceptions;
using static System.Text.RegularExpressions.Regex;

namespace Transfer.Domain.ValueObjects;

public class EmailAddress : ValueObject
{
    private EmailAddress(string? value)
    {
        Value = value ?? string.Empty;

        if (!string.IsNullOrWhiteSpace(Value) && !IsValidEmail(Value))
        {
            throw new DomainException("Invalid email format.");
        }
    }

    public static EmailAddress Of(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return new EmailAddress(string.Empty);

        return new EmailAddress(value.Trim());
    }


    public string Value { get; }

    public bool IsEmpty => string.IsNullOrWhiteSpace(Value);

    public override string ToString() => Value;

    private static bool IsValidEmail(string email) =>
        IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
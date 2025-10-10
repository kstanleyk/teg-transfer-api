using System.Text.RegularExpressions;
using Transfer.Domain.Exceptions;

namespace Transfer.Domain.ValueObjects;

public class PhoneNumber
{
    public string CountryCode { get; }
    public string Number { get; }

    public string FullNumber => $"{CountryCode}{Number}";

    private PhoneNumber(string countryCode, string number)
    {
        CountryCode = countryCode;
        Number = number;
    }

    private PhoneNumber() { } // For ORM or serializers

    public static PhoneNumber Of(string number, string? countryCode = null)
    {
        countryCode ??= "+237"; // Default to Cameroon
        return new PhoneNumber(countryCode, number);
    }

    public static PhoneNumber Create(string countryCode, string number)
    {
        if (string.IsNullOrWhiteSpace(number))
            throw new DomainException("Phone number is required.");

        if (!Regex.IsMatch(number, @"^\d{6,15}$"))
            throw new DomainException("Phone number must be between 6 and 15 digits.");

        if (string.IsNullOrWhiteSpace(countryCode))
            throw new DomainException("Country code is required.");

        if (!Regex.IsMatch(countryCode, @"^\+\d{1,4}$"))
            throw new DomainException("Invalid country code format. Must start with '+' and contain digits only.");

        return new PhoneNumber(countryCode, number);
    }

    public bool IsCameroon => CountryCode == "+237";

    public override string ToString() => FullNumber;

    public override bool Equals(object? obj) =>
        obj is PhoneNumber other && FullNumber == other.FullNumber;

    public override int GetHashCode() => FullNumber.GetHashCode();
}


using TegWallet.Domain.Exceptions;

namespace TegWallet.Domain.ValueObjects;

public record Currency
{
    public string Code { get; init; } = string.Empty;
    public string Symbol { get; init; } = string.Empty;
    public int DecimalPlaces { get; init; }

    // EF Core requires a parameterless constructor
    protected Currency() { }

    public Currency(string code, string symbol, int decimalPlaces)
    {
        Code = code;
        Symbol = symbol;
        DecimalPlaces = decimalPlaces;
    }

    public static readonly Currency Usd = new("USD", "$", 2);
    public static readonly Currency Ngn = new("NGN", "₦", 2);
    public static readonly Currency Xof = new("XOF", "CFA", 2);
    public static readonly Currency Cny = new("CNY", "¥", 2);

    public static Currency FromCode(string code)
    {
        return code.ToUpper() switch
        {
            "USD" => Usd,
            "NGN" => Ngn,
            "XOF" => Xof,
            "CNY" => Cny,
            _ => throw new ArgumentException($"Unsupported currency code: {code}")
        };
    }

    public static bool TryFromCode(string code, out Currency currency)
    {
        try
        {
            currency = FromCode(code);
            return true;
        }
        catch (DomainException)
        {
            currency = null;
            return false;
        }
    }

    public static IReadOnlyList<Currency> All => [Usd, Ngn, Xof, Cny];
}
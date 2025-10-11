namespace TegWallet.Domain.ValueObjects;

public record Currency
{
    public string Code { get; init; } = string.Empty;
    public string Symbol { get; init; } = string.Empty;
    public int DecimalPlaces { get; init; }

    // EF Core requires a parameterless constructor
    private Currency() { }

    public Currency(string code, string symbol, int decimalPlaces)
    {
        Code = code;
        Symbol = symbol;
        DecimalPlaces = decimalPlaces;
    }

    public static readonly Currency USD = new("USD", "$", 2);
    public static readonly Currency NGN = new("NGN", "₦", 2);
    public static readonly Currency XOF = new("XOF", "CFA", 0);

    public static Currency FromCode(string code)
    {
        return code.ToUpper() switch
        {
            "USD" => USD,
            "NGN" => NGN,
            "XOF" => XOF,
            _ => throw new ArgumentException($"Unsupported currency code: {code}")
        };
    }
}
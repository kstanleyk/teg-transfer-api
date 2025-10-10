namespace Transfer.Domain.ValueObjects;

public record Currency
{
    public string Code { get; }
    public string Symbol { get; }
    public int DecimalPlaces { get; }

    public Currency(string code, string symbol, int decimalPlaces)
    {
        if (string.IsNullOrEmpty(code)) throw new ArgumentException("Currency code required");
        Code = code.ToUpper();
        Symbol = symbol;
        DecimalPlaces = decimalPlaces;
    }

    public static readonly Currency USD = new("USD", "$", 2);
    public static readonly Currency NGN = new("NGN", "₦", 2);
    public static readonly Currency XOF = new("XOF", "CFA", 0);
}
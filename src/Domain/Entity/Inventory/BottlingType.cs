using Transfer.Domain.ValueObjects;

namespace Transfer.Domain.Entity.Inventory;

public sealed class BottlingType : ValueObject
{
    public static readonly BottlingType OneLiter = new(1.0m, "1L");
    public static readonly BottlingType HalfLiter = new(0.5m, "0.5L");
    public static readonly BottlingType ThreeLiters = new(3.0m, "3L");
    public static readonly BottlingType FiveLiters = new(5.0m, "5L");

    public decimal SizeInLiters { get; }
    public string DisplayName { get; }

    private BottlingType(decimal sizeInLiters, string displayName)
    {
        SizeInLiters = sizeInLiters;
        DisplayName = displayName;
    }

    public static IReadOnlyCollection<BottlingType> All =>
    [
        OneLiter, HalfLiter, ThreeLiters, FiveLiters
    ];

    public static BottlingType FromSize(decimal size) =>
        All.FirstOrDefault(p => p.SizeInLiters == size)
        ?? throw new ArgumentException($"Invalid packaging size: {size}");

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return SizeInLiters;
        yield return DisplayName;
    }
}
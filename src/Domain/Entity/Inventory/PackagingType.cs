using Agrovet.Domain.ValueObjects;

namespace Agrovet.Domain.Entity.Inventory;

public sealed class PackagingType : ValueObject
{
    public string Id { get; }
    public BottlingType BottlingType { get; }
    public int UnitsPerBox { get; }
    public string DisplayName { get; }

    private PackagingType(string id, BottlingType bottlingType, int unitsPerBox, string displayName)
    {
        Id = id;
        BottlingType = bottlingType;
        UnitsPerBox = unitsPerBox;
        DisplayName = displayName;
    }

    public static readonly PackagingType OneLiterCarton = new("OneLiterCarton", BottlingType.OneLiter, 12, "1L / Box of 12");
    public static readonly PackagingType HalfLiterCarton = new("HalfLiterCarton", BottlingType.HalfLiter, 24, "0.5L / Box of 24");
    public static readonly PackagingType ThreeLitersCarton = new("ThreeLitersCarton", BottlingType.ThreeLiters, 5, "3L / Box of 5");
    public static readonly PackagingType FiveLitersCarton = new("FiveLitersCarton", BottlingType.FiveLiters, 3, "5L / Box of 3");
    public static readonly PackagingType OneLiterUnit = new("OneLiterUnit", BottlingType.OneLiter, 1, "1L / One bottle");
    public static readonly PackagingType HalfLiterUnit = new("HalfLiterUnit", BottlingType.HalfLiter, 1, "0.5L / One bottle");
    public static readonly PackagingType ThreeLitersUnit = new("ThreeLitersUnit", BottlingType.ThreeLiters, 1, "3L / One bottle");
    public static readonly PackagingType FiveLitersUnit = new("FiveLitersUnit", BottlingType.FiveLiters, 1, "5L / One bottle");

    public static IReadOnlyCollection<PackagingType> All =>
    [
        OneLiterCarton, HalfLiterCarton, ThreeLitersCarton, FiveLitersCarton,
        OneLiterUnit, HalfLiterUnit, ThreeLitersUnit, FiveLitersUnit
    ];

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Id;
        yield return BottlingType;
        yield return UnitsPerBox;
        yield return DisplayName;
    }

    // ✅ Step 2: Add lookup method
    public static PackagingType FromId(string id)
    {
        return All.FirstOrDefault(p =>
            string.Equals(p.Id, id, StringComparison.OrdinalIgnoreCase))
            ?? throw new ArgumentException($"Invalid packaging type id: {id}");
    }
}

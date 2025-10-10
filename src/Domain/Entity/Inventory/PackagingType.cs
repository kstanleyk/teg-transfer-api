using Transfer.Domain.ValueObjects;

namespace Transfer.Domain.Entity.Inventory;

public sealed class PackagingType : ValueObject
{
    public string Id { get; }
    public BottlingType BottlingType { get; }
    public int UnitsPerBox { get; }
    public string DisplayName { get; }
    public bool IsCarton => UnitsPerBox > 1;
    public PackagingType? UnitEquivalent { get; private set; }
    public PackagingType? CartonEquivalent { get; private set; }

    private PackagingType()
    {
        // Required by EF Core
        Id = null!;
        BottlingType = null!;
        DisplayName = null!;
    }

    private PackagingType(string id, BottlingType bottlingType, int unitsPerBox, string displayName)
    {
        Id = id;
        BottlingType = bottlingType;
        UnitsPerBox = unitsPerBox;
        DisplayName = displayName;
    }

    // Static instances
    public static readonly PackagingType OneLiterCarton;
    public static readonly PackagingType HalfLiterCarton;
    public static readonly PackagingType ThreeLitersCarton;
    public static readonly PackagingType FiveLitersCarton;
    public static readonly PackagingType OneLiterUnit;
    public static readonly PackagingType HalfLiterUnit;
    public static readonly PackagingType ThreeLitersUnit;
    public static readonly PackagingType FiveLitersUnit;

    static PackagingType()
    {
        // Create unit types first
        OneLiterUnit = new PackagingType("OneLiterUnit", BottlingType.OneLiter, 1, "1L / One bottle");
        HalfLiterUnit = new PackagingType("HalfLiterUnit", BottlingType.HalfLiter, 1, "0.5L / One bottle");
        ThreeLitersUnit = new PackagingType("ThreeLitersUnit", BottlingType.ThreeLiters, 1, "3L / One bottle");
        FiveLitersUnit = new PackagingType("FiveLitersUnit", BottlingType.FiveLiters, 1, "5L / One bottle");

        // Create carton types with unit equivalents
        OneLiterCarton = new PackagingType("OneLiterCarton", BottlingType.OneLiter, 12, "1L / Box of 12");
        HalfLiterCarton = new PackagingType("HalfLiterCarton", BottlingType.HalfLiter, 24, "0.5L / Box of 24");
        ThreeLitersCarton = new PackagingType("ThreeLitersCarton", BottlingType.ThreeLiters, 5, "3L / Box of 5");
        FiveLitersCarton = new PackagingType("FiveLitersCarton", BottlingType.FiveLiters, 3, "5L / Box of 3");

        // Set up bidirectional relationships
        OneLiterCarton.SetUnitEquivalent(OneLiterUnit);
        HalfLiterCarton.SetUnitEquivalent(HalfLiterUnit);
        ThreeLitersCarton.SetUnitEquivalent(ThreeLitersUnit);
        FiveLitersCarton.SetUnitEquivalent(FiveLitersUnit);

        OneLiterUnit.SetCartonEquivalent(OneLiterCarton);
        HalfLiterUnit.SetCartonEquivalent(HalfLiterCarton);
        ThreeLitersUnit.SetCartonEquivalent(ThreeLitersCarton);
        FiveLitersUnit.SetCartonEquivalent(FiveLitersCarton);
    }

    private void SetUnitEquivalent(PackagingType unitEquivalent)
    {
        UnitEquivalent = unitEquivalent;
    }

    private void SetCartonEquivalent(PackagingType cartonEquivalent)
    {
        CartonEquivalent = cartonEquivalent;
    }

    public static IReadOnlyCollection<PackagingType> All =>
    [
        OneLiterCarton, HalfLiterCarton, ThreeLitersCarton, FiveLitersCarton,
        OneLiterUnit, HalfLiterUnit, ThreeLitersUnit, FiveLitersUnit
    ];

    public bool CanUnboxTo(PackagingType targetPackaging)
    {
        if (!IsCarton) return false;
        return UnitEquivalent?.Id == targetPackaging.Id;
    }

    public PackagingType GetUnitEquivalent()
    {
        if (!IsCarton)
            throw new InvalidOperationException($"Packaging type {Id} is already a unit.");

        return UnitEquivalent ?? throw new InvalidOperationException($"No unit equivalent found for {Id}");
    }

    public PackagingType? GetCartonEquivalent()
    {
        if (IsCarton)
            throw new InvalidOperationException($"Packaging type {Id} is already a carton.");

        return CartonEquivalent;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Id;
        yield return BottlingType;
        yield return UnitsPerBox;
        yield return DisplayName;
    }

    public static PackagingType FromId(string id)
    {
        return All.FirstOrDefault(p =>
            string.Equals(p.Id, id, StringComparison.OrdinalIgnoreCase))
            ?? throw new ArgumentException($"Invalid packaging type id: {id}");
    }
}

//public sealed class PackagingType : ValueObject
//{
//    public string Id { get; }
//    public BottlingType BottlingType { get; }
//    public int UnitsPerBox { get; }
//    public string DisplayName { get; }

//    private PackagingType()
//    {
//        // Required by EF Core
//        Id = null!;
//        BottlingType = null!;
//        DisplayName = null!;
//    }

//    private PackagingType(string id, BottlingType bottlingType, int unitsPerBox, string displayName)
//    {
//        Id = id;
//        BottlingType = bottlingType;
//        UnitsPerBox = unitsPerBox;
//        DisplayName = displayName;
//    }

//    public static readonly PackagingType OneLiterCarton = new("OneLiterCarton", BottlingType.OneLiter, 12, "1L / Box of 12");
//    public static readonly PackagingType HalfLiterCarton = new("HalfLiterCarton", BottlingType.HalfLiter, 24, "0.5L / Box of 24");
//    public static readonly PackagingType ThreeLitersCarton = new("ThreeLitersCarton", BottlingType.ThreeLiters, 5, "3L / Box of 5");
//    public static readonly PackagingType FiveLitersCarton = new("FiveLitersCarton", BottlingType.FiveLiters, 3, "5L / Box of 3");
//    public static readonly PackagingType OneLiterUnit = new("OneLiterUnit", BottlingType.OneLiter, 1, "1L / One bottle");
//    public static readonly PackagingType HalfLiterUnit = new("HalfLiterUnit", BottlingType.HalfLiter, 1, "0.5L / One bottle");
//    public static readonly PackagingType ThreeLitersUnit = new("ThreeLitersUnit", BottlingType.ThreeLiters, 1, "3L / One bottle");
//    public static readonly PackagingType FiveLitersUnit = new("FiveLitersUnit", BottlingType.FiveLiters, 1, "5L / One bottle");

//    public static IReadOnlyCollection<PackagingType> All =>
//    [
//        OneLiterCarton, HalfLiterCarton, ThreeLitersCarton, FiveLitersCarton,
//        OneLiterUnit, HalfLiterUnit, ThreeLitersUnit, FiveLitersUnit
//    ];

//    protected override IEnumerable<object> GetEqualityComponents()
//    {
//        yield return Id;
//        yield return BottlingType;
//        yield return UnitsPerBox;
//        yield return DisplayName;
//    }

//    // ✅ Step 2: Add lookup method
//    public static PackagingType FromId(string id)
//    {
//        return All.FirstOrDefault(p =>
//            string.Equals(p.Id, id, StringComparison.OrdinalIgnoreCase))
//            ?? throw new ArgumentException($"Invalid packaging type id: {id}");
//    }
//}

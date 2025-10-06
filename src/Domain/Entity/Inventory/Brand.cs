using Agrovet.Domain.ValueObjects;

namespace Agrovet.Domain.Entity.Inventory;

public sealed class Brand : ValueObject
{
    public static readonly Brand Engwari = new("Engwari");
    public static readonly Brand Eposi = new("Eposi");
    public static readonly Brand Lum = new("Lum");

    public string Name { get; } = null!;

    private Brand() { } // EF Core

    public Brand(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Brand name cannot be empty", nameof(name));
        Name = name;
    }

    public static IReadOnlyCollection<Brand> All => [Engwari, Eposi, Lum];

    public static Brand FromName(string name) => All.FirstOrDefault(b => b.Name == name) ?? throw new ArgumentException($"Invalid brand name: {name}");


    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Name;
    }
}
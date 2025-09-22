using Agrovet.Domain.Abstractions;

namespace Agrovet.Domain.Entity.Inventory;

public class Item : Entity<string>
{
    public string Name { get; private set; } = null!;
    public string ShortDescription { get; private set; } = null!;
    public string BarCodeText { get; private set; } = null!;
    public string Brand { get; private set; } = null!;
    public string Category { get; private set; } = null!;
    public string Status { get; private set; } = null!;
    public double MinStock { get; private set; }
    public double MaxStock { get; private set; }
    public double ReorderLev { get; private set; }
    public double ReorderQtty { get; private set; }
    public Guid? PublicId { get; private set; }
    public DateTime CreatedOn { get; private set; }

    protected Item()
    {
    }

    public static Item Create(string name, string shortDescription, string barCodeText, string brand, string category,
        string status, double minStock, double maxStock, double reorderLev, double reorderQtty,
        DateTime? createdOn = null)
    {
        DomainGuards.AgainstNullOrWhiteSpace(name);
        DomainGuards.AgainstNullOrWhiteSpace(shortDescription);
        DomainGuards.AgainstNullOrWhiteSpace(barCodeText);
        DomainGuards.AgainstNullOrWhiteSpace(brand);
        DomainGuards.AgainstNullOrWhiteSpace(category);
        DomainGuards.AgainstNullOrWhiteSpace(status);

        ValidateStockValues(minStock, maxStock, reorderLev, reorderQtty);

        return new Item
        {
            Name = name,
            ShortDescription = shortDescription,
            BarCodeText = barCodeText,
            Brand = brand,
            Category = category,
            Status = status,
            MinStock = minStock,
            MaxStock = maxStock,
            ReorderLev = reorderLev,
            ReorderQtty = reorderQtty,
            CreatedOn = createdOn ?? DateTime.UtcNow
        };
    }

    private static void ValidateStockValues(double minStock, double maxStock, double reorderLev, double reorderQtty)
    {
        if (minStock < 0)
            throw new ArgumentOutOfRangeException(nameof(minStock), "Min stock cannot be negative.");
        if (maxStock < 0)
            throw new ArgumentOutOfRangeException(nameof(maxStock), "Max stock cannot be negative.");
        if (reorderLev < 0)
            throw new ArgumentOutOfRangeException(nameof(reorderLev), "Reorder level cannot be negative.");
        if (reorderQtty < 0)
            throw new ArgumentOutOfRangeException(nameof(reorderQtty), "Reorder quantity cannot be negative.");
        if (maxStock < minStock)
            throw new ArgumentException("Max stock cannot be less than min stock.");
        if (reorderLev > maxStock)
            throw new ArgumentException("Reorder level cannot exceed max stock.");
    }

    public void SetId(string id)
    {
        DomainGuards.AgainstNullOrWhiteSpace(id);
        Id = id;
    }

    public void SetPublicId(Guid publicId)
    {
        ArgumentNullException.ThrowIfNull(publicId);
        PublicId = publicId;
    }
}
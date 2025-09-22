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

    public void Update(Item item)
    {
        DomainGuards.AgainstNullOrWhiteSpace(item.Name);
        DomainGuards.AgainstNullOrWhiteSpace(item.ShortDescription);
        DomainGuards.AgainstNullOrWhiteSpace(item.BarCodeText);
        DomainGuards.AgainstNullOrWhiteSpace(item.Brand);
        DomainGuards.AgainstNullOrWhiteSpace(item.Category);
        DomainGuards.AgainstNullOrWhiteSpace(item.Status);

        ValidateStockValues(item.MinStock, item.MaxStock, item.ReorderLev, item.ReorderQtty);

        Name = item.Name;
        ShortDescription = item.ShortDescription;
        BarCodeText = item.BarCodeText;
        Brand = item.Brand;
        Category = item.Category;
        Status = item.Status;
        MinStock = item.MinStock;
        MaxStock = item.MaxStock;
        ReorderLev = item.ReorderLev;
        ReorderQtty = item.ReorderQtty;
    }

    public bool HasChanges(Item? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return false;

        return Name != other.Name ||
               ShortDescription != other.ShortDescription ||
               BarCodeText != other.BarCodeText ||
               Brand != other.Brand ||
               Category != other.Category ||
               Status != other.Status ||
               !MinStock.Equals(other.MinStock) ||
               !MaxStock.Equals(other.MaxStock) ||
               !ReorderLev.Equals(other.ReorderLev) ||
               !ReorderQtty.Equals(other.ReorderQtty);
    }
}
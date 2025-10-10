using System.Security.Cryptography;
using System.Text;
using Transfer.Domain.Abstractions;

namespace Transfer.Domain.Entity.Inventory;

public class Product : Entity<string>
{
    public string Name { get; private set; } = null!;
    public Brand Brand { get; private set; } = null!;
    public BottlingType BottlingType { get; private set; } = null!;
    public string Sku { get; private set; } = null!;
    public string Category { get; private set; } = null!;
    public string Status { get; private set; } = null!;
    public double MinStock { get; private set; }
    public double MaxStock { get; private set; }
    public double ReorderLev { get; private set; }
    public double ReorderQtty { get; private set; }

    protected Product() { }

    public static Product Create(Brand brand, BottlingType bottlingType, string category, string status,
        double minStock, double maxStock, double reorderLev, double reorderQtty,
        Func<Product, string>? skuGenerator = null, DateTime? createdOn = null)
    {
        DomainGuards.AgainstNullOrWhiteSpace(category);
        DomainGuards.AgainstNullOrWhiteSpace(status);

        ValidateStockValues(minStock, maxStock, reorderLev, reorderQtty);

        var item = new Product
        {
            Name = $"Palm Oil {brand.Name} {bottlingType.DisplayName}",
            Brand = brand,
            BottlingType = bottlingType,
            Category = category,
            Status = status,
            MinStock = minStock,
            MaxStock = maxStock,
            ReorderLev = reorderLev,
            ReorderQtty = reorderQtty,
            CreatedOn = createdOn ?? DateTime.UtcNow
        };

        if (skuGenerator is not null)
        {
            item.UpdateSku(skuGenerator);
        }

        return item;
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

    public void Update(Product product)
    {
        DomainGuards.AgainstNullOrWhiteSpace(product.Category);
        DomainGuards.AgainstNullOrWhiteSpace(product.Status);

        ValidateStockValues(product.MinStock, product.MaxStock, product.ReorderLev, product.ReorderQtty);

        Brand = product.Brand;
        BottlingType = product.BottlingType;
        Category = product.Category;
        Status = product.Status;
        MinStock = product.MinStock;
        MaxStock = product.MaxStock;
        ReorderLev = product.ReorderLev;
        ReorderQtty = product.ReorderQtty;
    }

    public void UpdateSku(Func<Product, string> skuGenerator)
    {
        if (skuGenerator == null)
            throw new ArgumentNullException(nameof(skuGenerator));

        Sku = skuGenerator(this);
    }

    public bool HasChanges(Product? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return false;

        return !Equals(Brand, other.Brand) ||
               !Equals(BottlingType, other.BottlingType) ||
               Category != other.Category ||
               Status != other.Status ||
               !MinStock.Equals(other.MinStock) ||
               !MaxStock.Equals(other.MaxStock) ||
               !ReorderLev.Equals(other.ReorderLev) ||
               !ReorderQtty.Equals(other.ReorderQtty) ||
               Sku != other.Sku;
    }
}

public static class SkuGenerators
{
    public static string Deterministic(Product product)
    {
        if (product == null) throw new ArgumentNullException(nameof(product));

        var brandPart = (product.Brand is { Name.Length: >= 3 } ? 
            product.Brand.Name[..3] : product.Brand.Name).ToUpper();

        var packagingPart = (product.BottlingType is { DisplayName.Length: >= 4 }
            ? product.BottlingType.DisplayName[..4]
            : product.BottlingType.DisplayName).ToUpper();

        // Generate a short, stable hash from the packaging + brand
        var hashPart = GetStableHash(product.BottlingType.DisplayName + product.Brand).ToString("X4"); // 4 hex chars

        //return $"{brandPart}-{categoryPart}-{barcodePart}-{hashPart}";
        return $"{brandPart}-{packagingPart}-{hashPart}";
    }

    private static int GetStableHash(string input)
    {
        // Consistent hash code using SHA256, truncated to int
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
        return BitConverter.ToInt32(bytes, 0);
    }
}


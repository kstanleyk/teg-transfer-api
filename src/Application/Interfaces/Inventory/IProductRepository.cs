using Transfer.Application.Features.Inventory.Product.Dtos;
using Transfer.Domain.Entity.Inventory;

namespace Transfer.Application.Interfaces.Inventory;

public interface IProductRepository : IRepository<Product, string>
{
    Task<ProductStockBalanceResponse[]> GetItemStockBalancesAsync();
}
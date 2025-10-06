using Agrovet.Application.Features.Inventory.Product.Dtos;
using Agrovet.Domain.Entity.Inventory;

namespace Agrovet.Application.Interfaces.Inventory;

public interface IProductRepository : IRepository<Product, string>
{
    Task<ProductStockBalanceResponse[]> GetItemStockBalancesAsync();
}
using Agrovet.Domain.Entity.Inventory;

namespace Agrovet.Application.Interfaces.Inventory;

public interface IProductCategoryRepository : IRepository<ProductCategory, string>
{
}
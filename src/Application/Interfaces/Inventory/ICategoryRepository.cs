using Transfer.Domain.Entity.Inventory;

namespace Transfer.Application.Interfaces.Inventory;

public interface ICategoryRepository : IRepository<Category, string>
{
}
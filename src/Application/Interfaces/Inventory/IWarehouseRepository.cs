using Transfer.Domain.Entity.Inventory;

namespace Transfer.Application.Interfaces.Inventory;

public interface IWarehouseRepository : IRepository<Warehouse, string>
{
}
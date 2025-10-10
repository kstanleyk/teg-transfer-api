using Transfer.Domain.Entity.Core;

namespace Transfer.Application.Interfaces.Inventory;

public interface IWarehouseRepository : IRepository<Warehouse, string>
{
}
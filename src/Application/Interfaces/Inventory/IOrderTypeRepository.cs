using Transfer.Domain.Entity.Inventory;

namespace Transfer.Application.Interfaces.Inventory;

public interface IOrderTypeRepository : IRepository<OrderType, string>
{
}
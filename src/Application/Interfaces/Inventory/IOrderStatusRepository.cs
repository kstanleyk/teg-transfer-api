using Transfer.Domain.Entity.Inventory;

namespace Transfer.Application.Interfaces.Inventory;

public interface IOrderStatusRepository : IRepository<OrderStatus, string>
{
}
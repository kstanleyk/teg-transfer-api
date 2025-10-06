using Agrovet.Domain.Entity.Inventory;

namespace Agrovet.Application.Interfaces.Inventory;

public interface IOrderStatusRepository : IRepository<OrderStatus, string>
{
}
using Agrovet.Domain.Entity.Inventory;

namespace Agrovet.Application.Interfaces.Inventory;

public interface IOrderTypeRepository : IRepository<OrderType, string>
{
}
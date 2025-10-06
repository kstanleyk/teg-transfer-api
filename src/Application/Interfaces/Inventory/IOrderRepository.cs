using Agrovet.Application.Helpers;
using Agrovet.Domain.Entity.Inventory;

namespace Agrovet.Application.Interfaces.Inventory;

public interface IOrderRepository : IRepository<Order, string>
{
    Task<RepositoryActionResult<Order>> ReceiveOrderAsync(Guid publicId, Order order);
}
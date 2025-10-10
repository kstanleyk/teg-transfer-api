using Transfer.Application.Helpers;
using Transfer.Domain.Entity.Inventory;

namespace Transfer.Application.Interfaces.Inventory;

public interface IOrderRepository : IRepository<Order, string>
{
    Task<RepositoryActionResult<Order>> ReceiveOrderAsync(Guid publicId, Order order);
}
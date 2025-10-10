using Transfer.Application.Helpers;
using Transfer.Domain.Entity.Inventory;

namespace Transfer.Application.Interfaces.Inventory;

public interface IOrderDetailRepository : IRepository<OrderDetail, string>
{
    Task<RepositoryActionResult<IEnumerable<OrderDetail>>> UpdateOrderDetailsAsync(string id,
        OrderDetail[] orderDetails);
}
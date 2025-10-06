using Agrovet.Application.Helpers;
using Agrovet.Domain.Entity.Inventory;

namespace Agrovet.Application.Interfaces.Inventory;

public interface IOrderDetailRepository : IRepository<OrderDetail, string>
{
    Task<RepositoryActionResult<IEnumerable<OrderDetail>>> UpdateOrderDetailsAsync(string id,
        OrderDetail[] orderDetails);
}
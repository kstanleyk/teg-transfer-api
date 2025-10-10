using Transfer.Domain.Entity.Inventory;

namespace Transfer.Application.Interfaces.Inventory;

public interface ISupplierRepository : IRepository<Supplier, string>
{
}
using Agrovet.Application.Features.Inventory.Supplier.Dtos;
using Agrovet.Application.Interfaces.Inventory;
using AutoMapper;
using MediatR;

namespace Agrovet.Application.Features.Inventory.Supplier.Queries;

public record SuppliersQuery : IRequest<SupplierResponse[]>;

public class SuppliersQueryHandler(ISupplierRepository supplierRepository, IMapper mapper)
    : RequestHandlerBase, IRequestHandler<SuppliersQuery, SupplierResponse[]>
{
    public async Task<SupplierResponse[]> Handle(SuppliersQuery request, CancellationToken cancellationToken)
    {
        var suppliers = await supplierRepository.GetAllAsync();
        return mapper.Map<SupplierResponse[]>(suppliers);
    }

    protected override void DisposeCore()
    {
        supplierRepository.Dispose();
    }
}
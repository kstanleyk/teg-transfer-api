using AutoMapper;
using MediatR;
using Transfer.Application.Features.Inventory.Warehouse.Dtos;
using Transfer.Application.Interfaces.Inventory;

namespace Transfer.Application.Features.Inventory.Warehouse.Queries;

public record WarehousesQuery : IRequest<WarehouseResponse[]>;

public class WarehousesQueryHandler(IWarehouseRepository warehouseRepository, IMapper mapper)
    : RequestHandlerBase, IRequestHandler<WarehousesQuery, WarehouseResponse[]>
{

    public async Task<WarehouseResponse[]> Handle(WarehousesQuery request, CancellationToken cancellationToken)
    {
        var warehouses = await warehouseRepository.GetAllAsync();
        return mapper.Map<WarehouseResponse[]>(warehouses);
    }

    protected override void DisposeCore()
    {
        warehouseRepository.Dispose();
    }
}
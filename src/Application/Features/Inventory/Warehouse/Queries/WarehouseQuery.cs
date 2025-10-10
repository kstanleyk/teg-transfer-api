using AutoMapper;
using MediatR;
using Transfer.Application.Features.Inventory.Warehouse.Dtos;
using Transfer.Application.Interfaces.Inventory;

namespace Transfer.Application.Features.Inventory.Warehouse.Queries;

public record WarehouseQuery : IRequest<WarehouseResponse>
{
    public required Guid PublicId { get; set; }
}

public class WarehouseQueryHandler(IWarehouseRepository warehouseRepository, IMapper mapper)
    : RequestHandlerBase, IRequestHandler<WarehouseQuery, WarehouseResponse>
{

    public async Task<WarehouseResponse> Handle(WarehouseQuery request, CancellationToken cancellationToken)
    {
        var warehouse = await warehouseRepository.GetByPublicIdAsync(request.PublicId);
        return mapper.Map<WarehouseResponse>(warehouse);
    }

    protected override void DisposeCore()
    {
        warehouseRepository.Dispose();
    }
}
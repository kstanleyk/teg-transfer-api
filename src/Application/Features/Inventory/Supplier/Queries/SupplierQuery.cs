using AutoMapper;
using MediatR;
using Transfer.Application.Features.Inventory.Supplier.Dtos;
using Transfer.Application.Interfaces.Inventory;

namespace Transfer.Application.Features.Inventory.Supplier.Queries;

public record SupplierQuery : IRequest<SupplierResponse>
{
    public required Guid PublicId { get; set; }
}

public class SupplierQueryHandler(ISupplierRepository itemCategoryRepository, IMapper mapper)
    : RequestHandlerBase, IRequestHandler<SupplierQuery, SupplierResponse>
{

    public async Task<SupplierResponse> Handle(SupplierQuery request, CancellationToken cancellationToken)
    {
        var itemCategory = await itemCategoryRepository.GetByPublicIdAsync(request.PublicId);
        return mapper.Map<SupplierResponse>(itemCategory);
    }

    protected override void DisposeCore()
    {
        itemCategoryRepository.Dispose();
    }
}
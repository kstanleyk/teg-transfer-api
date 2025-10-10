using AutoMapper;
using MediatR;
using Transfer.Application.Features.Inventory.ProductMovement.Dtos;
using Transfer.Application.Interfaces.Inventory;

namespace Transfer.Application.Features.Inventory.ProductMovement.Queries;

public record ProductMovementQuery : IRequest<ProductMovementResponse>
{
    public required string Id { get; set; }
}

public class ProductMovementQueryHandler(IProductMovementRepository productMovementRepository, IMapper mapper)
    : RequestHandlerBase, IRequestHandler<ProductMovementQuery, ProductMovementResponse>
{

    public async Task<ProductMovementResponse> Handle(ProductMovementQuery request, CancellationToken cancellationToken)
    {
        var itemMovement = await productMovementRepository.GetAsync(request.Id);
        return mapper.Map<ProductMovementResponse>(itemMovement);
    }

    protected override void DisposeCore()
    {
        productMovementRepository.Dispose();
    }
}
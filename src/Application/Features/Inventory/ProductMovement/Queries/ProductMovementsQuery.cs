using Agrovet.Application.Features.Inventory.ProductMovement.Dtos;
using Agrovet.Application.Interfaces.Inventory;
using AutoMapper;
using MediatR;

namespace Agrovet.Application.Features.Inventory.ProductMovement.Queries;

public record ProductMovementsQuery : IRequest<ProductMovementResponse[]>;

public class ProductMovementsQueryHandler(IProductMovementRepository productMovementRepository, IMapper mapper)
    : RequestHandlerBase, IRequestHandler<ProductMovementsQuery, ProductMovementResponse[]>
{

    public async Task<ProductMovementResponse[]> Handle(ProductMovementsQuery request, CancellationToken cancellationToken)
    {
        var itemMovements = await productMovementRepository.GetAllAsync();
        return mapper.Map<ProductMovementResponse[]>(itemMovements);
    }

    protected override void DisposeCore()
    {
        productMovementRepository.Dispose();
    }
}
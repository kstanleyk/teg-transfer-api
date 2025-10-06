using Agrovet.Application.Features.Inventory.Product.Dtos;
using Agrovet.Application.Interfaces.Inventory;
using AutoMapper;
using MediatR;

namespace Agrovet.Application.Features.Inventory.Product.Queries;

public record ProductQuery : IRequest<ProductResponse>
{
    public required Guid PublicId { get; set; }
}

public class ProductQueryHandler(IProductRepository productRepository, IMapper mapper)
    : RequestHandlerBase, IRequestHandler<ProductQuery, ProductResponse>
{

    public async Task<ProductResponse> Handle(ProductQuery request, CancellationToken cancellationToken)
    {
        var item = await productRepository.GetByPublicIdAsync(request.PublicId);
        return mapper.Map<ProductResponse>(item);
    }

    protected override void DisposeCore()
    {
        productRepository.Dispose();
    }
}
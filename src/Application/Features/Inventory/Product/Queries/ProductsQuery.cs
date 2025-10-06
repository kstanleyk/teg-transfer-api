using Agrovet.Application.Features.Inventory.Product.Dtos;
using Agrovet.Application.Interfaces.Inventory;
using AutoMapper;
using MediatR;

namespace Agrovet.Application.Features.Inventory.Product.Queries;

public record ProductsQuery : IRequest<ProductResponse[]>;

public class ProductsQueryHandler(IProductRepository productRepository, IMapper mapper)
    : RequestHandlerBase, IRequestHandler<ProductsQuery, ProductResponse[]>
{
    public async Task<ProductResponse[]> Handle(ProductsQuery request, CancellationToken cancellationToken)
    {
        var products = await productRepository.GetAllAsync();
        return mapper.Map<ProductResponse[]>(products);
    }

    protected override void DisposeCore()
    {
        productRepository.Dispose();
    }
}
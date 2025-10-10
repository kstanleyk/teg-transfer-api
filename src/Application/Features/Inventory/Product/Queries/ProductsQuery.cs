using AutoMapper;
using MediatR;
using Transfer.Application.Features.Inventory.Product.Dtos;
using Transfer.Application.Interfaces.Inventory;

namespace Transfer.Application.Features.Inventory.Product.Queries;

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
using Agrovet.Application.Features.Inventory.Product.Dtos;
using Agrovet.Application.Interfaces.Inventory;
using AutoMapper;
using MediatR;

namespace Agrovet.Application.Features.Inventory.Product.Queries;

public record ProductStockBalancesQuery : IRequest<ProductStockBalanceResponse[]>;

public class ProductStockBalancesQueryHandler(IProductRepository productRepository, IMapper mapper)
    : RequestHandlerBase, IRequestHandler<ProductStockBalancesQuery, ProductStockBalanceResponse[]>
{
    public async Task<ProductStockBalanceResponse[]> Handle(ProductStockBalancesQuery request, CancellationToken cancellationToken)
    {
        var stockBalances = await productRepository.GetItemStockBalancesAsync();
        return mapper.Map<ProductStockBalanceResponse[]>(stockBalances);
    }

    protected override void DisposeCore()
    {
        productRepository.Dispose();
    }
}
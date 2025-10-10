using AutoMapper;
using MediatR;
using Transfer.Application.Features.Inventory.Product.Dtos;
using Transfer.Application.Interfaces.Inventory;

namespace Transfer.Application.Features.Inventory.Product.Queries;

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
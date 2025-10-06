using Agrovet.Application.Features.Inventory.ProductCategory.Dtos;
using Agrovet.Application.Interfaces.Inventory;
using AutoMapper;
using MediatR;

namespace Agrovet.Application.Features.Inventory.ProductCategory.Queries;

public record ProductCategoryQuery : IRequest<ProductCategoryResponse>
{
    public required Guid PublicId { get; set; }
}

public class ProductCategoryQueryHandler(IProductCategoryRepository productCategoryRepository, IMapper mapper)
    : RequestHandlerBase, IRequestHandler<ProductCategoryQuery, ProductCategoryResponse>
{

    public async Task<ProductCategoryResponse> Handle(ProductCategoryQuery request, CancellationToken cancellationToken)
    {
        var itemCategory = await productCategoryRepository.GetByPublicIdAsync(request.PublicId);
        return mapper.Map<ProductCategoryResponse>(itemCategory);
    }

    protected override void DisposeCore()
    {
        productCategoryRepository.Dispose();
    }
}
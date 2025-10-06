using Agrovet.Application.Features.Inventory.ProductCategory.Dtos;
using Agrovet.Application.Interfaces.Inventory;
using AutoMapper;
using MediatR;

namespace Agrovet.Application.Features.Inventory.ProductCategory.Queries;

public record ProductCategoriesQuery : IRequest<ProductCategoryResponse[]>;

public class ProductCategoriesQueryHandler(IProductCategoryRepository productCategoryRepository, IMapper mapper)
    : RequestHandlerBase, IRequestHandler<ProductCategoriesQuery, ProductCategoryResponse[]>
{

    public async Task<ProductCategoryResponse[]> Handle(ProductCategoriesQuery request, CancellationToken cancellationToken)
    {
        var itemCategories = await productCategoryRepository.GetAllAsync();
        return mapper.Map<ProductCategoryResponse[]>(itemCategories);
    }

    protected override void DisposeCore()
    {
        productCategoryRepository.Dispose();
    }
}
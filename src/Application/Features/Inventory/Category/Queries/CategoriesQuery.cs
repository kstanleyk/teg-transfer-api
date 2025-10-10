using AutoMapper;
using MediatR;
using Transfer.Application.Features.Inventory.Category.Dtos;
using Transfer.Application.Interfaces.Inventory;

namespace Transfer.Application.Features.Inventory.Category.Queries;

public record CategoriesQuery : IRequest<CategoryResponse[]>;

public class ProductCategoriesQueryHandler(ICategoryRepository categoryRepository, IMapper mapper)
    : RequestHandlerBase, IRequestHandler<CategoriesQuery, CategoryResponse[]>
{

    public async Task<CategoryResponse[]> Handle(CategoriesQuery request, CancellationToken cancellationToken)
    {
        var itemCategories = await categoryRepository.GetAllAsync();
        return mapper.Map<CategoryResponse[]>(itemCategories);
    }

    protected override void DisposeCore()
    {
        categoryRepository.Dispose();
    }
}
using AutoMapper;
using MediatR;
using Transfer.Application.Features.Inventory.Category.Dtos;
using Transfer.Application.Interfaces.Inventory;

namespace Transfer.Application.Features.Inventory.Category.Queries;

public record CategoryQuery : IRequest<CategoryResponse>
{
    public required Guid PublicId { get; set; }
}

public class CategoryQueryHandler(ICategoryRepository categoryRepository, IMapper mapper)
    : RequestHandlerBase, IRequestHandler<CategoryQuery, CategoryResponse>
{

    public async Task<CategoryResponse> Handle(CategoryQuery request, CancellationToken cancellationToken)
    {
        var itemCategory = await categoryRepository.GetByPublicIdAsync(request.PublicId);
        return mapper.Map<CategoryResponse>(itemCategory);
    }

    protected override void DisposeCore()
    {
        categoryRepository.Dispose();
    }
}
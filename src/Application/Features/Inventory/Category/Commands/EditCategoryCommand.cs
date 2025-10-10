using AutoMapper;
using MediatR;
using Transfer.Application.Features.Inventory.Category.Dtos;
using Transfer.Application.Helpers;
using Transfer.Application.Helpers.Exceptions;
using Transfer.Application.Interfaces.Inventory;

namespace Transfer.Application.Features.Inventory.Category.Commands;

public class EditCategoryCommandResponse : BaseResponse
{
    public CategoryUpdatedResponse Data { get; set; } = null!;
}

public class EditCategoryCommand : IRequest<EditCategoryCommandResponse>
{
    public required EditCategoryRequest Category { get; set; }
}

public class EditCategoryCommandHandler(ICategoryRepository categoryRepository, IMapper mapper) 
    : RequestHandlerBase, IRequestHandler<EditCategoryCommand, EditCategoryCommandResponse>
{
    public async Task<EditCategoryCommandResponse> Handle(EditCategoryCommand request, 
        CancellationToken cancellationToken)
    {
        var response = new EditCategoryCommandResponse();

        var validator = new EditCategoryCommandValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            response.ValidationErrors = validationResult.Errors
                .Select(e => e.ErrorMessage)
                .Distinct()
                .ToList();

            throw new ValidationException(response.ValidationErrors);
        }

        var icr = request.Category;

        var itemCategory = Transfer.Domain.Entity.Inventory.Category.Create(icr.Name);
        itemCategory.SetId(icr.Id);
        itemCategory.SetPublicId(icr.PublicId);

        var result = await categoryRepository.UpdateAsyncAsync(icr.PublicId, itemCategory);

        if (result.Status != RepositoryActionStatus.Updated && 
            result.Status != RepositoryActionStatus.NothingModified)
        {
            response.Success = false;
            return response;
        }

        response.Data = mapper.Map<CategoryUpdatedResponse>(result.Entity);

        return response;
    }

    protected override void DisposeCore()
    {
        categoryRepository.Dispose();
    }
}
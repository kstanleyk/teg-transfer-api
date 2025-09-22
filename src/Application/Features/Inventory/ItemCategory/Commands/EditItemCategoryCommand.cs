using Agrovet.Application.Features.Inventory.ItemCategory.Dtos;
using Agrovet.Application.Helpers;
using Agrovet.Application.Helpers.Exceptions;
using Agrovet.Application.Interfaces.Inventory;
using AutoMapper;
using MediatR;

namespace Agrovet.Application.Features.Inventory.ItemCategory.Commands;

public class EditItemCategoryCommandResponse : BaseResponse
{
    public ItemCategoryUpdatedResponse Data { get; set; } = null!;
}

public class EditItemCategoryCommand : IRequest<EditItemCategoryCommandResponse>
{
    public required EditItemCategoryRequest ItemCategory { get; set; }
}

public class EditItemCategoryCommandHandler(IItemCategoryRepository itemCategoryRepository, IMapper mapper) 
    : RequestHandlerBase, IRequestHandler<EditItemCategoryCommand, EditItemCategoryCommandResponse>
{
    public async Task<EditItemCategoryCommandResponse> Handle(EditItemCategoryCommand request, 
        CancellationToken cancellationToken)
    {
        var response = new EditItemCategoryCommandResponse();

        var validator = new EditItemCategoryCommandValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            response.ValidationErrors = validationResult.Errors
                .Select(e => e.ErrorMessage)
                .Distinct()
                .ToList();

            throw new ValidationException(response.ValidationErrors);
        }

        var icr = request.ItemCategory;

        var itemCategory = Domain.Entity.Inventory.ItemCategory.Create(icr.Name);
        itemCategory.SetId(icr.Id);
        itemCategory.SetPublicId(icr.PublicId);

        var result = await itemCategoryRepository.UpdateAsyncAsync(icr.PublicId, itemCategory);

        if (result.Status != RepositoryActionStatus.Updated && 
            result.Status != RepositoryActionStatus.NothingModified)
        {
            response.Success = false;
            return response;
        }

        response.Data = mapper.Map<ItemCategoryUpdatedResponse>(result.Entity);

        return response;
    }

    protected override void DisposeCore()
    {
        itemCategoryRepository.Dispose();
    }
}
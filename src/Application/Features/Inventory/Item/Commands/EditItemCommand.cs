using Agrovet.Application.Features.Inventory.Item.Dtos;
using Agrovet.Application.Helpers;
using Agrovet.Application.Helpers.Exceptions;
using Agrovet.Application.Interfaces.Core;
using Agrovet.Application.Interfaces.Inventory;
using AutoMapper;
using MediatR;

namespace Agrovet.Application.Features.Inventory.Item.Commands;

public class EditItemCommandResponse : BaseResponse
{
    public ItemUpdatedResponse Data { get; set; } = null!;
}

public class EditItemCommand : IRequest<EditItemCommandResponse>
{
    public required EditItemRequest Item { get; set; }
}

public class EditItemCommandHandler(IItemRepository itemRepository, IEstateRepository estateRepository, IMapper mapper)
    :
        RequestHandlerBase, IRequestHandler<EditItemCommand, EditItemCommandResponse>
{
    public async Task<EditItemCommandResponse> Handle(EditItemCommand request, 
        CancellationToken cancellationToken)
    {
        var response = new EditItemCommandResponse();

        var ids = await estateRepository.GetIdsAsync();

        var validationCodes = new ItemValidationCodes
        {
            CategoryCodes = ids
        };

        var validator = new EditItemCommandValidator(validationCodes);
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            response.ValidationErrors = validationResult.Errors
                .Select(e => e.ErrorMessage)
                .Distinct()
                .ToList();

            throw new ValidationException(response.ValidationErrors);
        }

        var ir = request.Item;
        var item = Domain.Entity.Inventory.Item.Create(ir.Name, ir.ShortDescription, ir.BarCodeText, ir.Brand,
            ir.Category, ir.Status, ir.MinStock, ir.MaxStock, ir.ReorderLev, ir.ReorderQtty, DateTime.UtcNow);

        item.SetId(ir.Id);
        //item.SetPublicId(ir.PublicId);

        var result = await itemRepository.EditAsync(item);

        if (result.Status != RepositoryActionStatus.Updated && 
            result.Status != RepositoryActionStatus.NothingModified)
        {
            response.Success = false;
            return response;
        }

        response.Data = mapper.Map<ItemUpdatedResponse>(result.Entity);

        return response;
    }

    protected override void DisposeCore()
    {
        itemRepository.Dispose();
        estateRepository.Dispose();
    }
}
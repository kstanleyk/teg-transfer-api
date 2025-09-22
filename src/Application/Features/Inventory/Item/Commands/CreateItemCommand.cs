using Agrovet.Application.Features.Inventory.Item.Dtos;
using Agrovet.Application.Helpers;
using Agrovet.Application.Helpers.Exceptions;
using Agrovet.Application.Interfaces.Inventory;
using AutoMapper;
using MediatR;
using SequentialGuid;

namespace Agrovet.Application.Features.Inventory.Item.Commands;

public class CreateItemCommandResponse : BaseResponse
{
    public ItemCreatedResponse Data { get; set; } = null!;
}

public class CreateItemCommand : IRequest<CreateItemCommandResponse>
{
    public required CreateItemRequest Item { get; set; }
}

public class CreateItemCommandHandler(IItemRepository itemRepository, IMapper mapper) : 
    RequestHandlerBase, IRequestHandler<CreateItemCommand, CreateItemCommandResponse>
{
    public async Task<CreateItemCommandResponse> Handle(CreateItemCommand request, CancellationToken cancellationToken)
    {
        var response = new CreateItemCommandResponse();

        if (request.Item == null)
            throw new ArgumentNullException(nameof(request.Item));

        // Validate the request
        var validator = new CreateItemCommandValidator(new ItemValidationCodes());
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

        item.SetPublicId(SequentialGuidGenerator.Instance.NewGuid());

        var result = await itemRepository.AddAsync(item);

        if (result.Status != RepositoryActionStatus.Created)
        {
            response.Success = false;
            return response;
        }

        response.Data = mapper.Map<ItemCreatedResponse>(result.Entity);

        return response;
    }

    protected override void DisposeCore()
    {
        itemRepository.Dispose();
    }
}

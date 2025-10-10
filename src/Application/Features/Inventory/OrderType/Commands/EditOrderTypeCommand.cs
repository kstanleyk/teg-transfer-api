using AutoMapper;
using MediatR;
using Transfer.Application.Features.Inventory.OrderType.Dtos;
using Transfer.Application.Helpers;
using Transfer.Application.Helpers.Exceptions;
using Transfer.Application.Interfaces.Inventory;

namespace Transfer.Application.Features.Inventory.OrderType.Commands;

public class EditOrderTypeCommandResponse : BaseResponse
{
    public OrderTypeUpdatedResponse Data { get; set; } = null!;
}

public class EditOrderTypeCommand : IRequest<EditOrderTypeCommandResponse>
{
    public required EditOrderTypeRequest OrderType { get; set; }
}

public class EditOrderTypeCommandHandler(IOrderTypeRepository orderTypeRepository, IMapper mapper) 
    : RequestHandlerBase, IRequestHandler<EditOrderTypeCommand, EditOrderTypeCommandResponse>
{
    public async Task<EditOrderTypeCommandResponse> Handle(EditOrderTypeCommand request, 
        CancellationToken cancellationToken)
    {
        var response = new EditOrderTypeCommandResponse();

        var validator = new EditOrderTypeCommandValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            response.ValidationErrors = validationResult.Errors
                .Select(e => e.ErrorMessage)
                .Distinct()
                .ToList();

            throw new ValidationException(response.ValidationErrors);
        }

        var icr = request.OrderType;

        var orderType = Transfer.Domain.Entity.Inventory.OrderType.Create(icr.Name);
        orderType.SetId(icr.Id);
        orderType.SetPublicId(icr.PublicId);

        var result = await orderTypeRepository.UpdateAsyncAsync(icr.PublicId, orderType);

        if (result.Status != RepositoryActionStatus.Updated && 
            result.Status != RepositoryActionStatus.NothingModified)
        {
            response.Success = false;
            return response;
        }

        response.Data = mapper.Map<OrderTypeUpdatedResponse>(result.Entity);

        return response;
    }

    protected override void DisposeCore()
    {
        orderTypeRepository.Dispose();
    }
}
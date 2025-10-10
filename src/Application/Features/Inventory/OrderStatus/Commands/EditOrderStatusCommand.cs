using AutoMapper;
using MediatR;
using Transfer.Application.Features.Inventory.OrderStatus.Dtos;
using Transfer.Application.Helpers;
using Transfer.Application.Helpers.Exceptions;
using Transfer.Application.Interfaces.Inventory;

namespace Transfer.Application.Features.Inventory.OrderStatus.Commands;

public class EditOrderStatusCommandResponse : BaseResponse
{
    public OrderStatusUpdatedResponse Data { get; set; } = null!;
}

public class EditOrderStatusCommand : IRequest<EditOrderStatusCommandResponse>
{
    public required EditOrderStatusRequest OrderStatus { get; set; }
}

public class EditOrderStatusCommandHandler(IOrderStatusRepository orderStatusRepository, IMapper mapper) 
    : RequestHandlerBase, IRequestHandler<EditOrderStatusCommand, EditOrderStatusCommandResponse>
{
    public async Task<EditOrderStatusCommandResponse> Handle(EditOrderStatusCommand request, 
        CancellationToken cancellationToken)
    {
        var response = new EditOrderStatusCommandResponse();

        var validator = new EditOrderStatusCommandValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            response.ValidationErrors = validationResult.Errors
                .Select(e => e.ErrorMessage)
                .Distinct()
                .ToList();

            throw new ValidationException(response.ValidationErrors);
        }

        var icr = request.OrderStatus;

        var orderStatus = Transfer.Domain.Entity.Inventory.OrderStatus.Create(icr.Name);
        orderStatus.SetId(icr.Id);
        orderStatus.SetPublicId(icr.PublicId);

        var result = await orderStatusRepository.UpdateAsyncAsync(icr.PublicId, orderStatus);

        if (result.Status != RepositoryActionStatus.Updated && 
            result.Status != RepositoryActionStatus.NothingModified)
        {
            response.Success = false;
            return response;
        }

        response.Data = mapper.Map<OrderStatusUpdatedResponse>(result.Entity);

        return response;
    }

    protected override void DisposeCore()
    {
        orderStatusRepository.Dispose();
    }
}
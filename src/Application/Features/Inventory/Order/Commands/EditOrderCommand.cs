using Agrovet.Application.Features.Inventory.Order.Dtos;
using Agrovet.Application.Helpers;
using Agrovet.Application.Helpers.Exceptions;
using Agrovet.Application.Interfaces.Core;
using Agrovet.Application.Interfaces.Inventory;
using AutoMapper;
using MediatR;

namespace Agrovet.Application.Features.Inventory.Order.Commands;

public class EditOrderCommandResponse : BaseResponse
{
    public OrderUpdatedResponse Data { get; set; } = null!;
}

public class EditOrderCommand : IRequest<EditOrderCommandResponse>
{
    public required EditOrderRequest Order { get; set; }
}

public class EditOrderCommandHandler(
    IOrderRepository averageWeightRepository,
    IEstateRepository estateRepository,
    IMapper mapper)
    :
        RequestHandlerBase, IRequestHandler<EditOrderCommand, EditOrderCommandResponse>
{
    public async Task<EditOrderCommandResponse> Handle(EditOrderCommand request, 
        CancellationToken cancellationToken)
    {
        var response = new EditOrderCommandResponse();

        var ids = await estateRepository.GetIdsAsync();

        var validationCodes = new OrderValidationCodes
        {
            ValidSuppliers = ids
        };

        var validator = new EditOrderCommandValidator(validationCodes);
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            response.ValidationErrors = validationResult.Errors
                .Select(e => e.ErrorMessage)
                .Distinct()
                .ToList();

            throw new ValidationException(response.ValidationErrors);
        }

        var or = request.Order;

        var order = Domain.Entity.Inventory.Order.Create(or.OrderType, or.OrderDate, or.Status, or.Description,
            or.Supplier, or.TransDate, DateTime.UtcNow);

        order.SetId(or.Id);
        order.SetPublicId(or.PublicId);

        var result = await averageWeightRepository.EditAsync(order);

        if (result.Status != RepositoryActionStatus.Updated && 
            result.Status != RepositoryActionStatus.NothingModified)
        {
            response.Success = false;
            return response;
        }

        response.Data = mapper.Map<OrderUpdatedResponse>(result.Entity);

        return response;
    }

    protected override void DisposeCore()
    {
        averageWeightRepository.Dispose();
        estateRepository.Dispose();
    }
}
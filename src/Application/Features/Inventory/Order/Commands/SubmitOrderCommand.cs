using Agrovet.Application.Features.Inventory.Order.Dtos;
using Agrovet.Application.Helpers;
using Agrovet.Application.Helpers.Exceptions;
using Agrovet.Application.Interfaces.Inventory;
using Agrovet.Domain.ValueObjects;
using AutoMapper;
using MediatR;

namespace Agrovet.Application.Features.Inventory.Order.Commands;

public class SubmitOrderCommandResponse : BaseResponse
{
    public OrderUpdatedResponse Data { get; set; } = null!;
}

public class SubmitOrderCommand : IRequest<SubmitOrderCommandResponse>
{
    public required EditOrderRequest Order { get; set; }
}

public class SubmitOrderCommandHandler(IOrderRepository orderRepository, IMapper mapper)
    :
        RequestHandlerBase, IRequestHandler<SubmitOrderCommand, SubmitOrderCommandResponse>
{
    public async Task<SubmitOrderCommandResponse> Handle(SubmitOrderCommand request, 
        CancellationToken cancellationToken)
    {
        var response = new SubmitOrderCommandResponse();

        var validationCodes = new OrderValidationCodes
        {
            ValidSuppliers = []
        };

        var validator = new SubmitOrderCommandValidator(validationCodes);
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
        var orderDetails = or.OrderDetails.ToArray();

        foreach (var detail in orderDetails)
        {
            var orderDetail = Domain.Entity.Inventory.OrderDetail.Create(detail.Item, detail.Qtty, detail.UnitCost);
            orderDetail.SetPublicId(PublicId.CreateUnique().Value);
            order.AddOrderDetail(orderDetail);
        }

        order.SetId(or.Id);
        order.SetPublicId(or.PublicId);
        order.AttachOrderDetails();

        order.MarkAsSubmitted();

        var result = await orderRepository.UpdateAsyncAsync(order.PublicId, order);

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
        orderRepository.Dispose();
    }
}
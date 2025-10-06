using Agrovet.Application.Features.Inventory.Order.Dtos;
using Agrovet.Application.Helpers;
using Agrovet.Application.Helpers.Exceptions;
using Agrovet.Application.Interfaces.Inventory;
using Agrovet.Domain.ValueObjects;
using AutoMapper;
using MediatR;

namespace Agrovet.Application.Features.Inventory.Order.Commands;

public class ReceiveOrderCommandResponse : BaseResponse
{
    public OrderUpdatedResponse Data { get; set; } = null!;
}

public class ReceiveOrderCommand : IRequest<ReceiveOrderCommandResponse>
{
    public required EditOrderRequest Order { get; set; }
}

public class ReceiveOrderCommandHandler(IOrderRepository orderRepository, IMapper mapper)
    :
        RequestHandlerBase, IRequestHandler<ReceiveOrderCommand, ReceiveOrderCommandResponse>
{
    public async Task<ReceiveOrderCommandResponse> Handle(ReceiveOrderCommand request, 
        CancellationToken cancellationToken)
    {
        var response = new ReceiveOrderCommandResponse();

        var validationCodes = new OrderValidationCodes
        {
            ValidSuppliers = []
        };

        var validator = new ReceiveOrderCommandValidator(validationCodes);
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

        var order = ProcessOrder(or);

        var result = await orderRepository.ReceiveOrderAsync(order.PublicId, order);

        if (result.Status != RepositoryActionStatus.Updated && 
            result.Status != RepositoryActionStatus.NothingModified)
        {
            response.Success = false;
            return response;
        }

        response.Data = mapper.Map<OrderUpdatedResponse>(result.Entity);

        return response;
    }

    private static Domain.Entity.Inventory.Order ProcessOrder(EditOrderRequest or)
    {
        var order = Domain.Entity.Inventory.Order.Create(or.OrderType, or.OrderDate, or.Status, or.Description,
            or.Supplier, or.TransDate, DateTime.UtcNow);
        order.SetId(or.Id);
        order.SetPublicId(or.PublicId);

        var orderDetails = or.OrderDetails.ToArray();
        foreach (var detail in orderDetails)
        {
            var orderDetail = Domain.Entity.Inventory.OrderDetail.Create(detail.Item, detail.Qtty, detail.UnitCost);
            orderDetail.SetPublicId(PublicId.CreateUnique().Value);
            order.AddOrderDetail(orderDetail);
        }
        order.AttachOrderDetails();
        order.MarkAsReceived();
        return order;
    }

    protected override void DisposeCore()
    {
        orderRepository.Dispose();
    }
}
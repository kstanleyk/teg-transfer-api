using AutoMapper;
using MediatR;
using Transfer.Application.Features.Inventory.Order.Dtos;
using Transfer.Application.Helpers;
using Transfer.Application.Helpers.Exceptions;
using Transfer.Application.Interfaces.Inventory;
using Transfer.Domain.ValueObjects;

namespace Transfer.Application.Features.Inventory.Order.Commands;

public class EditOrderCommandResponse : BaseResponse
{
    public OrderUpdatedResponse Data { get; set; } = null!;
}

public class EditOrderCommand : IRequest<EditOrderCommandResponse>
{
    public required EditOrderRequest Order { get; set; }
}

public class EditOrderCommandHandler(IOrderRepository orderRepository, IMapper mapper)
    :
        RequestHandlerBase, IRequestHandler<EditOrderCommand, EditOrderCommandResponse>
{
    public async Task<EditOrderCommandResponse> Handle(EditOrderCommand request, 
        CancellationToken cancellationToken)
    {
        var response = new EditOrderCommandResponse();

        var validationCodes = new OrderValidationCodes
        {
            ValidSuppliers = []
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

        var order = Transfer.Domain.Entity.Inventory.Order.Create(or.OrderType, or.OrderDate, or.Status, or.Description,
            or.Supplier, or.TransDate, DateTime.UtcNow);
        order.SetId(or.Id);
        order.SetPublicId(or.PublicId);
        order.MarkAsPendingSubmission();

        var orderDetails = or.OrderDetails.ToArray();
        foreach (var detail in orderDetails)
        {
            var packagingType = Transfer.Domain.Entity.Inventory.PackagingType.FromId(detail.PackagingType);

            var orderDetail = Transfer.Domain.Entity.Inventory.OrderDetail.Create(detail.Item, detail.BatchNumber,
                detail.Qtty, detail.UnitCost, packagingType);

            orderDetail.SetPublicId(PublicId.CreateUnique().Value);
            order.AddOrderDetail(orderDetail);
        }
        order.AttachOrderDetails();

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
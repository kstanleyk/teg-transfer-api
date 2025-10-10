using AutoMapper;
using MediatR;
using SequentialGuid;
using Transfer.Application.Features.Inventory.Order.Dtos;
using Transfer.Application.Helpers;
using Transfer.Application.Helpers.Exceptions;
using Transfer.Application.Interfaces.Inventory;
using Transfer.Domain.ValueObjects;

namespace Transfer.Application.Features.Inventory.Order.Commands;

public class CreateOrderCommandResponse : BaseResponse
{
    public OrderCreatedResponse Data { get; set; } = null!;
}

public class CreateOrderCommand : IRequest<CreateOrderCommandResponse>
{
    public required CreateOrderRequest Order { get; set; }
}

public class CreateOrderCommandHandler(IOrderRepository orderRepository, IMapper mapper)
    :
        RequestHandlerBase, IRequestHandler<CreateOrderCommand, CreateOrderCommandResponse>
{
    public async Task<CreateOrderCommandResponse> Handle(CreateOrderCommand request,
        CancellationToken cancellationToken)
    {
        var response = new CreateOrderCommandResponse();

        var validationCodes = new OrderValidationCodes
        {
            ValidSuppliers = []
        };

        var validator = new CreateOrderCommandValidator(validationCodes);
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            response.ValidationErrors = validationResult.Errors
                .Select(e => e.ErrorMessage)
                .Distinct()
                .ToList();

            throw new ValidationException(response.ValidationErrors);
        }

        if (request.Order == null)
            throw new ArgumentNullException(nameof(request.Order));

        var or = request.Order;

        var order = Transfer.Domain.Entity.Inventory.Order.Create(or.OrderType, or.OrderDate, or.Status, or.Description,
            or.Supplier, or.TransDate, DateTime.UtcNow);

        var orderDetails = or.OrderDetails?.ToArray();
        if (orderDetails != null)
        {
            foreach (var detail in orderDetails)
            {
                var packagingType = Transfer.Domain.Entity.Inventory.PackagingType.FromId(detail.PackagingType);

                var orderDetail = Transfer.Domain.Entity.Inventory.OrderDetail.Create(detail.Item, detail.BatchNumber,
                    detail.Qtty, detail.UnitCost, packagingType);

                orderDetail.SetPublicId(PublicId.CreateUnique().Value);
                order.AddOrderDetail(orderDetail);
            }
        }

        order.MarkAsPendingSubmission();

        order.SetPublicId(SequentialGuidGenerator.Instance.NewGuid());

        var result = await orderRepository.AddAsync(order);

        if (result.Status != RepositoryActionStatus.Created)
        {
            response.Success = false;
            return response;
        }

        response.Data = mapper.Map<OrderCreatedResponse>(result.Entity);

        return response;
    }

    protected override void DisposeCore()
    {
        orderRepository.Dispose();
    }
}
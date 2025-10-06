using Agrovet.Application.Features.Inventory.OrderStatus.Dtos;
using Agrovet.Application.Interfaces.Inventory;
using AutoMapper;
using MediatR;

namespace Agrovet.Application.Features.Inventory.OrderStatus.Queries;

public record OrderStatusQuery : IRequest<OrderStatusResponse>
{
    public required Guid PublicId { get; set; }
}

public class OrderStatusQueryHandler(IOrderStatusRepository orderStatusRepository, IMapper mapper)
    : RequestHandlerBase, IRequestHandler<OrderStatusQuery, OrderStatusResponse>
{

    public async Task<OrderStatusResponse> Handle(OrderStatusQuery request, CancellationToken cancellationToken)
    {
        var orderStatus = await orderStatusRepository.GetByPublicIdAsync(request.PublicId);
        return mapper.Map<OrderStatusResponse>(orderStatus);
    }

    protected override void DisposeCore()
    {
        orderStatusRepository.Dispose();
    }
}
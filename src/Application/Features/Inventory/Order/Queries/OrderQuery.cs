using AutoMapper;
using MediatR;
using Transfer.Application.Features.Inventory.Order.Dtos;
using Transfer.Application.Interfaces.Inventory;

namespace Transfer.Application.Features.Inventory.Order.Queries;

public record OrderQuery : IRequest<OrderResponse>
{
    public Guid PublicId { get; set; }
}

public class OrderQueryHandler(IOrderRepository orderRepository, IMapper mapper)
    : RequestHandlerBase, IRequestHandler<OrderQuery, OrderResponse>
{

    public async Task<OrderResponse> Handle(OrderQuery request, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByPublicIdAsync(request.PublicId);
        return mapper.Map<OrderResponse>(order);
    }

    protected override void DisposeCore()
    {
        orderRepository.Dispose();
    }
}
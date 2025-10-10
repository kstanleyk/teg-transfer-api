using AutoMapper;
using MediatR;
using Transfer.Application.Features.Inventory.Order.Dtos;
using Transfer.Application.Interfaces.Inventory;

namespace Transfer.Application.Features.Inventory.Order.Queries;

public record OrdersQuery : IRequest<OrderResponse[]>;

public class OrdersQueryHandler(IOrderRepository orderRepository, IMapper mapper)
    : RequestHandlerBase, IRequestHandler<OrdersQuery, OrderResponse[]>
{

    public async Task<OrderResponse[]> Handle(OrdersQuery request, CancellationToken cancellationToken)
    {
        var orders = await orderRepository.GetAllAsync();
        return mapper.Map<OrderResponse[]>(orders);
    }

    protected override void DisposeCore()
    {
        orderRepository.Dispose();
    }
}
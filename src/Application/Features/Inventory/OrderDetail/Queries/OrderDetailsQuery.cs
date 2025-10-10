using AutoMapper;
using MediatR;
using Transfer.Application.Features.Inventory.OrderDetail.Dtos;
using Transfer.Application.Interfaces.Inventory;

namespace Transfer.Application.Features.Inventory.OrderDetail.Queries;

public record OrderDetailsQuery : IRequest<OrderDetailResponse[]>;

public class OrderDetailsQueryHandler(IOrderDetailRepository orderRepository, IMapper mapper)
    : RequestHandlerBase, IRequestHandler<OrderDetailsQuery, OrderDetailResponse[]>
{

    public async Task<OrderDetailResponse[]> Handle(OrderDetailsQuery request, CancellationToken cancellationToken)
    {
        var orderDetails = await orderRepository.GetAllAsync();
        return mapper.Map<OrderDetailResponse[]>(orderDetails);
    }

    protected override void DisposeCore()
    {
        orderRepository.Dispose();
    }
}
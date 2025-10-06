using Agrovet.Application.Features.Inventory.Order.Dtos;
using Agrovet.Application.Interfaces.Inventory;
using AutoMapper;
using MediatR;

namespace Agrovet.Application.Features.Inventory.OrderDetail.Queries;

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
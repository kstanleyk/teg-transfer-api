using Agrovet.Application.Features.Inventory.OrderStatus.Dtos;
using Agrovet.Application.Interfaces.Inventory;
using AutoMapper;
using MediatR;

namespace Agrovet.Application.Features.Inventory.OrderStatus.Queries;

public record OrderStatusesQuery : IRequest<OrderStatusResponse[]>;

public class OrderStatusesQueryHandler(IOrderStatusRepository orderStatusRepository, IMapper mapper)
    : RequestHandlerBase, IRequestHandler<OrderStatusesQuery, OrderStatusResponse[]>
{

    public async Task<OrderStatusResponse[]> Handle(OrderStatusesQuery request, CancellationToken cancellationToken)
    {
        var orderStatuses = await orderStatusRepository.GetAllAsync();
        return mapper.Map<OrderStatusResponse[]>(orderStatuses);
    }

    protected override void DisposeCore()
    {
        orderStatusRepository.Dispose();
    }
}
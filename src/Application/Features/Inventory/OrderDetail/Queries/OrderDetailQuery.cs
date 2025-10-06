using Agrovet.Application.Features.Inventory.Order.Dtos;
using Agrovet.Application.Interfaces.Inventory;
using AutoMapper;
using MediatR;

namespace Agrovet.Application.Features.Inventory.OrderDetail.Queries;

public record OrderDetailQuery : IRequest<OrderDetailResponse>
{
    public Guid PublicId { get; set; }
}

public class OrderDetailQueryHandler(IOrderDetailRepository orderRepository, IMapper mapper)
    : RequestHandlerBase, IRequestHandler<OrderDetailQuery, OrderDetailResponse>
{

    public async Task<OrderDetailResponse> Handle(OrderDetailQuery request, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByPublicIdAsync(request.PublicId);
        return mapper.Map<OrderDetailResponse>(order);
    }

    protected override void DisposeCore()
    {
        orderRepository.Dispose();
    }
}
using AutoMapper;
using MediatR;
using Transfer.Application.Features.Inventory.OrderDetail.Dtos;
using Transfer.Application.Interfaces.Inventory;

namespace Transfer.Application.Features.Inventory.OrderDetail.Queries;

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
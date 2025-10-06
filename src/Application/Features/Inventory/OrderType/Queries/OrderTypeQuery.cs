using Agrovet.Application.Features.Inventory.OrderType.Dtos;
using Agrovet.Application.Interfaces.Inventory;
using AutoMapper;
using MediatR;

namespace Agrovet.Application.Features.Inventory.OrderType.Queries;

public record OrderTypeQuery : IRequest<OrderTypeResponse>
{
    public required Guid PublicId { get; set; }
}

public class OrderTypeQueryHandler(IOrderTypeRepository orderTypeRepository, IMapper mapper)
    : RequestHandlerBase, IRequestHandler<OrderTypeQuery, OrderTypeResponse>
{

    public async Task<OrderTypeResponse> Handle(OrderTypeQuery request, CancellationToken cancellationToken)
    {
        var orderType = await orderTypeRepository.GetByPublicIdAsync(request.PublicId);
        return mapper.Map<OrderTypeResponse>(orderType);
    }

    protected override void DisposeCore()
    {
        orderTypeRepository.Dispose();
    }
}
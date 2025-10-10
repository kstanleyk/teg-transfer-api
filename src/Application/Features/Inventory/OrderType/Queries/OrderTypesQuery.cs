using AutoMapper;
using MediatR;
using Transfer.Application.Features.Inventory.OrderType.Dtos;
using Transfer.Application.Interfaces.Inventory;

namespace Transfer.Application.Features.Inventory.OrderType.Queries;

public record OrderTypesQuery : IRequest<OrderTypeResponse[]>;

public class OrderTypesQueryHandler(IOrderTypeRepository orderTypeRepository, IMapper mapper)
    : RequestHandlerBase, IRequestHandler<OrderTypesQuery, OrderTypeResponse[]>
{

    public async Task<OrderTypeResponse[]> Handle(OrderTypesQuery request, CancellationToken cancellationToken)
    {
        var orderTypes = await orderTypeRepository.GetAllAsync();
        return mapper.Map<OrderTypeResponse[]>(orderTypes);
    }

    protected override void DisposeCore()
    {
        orderTypeRepository.Dispose();
    }
}
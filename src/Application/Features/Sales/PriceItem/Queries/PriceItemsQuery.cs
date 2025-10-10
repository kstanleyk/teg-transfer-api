using AutoMapper;
using MediatR;
using Transfer.Application.Features.Sales.PriceItem.Dtos;
using Transfer.Application.Interfaces.Sales;

namespace Transfer.Application.Features.Sales.PriceItem.Queries;

public record PriceItemsQuery : IRequest<PriceItemResponse[]>;

public class PriceItemsQueryHandler(IPriceItemRepository priceItemRepository, IMapper mapper)
    : RequestHandlerBase, IRequestHandler<PriceItemsQuery, PriceItemResponse[]>
{

    public async Task<PriceItemResponse[]> Handle(PriceItemsQuery request, CancellationToken cancellationToken)
    {
        var itemCategories = await priceItemRepository.GetAllAsync();
        return mapper.Map<PriceItemResponse[]>(itemCategories);
    }
}
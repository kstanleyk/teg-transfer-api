using Agrovet.Application.Features.Core.AverageWeight.Dtos;
using Agrovet.Application.Interfaces.Core;
using AutoMapper;
using MediatR;

namespace Agrovet.Application.Features.Core.AverageWeight.Queries;

public record AverageWeightsQuery : IRequest<AverageWeightResponse[]>;

public class AverageWeightsQueryHandler(IAverageWeightRepository averageWeightRepository, IMapper mapper)
    : RequestHandlerBase, IRequestHandler<AverageWeightsQuery, AverageWeightResponse[]>
{

    public async Task<AverageWeightResponse[]> Handle(AverageWeightsQuery request, CancellationToken cancellationToken)
    {
        var averageWeight = await averageWeightRepository.GetAllAsync();
        return mapper.Map<AverageWeightResponse[]>(averageWeight);
    }

    protected override void DisposeCore()
    {
        averageWeightRepository.Dispose();
    }
}
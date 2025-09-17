using Agrovet.Application.Features.AverageWeight.Dtos;
using Agrovet.Application.Interfaces.Core;
using AutoMapper;
using MediatR;

namespace Agrovet.Application.Features.AverageWeight.Queries;

public record AverageWeightsQuery : IRequest<AverageWeightResponse[]>;

public class AverageWeightsQueryHandler(IAverageWeightRepository averageWeightRepository, IMapper mapper)
    : RequestHandlerBase, IRequestHandler<AverageWeightsQuery, AverageWeightResponse[]>
{

    public async Task<AverageWeightResponse[]> Handle(AverageWeightsQuery request, CancellationToken cancellationToken)
    {
        var averageWeights = await averageWeightRepository.GetAllAsync();
        return mapper.Map<AverageWeightResponse[]>(averageWeights);
    }

    protected override void DisposeCore()
    {
        averageWeightRepository.Dispose();
    }
}
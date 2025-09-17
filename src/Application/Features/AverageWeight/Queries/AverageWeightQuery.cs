using Agrovet.Application.Features.AverageWeight.Dtos;
using Agrovet.Application.Interfaces.Core;
using AutoMapper;
using MediatR;

namespace Agrovet.Application.Features.AverageWeights.Queries;

public record AverageWeightQuery : IRequest<AverageWeightResponse>
{
    public required string Id { get; set; }
}

public class AverageWeightQueryHandler(IAverageWeightRepository departmentRepository, IMapper mapper)
    : RequestHandlerBase, IRequestHandler<AverageWeightQuery, AverageWeightResponse>
{

    public async Task<AverageWeightResponse> Handle(AverageWeightQuery request, CancellationToken cancellationToken)
    {
        var department = await departmentRepository.GetAsync(request.Id);
        return mapper.Map<AverageWeightResponse>(department);
    }

    protected override void DisposeCore()
    {
        departmentRepository.Dispose();
    }
}
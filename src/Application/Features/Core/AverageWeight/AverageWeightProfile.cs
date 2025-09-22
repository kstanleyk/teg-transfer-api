using Agrovet.Application.Features.Core.AverageWeight.Dtos;
using AutoMapper;

namespace Agrovet.Application.Features.Core.AverageWeight;

public class AverageWeightProfile : Profile
{
    public AverageWeightProfile()
    {
        CreateMap<Domain.Entity.Core.AverageWeight, AverageWeightResponse>()
            .ForMember(dest => dest.PublicId, opt => opt.MapFrom(src => src.PublicId!.Value));
    }
}
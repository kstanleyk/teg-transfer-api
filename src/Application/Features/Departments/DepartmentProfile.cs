using Agrovet.Domain.Entity;
using AutoMapper;
using DepartmentDto = Agrovet.Application.Models.Core.Department.DepartmentDto;

namespace Agrovet.Application.Features.Departments;

public class DepartmentProfile : Profile
{
    public DepartmentProfile()
    {
        CreateMap<Department, DepartmentDto>()
            .ForMember(dest => dest.PublicId, opt => opt.MapFrom(src => src.PublicId!.Value));
    }
}
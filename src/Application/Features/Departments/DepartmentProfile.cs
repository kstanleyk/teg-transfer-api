using Agrovet.Domain.Entity;
using AutoMapper;
using DepartmentRequest = Agrovet.Application.Models.Core.Department.DepartmentRequest;

namespace Agrovet.Application.Features.Departments;

public class DepartmentProfile : Profile
{
    public DepartmentProfile()
    {
        CreateMap<Department, DepartmentRequest>()
            .ForMember(dest => dest.PublicId, opt => opt.MapFrom(src => src.PublicId!.Value));
    }
}
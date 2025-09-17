using Agrovet.Application.Interfaces.Core;
using AutoMapper;
using MediatR;
using DepartmentDto = Agrovet.Application.Models.Core.Department.DepartmentDto;

namespace Agrovet.Application.Features.Departments.Queries;

public record GetDepartmentsQuery : IRequest<IEnumerable<DepartmentDto>>;

public class GetDepartmentsQueryHandler(IDepartmentRepository departmentRepository, IMapper mapper)
    : IRequestHandler<GetDepartmentsQuery, IEnumerable<DepartmentDto>>
{

    public async Task<IEnumerable<DepartmentDto>> Handle(GetDepartmentsQuery request, CancellationToken cancellationToken)
    {
        var departments = await departmentRepository.GetAllAsync();
        return mapper.Map<IEnumerable<DepartmentDto>>(departments);
    }
}
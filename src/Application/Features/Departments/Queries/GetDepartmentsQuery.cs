using Agrovet.Application.Interfaces.Core;
using AutoMapper;
using MediatR;
using DepartmentRequest = Agrovet.Application.Models.Core.Department.DepartmentRequest;

namespace Agrovet.Application.Features.Departments.Queries;

public record GetDepartmentsQuery : IRequest<IEnumerable<DepartmentRequest>>;

public class GetDepartmentsQueryHandler(IDepartmentRepository departmentRepository, IMapper mapper)
    : IRequestHandler<GetDepartmentsQuery, IEnumerable<DepartmentRequest>>
{

    public async Task<IEnumerable<DepartmentRequest>> Handle(GetDepartmentsQuery request, CancellationToken cancellationToken)
    {
        var departments = await departmentRepository.GetAllAsync();
        return mapper.Map<IEnumerable<DepartmentRequest>>(departments);
    }
}
using Agrovet.Application.Helpers.Exceptions;
using Agrovet.Application.Features.Departments.Validators;
using Agrovet.Application.Helpers;
using Agrovet.Application.Interfaces.Core;
using Agrovet.Application.Models.Core.Department;
using Agrovet.Domain.Entity;
using AutoMapper;
using MediatR;


namespace Agrovet.Application.Features.Departments.Commands;

public class EditDepartmentCommandResponse : BaseResponse
{
    public DepartmentVm Data { get; set; } = null!;
}

public class EditDepartmentCommand : IRequest<EditDepartmentCommandResponse>
{
    public required EditDepartmentDto Department { get; set; }
}

public class EditDepartmentCommandHandler(IDepartmentRepository departmentPersistence, IMapper mapper)
    :
        RequestHandlerBase, IRequestHandler<EditDepartmentCommand, EditDepartmentCommandResponse>
{
    public async Task<EditDepartmentCommandResponse> Handle(EditDepartmentCommand request, CancellationToken cancellationToken)
    {
        var response = new EditDepartmentCommandResponse();

        var validator = new EditDepartmentCommandValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            response.ValidationErrors = validationResult.Errors
                .Select(e => e.ErrorMessage)
                .Distinct()
                .ToList();

            throw new ValidationException(response.ValidationErrors);
        }

        var department = Department.Create(
            name: request.Department.Name,
            facultyId: request.Department.FacultyId,
            createdOn: DateTime.UtcNow);

        department.SetId(request.Department.Id);

        var result = await departmentPersistence.PatchAsync(
            department,
            x => x.Name,
            x=>x.FacultyId
        );

        if (result.Status != RepositoryActionStatus.Updated && result.Status != RepositoryActionStatus.NothingModified)
        {
            response.Success = false;
            return response;
        }

        response.Data = mapper.Map<DepartmentVm>(result.Entity);

        return response;
    }

    protected override void DisposeCore()
    {
        departmentPersistence.Dispose();
    }
}
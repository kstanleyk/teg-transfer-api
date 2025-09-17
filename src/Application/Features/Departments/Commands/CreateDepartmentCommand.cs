using Agrovet.Application.Features.Departments.Validators;
using Agrovet.Application.Helpers;
using Agrovet.Application.Helpers.Exceptions;
using Agrovet.Application.Interfaces.Core;
using Agrovet.Domain.Entity;
using AutoMapper;
using MediatR;
using SequentialGuid;
using CreateDepartmentDto = Agrovet.Application.Models.Core.Department.CreateDepartmentDto;
using CreateDepartmentVm = Agrovet.Application.Models.Core.Department.CreateDepartmentVm;

namespace Agrovet.Application.Features.Departments.Commands;

public class CreateDepartmentCommandResponse : BaseResponse
{
    public CreateDepartmentVm Data { get; set; } = null!;
}

public class CreateDepartmentCommand : IRequest<CreateDepartmentCommandResponse>
{
    public required CreateDepartmentDto Department { get; set; }
}

public class CreateDepartmentCommandHandler(IDepartmentRepository departmentPersistence, IMapper mapper)
    :
        RequestHandlerBase, IRequestHandler<CreateDepartmentCommand, CreateDepartmentCommandResponse>
{
    public async Task<CreateDepartmentCommandResponse> Handle(CreateDepartmentCommand request,
        CancellationToken cancellationToken)
    {
        var response = new CreateDepartmentCommandResponse();

        var validator = new CreateDepartmentCommandValidator();
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

        department.SetPublicId(SequentialGuidGenerator.Instance.NewGuid());

        var result = await departmentPersistence.AddAsync(department);

        if (result.Status != RepositoryActionStatus.Created)
        {
            response.Success = false;
            return response;
        }

        response.Data = mapper.Map<CreateDepartmentVm>(result.Entity);

        return response;
    }

    protected override void DisposeCore()
    {
        departmentPersistence.Dispose();
    }
}
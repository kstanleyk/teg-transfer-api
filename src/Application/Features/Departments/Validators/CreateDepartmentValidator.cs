using Agrovet.Application.Features.Departments.Commands;
using FluentValidation;
using CreateDepartmentDto = Agrovet.Application.Models.Core.Department.CreateDepartmentDto;

namespace Agrovet.Application.Features.Departments.Validators;

public class CreateDepartmentValidator : DepartmentBaseValidator<CreateDepartmentDto>
{
    public CreateDepartmentValidator()
    {
        AddCommonRules();
    }
}

public class CreateDepartmentCommandValidator : AbstractValidator<CreateDepartmentCommand>
{
    public CreateDepartmentCommandValidator()
    {
        RuleFor(p => p.Department)
            .NotNull().WithMessage("Department cannot be empty.")
            .SetValidator(new CreateDepartmentValidator());
    }
}
using Agrovet.Application.Features.Departments.Commands;
using Agrovet.Application.Models.Core.Department;
using FluentValidation;

namespace Agrovet.Application.Features.Departments.Validators;

public class CreateDepartmentValidator : DepartmentBaseValidator<CreateDepartmentRequest>
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
            .NotNull().WithMessage("AverageWeight cannot be empty.")
            .SetValidator(new CreateDepartmentValidator());
    }
}
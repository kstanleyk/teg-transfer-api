using Agrovet.Application.Features.Departments.Commands;
using FluentValidation;
using EditDepartmentDto = Agrovet.Application.Models.Core.Department.EditDepartmentDto;

namespace Agrovet.Application.Features.Departments.Validators;

public class EditDepartmentValidator : DepartmentBaseValidator<EditDepartmentDto>
{
    public EditDepartmentValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id is required")
            .MaximumLength(3).WithMessage("Id must not exceed 3 characters");

        AddCommonRules();
    }
}

public class EditDepartmentCommandValidator : AbstractValidator<EditDepartmentCommand>
{
    public EditDepartmentCommandValidator()
    {
        RuleFor(p => p.Department)
            .NotNull().WithMessage("Department cannot be empty.")
            .SetValidator(new EditDepartmentValidator());
    }
}
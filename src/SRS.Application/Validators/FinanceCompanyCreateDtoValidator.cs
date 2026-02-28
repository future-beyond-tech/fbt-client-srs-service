using FluentValidation;
using SRS.Application.DTOs;

namespace SRS.Application.Validators;

public class FinanceCompanyCreateDtoValidator : AbstractValidator<FinanceCompanyCreateDto>
{
    public FinanceCompanyCreateDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(150);
    }
}

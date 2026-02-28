using FluentValidation;
using SRS.Application.DTOs;

namespace SRS.Application.Validators;

public class PurchaseExpenseCreateDtoValidator : AbstractValidator<PurchaseExpenseCreateDto>
{
    public PurchaseExpenseCreateDtoValidator()
    {
        RuleFor(x => x.ExpenseType)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Amount)
            .GreaterThan(0);
    }
}

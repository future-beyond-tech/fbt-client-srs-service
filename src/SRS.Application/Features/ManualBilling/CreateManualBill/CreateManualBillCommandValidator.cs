using FluentValidation;
using SRS.Application.Common;
using SRS.Application.DTOs;
using SRS.Domain.Enums;

namespace SRS.Application.Features.ManualBilling.CreateManualBill;

public class CreateManualBillCommandValidator : AbstractValidator<CreateManualBillCommand>
{
    public CreateManualBillCommandValidator()
    {
        RuleFor(x => x.Dto).NotNull();
        When(x => x.Dto is not null, () =>
        {
            RuleFor(x => x.Dto!.CustomerName)
                .NotEmpty().WithMessage("Customer name is required.")
                .MaximumLength(200);

            RuleFor(x => x.Dto!.Phone)
                .NotEmpty().WithMessage("Phone is required.")
                .Must(BeValidE164OrNormalizable).WithMessage("Phone must be valid (e.g. 9876543210 or +919876543210).");

            RuleFor(x => x.Dto!.Address).MaximumLength(500);
            RuleFor(x => x.Dto!.PhotoUrl).NotEmpty().WithMessage("Photo URL is required.").MaximumLength(1000);
            RuleFor(x => x.Dto!.SellerName).MaximumLength(200).When(x => x.Dto!.SellerName is not null);
            RuleFor(x => x.Dto!.SellerAddress).MaximumLength(500).When(x => x.Dto!.SellerAddress is not null);
            RuleFor(x => x.Dto!.CustomerNameTitle).MaximumLength(10).When(x => x.Dto!.CustomerNameTitle is not null);
            RuleFor(x => x.Dto!.SellerNameTitle).MaximumLength(10).When(x => x.Dto!.SellerNameTitle is not null);
            RuleFor(x => x.Dto!.ItemDescription).NotEmpty().WithMessage("Item description is required.").MaximumLength(1000);
            RuleFor(x => x.Dto!.ChassisNumber).MaximumLength(100).When(x => x.Dto!.ChassisNumber is not null);
            RuleFor(x => x.Dto!.EngineNumber).MaximumLength(100).When(x => x.Dto!.EngineNumber is not null);
            RuleFor(x => x.Dto!.Color).MaximumLength(80).When(x => x.Dto!.Color is not null);
            RuleFor(x => x.Dto!.Notes).MaximumLength(1000).When(x => x.Dto!.Notes is not null);
            RuleFor(x => x.Dto!.AmountTotal).GreaterThanOrEqualTo(0).WithMessage("Amount total must be >= 0.");
            RuleFor(x => x.Dto!.PaymentMode).IsInEnum();
            RuleFor(x => x.Dto!.CashAmount).GreaterThanOrEqualTo(0).When(x => x.Dto!.CashAmount.HasValue);
            RuleFor(x => x.Dto!.UpiAmount).GreaterThanOrEqualTo(0).When(x => x.Dto!.UpiAmount.HasValue);
            RuleFor(x => x.Dto!.FinanceAmount).GreaterThanOrEqualTo(0).When(x => x.Dto!.FinanceAmount.HasValue);
            RuleFor(x => x.Dto!.FinanceCompany).MaximumLength(150).When(x => x.Dto!.FinanceCompany is not null);
            RuleFor(x => x.Dto).Must(PaymentSplitSumsToTotal).WithMessage("Cash + UPI + Finance must equal AmountTotal.");
        });
    }

    private static bool BeValidE164OrNormalizable(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone)) return false;
        try
        {
            PhoneNormalizer.NormalizeToE164(phone);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static bool PaymentSplitSumsToTotal(ManualBillCreateDto? dto)
    {
        if (dto is null) return true;
        var cash = dto.CashAmount ?? 0m;
        var upi = dto.UpiAmount ?? 0m;
        var finance = dto.FinanceAmount ?? 0m;
        var sum = cash + upi + finance;
        return sum == dto.AmountTotal;
    }
}

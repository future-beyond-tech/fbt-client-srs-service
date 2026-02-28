using FluentValidation;
using SRS.Application.DTOs;

namespace SRS.Application.Validators;

public class UpdateDeliveryNoteSettingsDtoValidator : AbstractValidator<UpdateDeliveryNoteSettingsDto>
{
    public UpdateDeliveryNoteSettingsDtoValidator()
    {
        RuleFor(x => x.ShopName)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.ShopAddress)
            .NotEmpty()
            .MaximumLength(500);

        RuleFor(x => x.GSTNumber)
            .MaximumLength(50);

        RuleFor(x => x.ContactNumber)
            .MaximumLength(30);

        RuleFor(x => x.FooterText)
            .MaximumLength(500);

        RuleFor(x => x.TermsAndConditions)
            .MaximumLength(2000);

        RuleFor(x => x.LogoUrl)
            .MaximumLength(1000)
            .Must(BeValidAbsoluteUrl)
            .When(x => !string.IsNullOrWhiteSpace(x.LogoUrl))
            .WithMessage("LogoUrl must be a valid absolute URL.");

        RuleFor(x => x.SignatureLine)
            .MaximumLength(150);
    }

    private static bool BeValidAbsoluteUrl(string? url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out _);
    }
}

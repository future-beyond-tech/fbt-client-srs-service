using FluentValidation;
using SRS.Application.DTOs;

namespace SRS.Application.Validators;

public class VehicleUpdateDtoValidator : AbstractValidator<VehicleUpdateDto>
{
    public VehicleUpdateDtoValidator()
    {
        RuleFor(x => x.SellingPrice)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.Colour)
            .MaximumLength(50);

        RuleFor(x => x.RegistrationNumber)
            .MaximumLength(30)
            .Must(value => !string.IsNullOrWhiteSpace(value))
            .When(x => x.RegistrationNumber is not null)
            .WithMessage("RegistrationNumber cannot be empty when provided.");
    }
}

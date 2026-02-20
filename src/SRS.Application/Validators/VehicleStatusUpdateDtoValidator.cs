using FluentValidation;
using SRS.Application.DTOs;

namespace SRS.Application.Validators;

public class VehicleStatusUpdateDtoValidator : AbstractValidator<VehicleStatusUpdateDto>
{
    public VehicleStatusUpdateDtoValidator()
    {
        RuleFor(x => x.Status)
            .IsInEnum();
    }
}

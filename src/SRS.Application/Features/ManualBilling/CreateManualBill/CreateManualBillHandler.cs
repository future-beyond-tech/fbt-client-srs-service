using FluentValidation;
using Microsoft.Extensions.Logging;
using SRS.Application.Common;
using SRS.Application.DTOs;
using SRS.Application.Interfaces;
using SRS.Domain.Entities;
using SRS.Domain.Enums;

namespace SRS.Application.Features.ManualBilling.CreateManualBill;

public class CreateManualBillHandler(
    IManualBillRepository repository,
    IValidator<CreateManualBillCommand> validator,
    ILogger<CreateManualBillHandler> logger) : ICreateManualBillHandler
{
    public async Task<CreateManualBillResultDto> Handle(CreateManualBillCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var dto = command.Dto;
        var phone = PhoneNormalizer.NormalizeToE164(dto.Phone);

        var nextBillNumber = await repository.GetNextBillNumberAsync(cancellationToken);
        var now = DateTime.UtcNow;
        var entity = new ManualBill
        {
            BillNumber = nextBillNumber,
            BillType = "Manual",
            CustomerName = dto.CustomerName.Trim(),
            Phone = phone,
            Address = string.IsNullOrWhiteSpace(dto.Address) ? null : dto.Address.Trim(),
            PhotoUrl = dto.PhotoUrl.Trim(),
            SellerName = string.IsNullOrWhiteSpace(dto.SellerName) ? null : dto.SellerName.Trim(),
            SellerAddress = string.IsNullOrWhiteSpace(dto.SellerAddress) ? null : dto.SellerAddress.Trim(),
            CustomerNameTitle = string.IsNullOrWhiteSpace(dto.CustomerNameTitle) ? null : dto.CustomerNameTitle.Trim(),
            SellerNameTitle = string.IsNullOrWhiteSpace(dto.SellerNameTitle) ? null : dto.SellerNameTitle.Trim(),
            ItemDescription = dto.ItemDescription.Trim(),
            ChassisNumber = string.IsNullOrWhiteSpace(dto.ChassisNumber) ? null : dto.ChassisNumber.Trim(),
            EngineNumber = string.IsNullOrWhiteSpace(dto.EngineNumber) ? null : dto.EngineNumber.Trim(),
            Color = string.IsNullOrWhiteSpace(dto.Color) ? null : dto.Color.Trim(),
            Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim(),
            AmountTotal = dto.AmountTotal,
            PaymentMode = dto.PaymentMode,
            CashAmount = dto.CashAmount,
            UpiAmount = dto.UpiAmount,
            FinanceAmount = dto.FinanceAmount,
            FinanceCompany = string.IsNullOrWhiteSpace(dto.FinanceCompany) ? null : dto.FinanceCompany.Trim(),
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        var added = await repository.AddAsync(entity, cancellationToken);
        logger.LogInformation("Manual bill created. BillNumber={BillNumber}", added.BillNumber);

        return new CreateManualBillResultDto
        {
            BillNumber = added.BillNumber,
            PdfUrl = null,
            CreatedAt = added.CreatedAtUtc
        };
    }
}

using SRS.Application.DTOs;

namespace SRS.Application.Features.ManualBilling.CreateManualBill;

public interface ICreateManualBillHandler
{
    Task<CreateManualBillResultDto> Handle(CreateManualBillCommand command, CancellationToken cancellationToken = default);
}

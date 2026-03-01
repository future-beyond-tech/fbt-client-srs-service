using SRS.Application.DTOs;

namespace SRS.Application.Features.ManualBilling.GetManualBillByNumber;

public interface IGetManualBillByNumberHandler
{
    Task<ManualBillDetailDto?> Handle(GetManualBillByNumberQuery query, CancellationToken cancellationToken = default);
}

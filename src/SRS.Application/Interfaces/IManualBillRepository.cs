using SRS.Domain.Entities;

namespace SRS.Application.Interfaces;

public interface IManualBillRepository
{
    Task<ManualBill?> GetByBillNumberAsync(int billNumber, CancellationToken cancellationToken = default);
    Task<int> GetNextBillNumberAsync(CancellationToken cancellationToken = default);
    Task<ManualBill> AddAsync(ManualBill entity, CancellationToken cancellationToken = default);
}

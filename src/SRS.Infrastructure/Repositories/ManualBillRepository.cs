using Microsoft.EntityFrameworkCore;
using SRS.Application.Interfaces;
using SRS.Domain.Entities;
using SRS.Infrastructure.Persistence;

namespace SRS.Infrastructure.Repositories;

public class ManualBillRepository(AppDbContext context) : IManualBillRepository
{
    public async Task<ManualBill?> GetByBillNumberAsync(int billNumber, CancellationToken cancellationToken = default)
    {
        return await context.ManualBills
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.BillNumber == billNumber, cancellationToken);
    }

    public async Task<int> GetNextBillNumberAsync(CancellationToken cancellationToken = default)
    {
        var max = await context.ManualBills.MaxAsync(b => (int?)b.BillNumber, cancellationToken);
        return (max ?? 0) + 1;
    }

    public async Task<ManualBill> AddAsync(ManualBill entity, CancellationToken cancellationToken = default)
    {
        context.ManualBills.Add(entity);
        await context.SaveChangesAsync(cancellationToken);
        return entity;
    }
}

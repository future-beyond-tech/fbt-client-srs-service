using SRS.Application.DTOs;

namespace SRS.Application.Interfaces;

public interface IPurchaseExpenseService
{
    Task<PurchaseExpenseDto> CreateAsync(int vehicleId, PurchaseExpenseCreateDto dto);
    Task<List<PurchaseExpenseDto>> GetByVehicleIdAsync(int vehicleId);
    Task DeleteAsync(int expenseId);
}

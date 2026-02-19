using SRS.Application.DTOs;

namespace SRS.Application.Interfaces;

public interface IPurchaseService
{
    Task<PurchaseResponseDto> CreateAsync(PurchaseCreateDto dto);
    Task<List<PurchaseResponseDto>> GetAllAsync();
    Task<PurchaseResponseDto?> GetByIdAsync(int id);
}

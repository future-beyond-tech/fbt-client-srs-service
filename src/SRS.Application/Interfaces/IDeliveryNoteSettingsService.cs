using SRS.Application.DTOs;

namespace SRS.Application.Interfaces;

public interface IDeliveryNoteSettingsService
{
    Task<DeliveryNoteSettingsDto> GetAsync();
    Task<DeliveryNoteSettingsDto> UpdateAsync(UpdateDeliveryNoteSettingsDto dto);
}

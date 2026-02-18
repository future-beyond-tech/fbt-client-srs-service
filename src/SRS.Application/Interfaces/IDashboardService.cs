using SRS.Application.DTOs;

namespace SRS.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardDto> GetAsync();
}

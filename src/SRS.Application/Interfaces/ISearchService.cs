using SRS.Application.DTOs;

namespace SRS.Application.Interfaces;

public interface ISearchService
{
    Task<List<SearchResultDto>> SearchAsync(string keyword);
}

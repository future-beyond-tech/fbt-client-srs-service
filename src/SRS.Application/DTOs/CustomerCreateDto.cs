namespace SRS.Application.DTOs;

public class CustomerCreateDto
{
    public string Name { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public string? Address { get; set; }
}

namespace SRS.Application.DTOs;

public class VehiclePhotoDto
{
    public int Id { get; set; }
    public string PhotoUrl { get; set; } = null!;
    public bool IsPrimary { get; set; }
    public int DisplayOrder { get; set; }
}

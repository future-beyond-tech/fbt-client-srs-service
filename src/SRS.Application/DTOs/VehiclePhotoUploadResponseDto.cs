namespace SRS.Application.DTOs;

public class VehiclePhotoUploadResponseDto
{
    public int VehicleId { get; set; }
    public List<UploadedVehiclePhotoDto> Photos { get; set; } = [];
}

public class UploadedVehiclePhotoDto
{
    public int Id { get; set; }
    public string Url { get; set; } = null!;
    public bool IsPrimary { get; set; }
}

namespace SRS.Domain.Entities;

public class VehiclePhoto
{
    public int Id { get; set; }
    public int VehicleId { get; set; }
    public Vehicle Vehicle { get; set; } = null!;
    public string PhotoUrl { get; set; } = null!;
    public bool IsPrimary { get; set; } = false;
    public int DisplayOrder { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

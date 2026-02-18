using SRS.Domain.Common;

namespace SRS.Domain.Entities;

public class Customer : BaseEntity
{
    public string Name { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public string Address { get; set; } = null!;
    public string? PhotoUrl { get; set; }

    public ICollection<Sale> Sales { get; set; } = new List<Sale>();
}

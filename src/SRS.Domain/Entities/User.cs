using SRS.Domain.Common;
using SRS.Domain.Enums;

namespace SRS.Domain.Entities;

public class User : BaseEntity
{
    public string Username { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public UserRole Role { get; set; }
}

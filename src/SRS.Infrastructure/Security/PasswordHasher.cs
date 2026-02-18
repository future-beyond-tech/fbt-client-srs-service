namespace SRS.Infrastructure.Security;

using BCrypt.Net;
using SRS.Application.Interfaces;

public class PasswordHasher : IPasswordHasher
{
    public string Hash(string password)
    {
        return BCrypt.HashPassword(password);
    }

    public bool Verify(string password, string passwordHash)
    {
        return BCrypt.Verify(password, passwordHash);
    }
}

namespace UserManagementApi.Models;

public enum UserRole
{
    User = 0,
    Manager = 1,
    Admin = 2
}

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int Age { get; set; }
    public string? PhoneNumber { get; set; }
    public UserRole Role { get; set; } = UserRole.User;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }
}

using System.ComponentModel.DataAnnotations;
using UserManagementApi.Models;

namespace UserManagementApi.DTOs;

/// <summary>Payload for POST /api/users. Validated automatically by [ApiController].</summary>
public class CreateUserRequest
{
    [Required, StringLength(100, MinimumLength = 2)]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(254)]
    public string Email { get; set; } = string.Empty;

    [Range(18, 120, ErrorMessage = "Age must be between 18 and 120.")]
    public int Age { get; set; }

    [RegularExpression(@"^\+?[0-9\s\-]{7,15}$", ErrorMessage = "Phone number must be 7-15 digits and may start with '+'.")]
    public string? PhoneNumber { get; set; }

    [EnumDataType(typeof(UserRole))]
    public UserRole Role { get; set; } = UserRole.User;
}

/// <summary>Payload for PUT /api/users/{id}. Full replacement of the user's editable fields.</summary>
public class UpdateUserRequest : CreateUserRequest { }

public class UserResponse
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int Age { get; set; }
    public string? PhoneNumber { get; set; }
    public string Role { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }

    public static UserResponse From(User u) => new()
    {
        Id = u.Id,
        FullName = u.FullName,
        Email = u.Email,
        Age = u.Age,
        PhoneNumber = u.PhoneNumber,
        Role = u.Role.ToString(),
        CreatedAtUtc = u.CreatedAtUtc,
        UpdatedAtUtc = u.UpdatedAtUtc
    };
}

public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
}

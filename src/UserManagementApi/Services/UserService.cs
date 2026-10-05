using System.Collections.Concurrent;
using UserManagementApi.DTOs;
using UserManagementApi.Exceptions;
using UserManagementApi.Models;

namespace UserManagementApi.Services;

/// <summary>
/// In-memory implementation. Swap for an EF Core / database-backed service
/// by implementing IUserService and changing the DI registration in Program.cs.
/// </summary>
public class UserService : IUserService
{
    private readonly ConcurrentDictionary<Guid, User> _users = new();
    private readonly object _writeLock = new(); // keeps the email-uniqueness check + write atomic

    public Task<PagedResult<UserResponse>> GetAllAsync(int page, int pageSize, string? search)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        IEnumerable<User> query = _users.Values;

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(u =>
                u.FullName.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                u.Email.Contains(term, StringComparison.OrdinalIgnoreCase));
        }

        var ordered = query.OrderBy(u => u.CreatedAtUtc).ToList();

        var items = ordered
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(UserResponse.From)
            .ToList();

        return Task.FromResult(new PagedResult<UserResponse>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = ordered.Count
        });
    }

    public Task<UserResponse> GetByIdAsync(Guid id)
    {
        if (!_users.TryGetValue(id, out var user))
            throw new NotFoundException($"User '{id}' was not found.");

        return Task.FromResult(UserResponse.From(user));
    }

    public Task<UserResponse> CreateAsync(CreateUserRequest request)
    {
        lock (_writeLock)
        {
            EnsureEmailIsUnique(request.Email, excludingId: null);

            var user = new User
            {
                FullName = request.FullName.Trim(),
                Email = request.Email.Trim().ToLowerInvariant(),
                Age = request.Age,
                PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim(),
                Role = request.Role
            };

            _users[user.Id] = user;
            return Task.FromResult(UserResponse.From(user));
        }
    }

    public Task<UserResponse> UpdateAsync(Guid id, UpdateUserRequest request)
    {
        lock (_writeLock)
        {
            if (!_users.TryGetValue(id, out var user))
                throw new NotFoundException($"User '{id}' was not found.");

            EnsureEmailIsUnique(request.Email, excludingId: id);

            user.FullName = request.FullName.Trim();
            user.Email = request.Email.Trim().ToLowerInvariant();
            user.Age = request.Age;
            user.PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim();
            user.Role = request.Role;
            user.UpdatedAtUtc = DateTime.UtcNow;

            return Task.FromResult(UserResponse.From(user));
        }
    }

    public Task DeleteAsync(Guid id)
    {
        lock (_writeLock)
        {
            if (!_users.TryRemove(id, out _))
                throw new NotFoundException($"User '{id}' was not found.");
        }
        return Task.CompletedTask;
    }

    private void EnsureEmailIsUnique(string email, Guid? excludingId)
    {
        var normalized = email.Trim().ToLowerInvariant();
        var exists = _users.Values.Any(u => u.Email == normalized && u.Id != excludingId);
        if (exists)
            throw new ConflictException($"A user with email '{normalized}' already exists.");
    }
}

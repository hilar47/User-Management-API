using Microsoft.AspNetCore.Mvc;
using UserManagementApi.DTOs;
using UserManagementApi.Services;

namespace UserManagementApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class UsersController : ControllerBase
{
    private readonly IUserService _users;

    public UsersController(IUserService users) => _users = users;

    /// <summary>List users with optional search and paging.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<UserResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<UserResponse>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null)
        => Ok(await _users.GetAllAsync(page, pageSize, search));

    /// <summary>Get a single user by id.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponse>> GetById(Guid id)
        => Ok(await _users.GetByIdAsync(id));

    /// <summary>Create a new user.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserResponse>> Create([FromBody] CreateUserRequest request)
    {
        var created = await _users.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Replace an existing user's data.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserResponse>> Update(Guid id, [FromBody] UpdateUserRequest request)
        => Ok(await _users.UpdateAsync(id, request));

    /// <summary>Delete a user.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _users.DeleteAsync(id);
        return NoContent();
    }
}

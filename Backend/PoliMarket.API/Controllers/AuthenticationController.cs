using Microsoft.AspNetCore.Mvc;
using PoliMarket.Components.Authentication;
using PoliMarket.Models.Entities;

namespace PoliMarket.API.Controllers;

/// <summary>
/// Authentication and User Management Controller
/// Handles user login, logout, and user management operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Tags("Authentication")]
public class AuthenticationController : ControllerBase
{
    private readonly IAuthenticationComponent _authComponent;
    private readonly ILogger<AuthenticationController> _logger;

    public AuthenticationController(IAuthenticationComponent authComponent, ILogger<AuthenticationController> logger)
    {
        _authComponent = authComponent;
        _logger = logger;
    }

    /// <summary>
    /// User login endpoint
    /// </summary>
    /// <param name="request">Login credentials</param>
    /// <returns>Authentication result with user info and token</returns>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = HttpContext.Request.Headers.UserAgent.ToString();
        
        var result = await _authComponent.LoginAsync(request.Username, request.Password, ipAddress, userAgent);
        
        if (result.Success)
        {
            return Ok(result);
        }
        
        return BadRequest(result);
    }

    /// <summary>
    /// User logout endpoint
    /// </summary>
    /// <param name="request">Logout request with token</param>
    /// <returns>Logout result</returns>
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
    {
        var result = await _authComponent.LogoutAsync(request.Token);
        return Ok(result);
    }

    /// <summary>
    /// Validate user token
    /// </summary>
    /// <param name="token">Authentication token</param>
    /// <returns>User information if token is valid</returns>
    [HttpGet("validate")]
    public async Task<IActionResult> ValidateToken([FromQuery] string token)
    {
        var result = await _authComponent.ValidateTokenAsync(token);
        
        if (result.Success)
        {
            return Ok(result);
        }
        
        return Unauthorized(result);
    }

    /// <summary>
    /// Create new user (Admin only)
    /// </summary>
    /// <param name="request">User creation request</param>
    /// <returns>Created user information</returns>
    [HttpPost("users")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        // TODO: Add authorization check for admin role
        var result = await _authComponent.CreateUserAsync(request);
        
        if (result.Success)
        {
            return CreatedAtAction(nameof(GetUser), new { id = result.Data?.Id }, result);
        }
        
        return BadRequest(result);
    }

    /// <summary>
    /// Get user by ID
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>User information</returns>
    [HttpGet("users/{id}")]
    public async Task<IActionResult> GetUser(string id)
    {
        var result = await _authComponent.GetUserByIdAsync(id);
        
        if (result.Success)
        {
            return Ok(result);
        }
        
        return NotFound(result);
    }

    /// <summary>
    /// Get all users with pagination
    /// </summary>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>List of users</returns>
    [HttpGet("users")]
    public async Task<IActionResult> GetUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _authComponent.GetUsersAsync(page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Update user information
    /// </summary>
    /// <param name="id">User ID</param>
    /// <param name="request">Update request</param>
    /// <returns>Updated user information</returns>
    [HttpPut("users/{id}")]
    public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateUserRequest request)
    {
        var result = await _authComponent.UpdateUserAsync(id, request);
        
        if (result.Success)
        {
            return Ok(result);
        }
        
        return BadRequest(result);
    }

    /// <summary>
    /// Change user password
    /// </summary>
    /// <param name="id">User ID</param>
    /// <param name="request">Password change request</param>
    /// <returns>Password change result</returns>
    [HttpPost("users/{id}/change-password")]
    public async Task<IActionResult> ChangePassword(string id, [FromBody] ChangePasswordRequest request)
    {
        var result = await _authComponent.ChangePasswordAsync(id, request.CurrentPassword, request.NewPassword);

        if (result.Success)
        {
            return Ok(result);
        }

        return BadRequest(result);
    }

    /// <summary>
    /// Get all users for debugging (temporary endpoint)
    /// </summary>
    /// <returns>List of all users</returns>
    [HttpGet("debug/users")]
    public async Task<IActionResult> GetAllUsers()
    {
        var result = await _authComponent.GetUsersAsync(1, 100);
        return Ok(result);
    }
}

/// <summary>
/// Login request model
/// </summary>
public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// Logout request model
/// </summary>
public class LogoutRequest
{
    public string Token { get; set; } = string.Empty;
}

/// <summary>
/// Change password request model
/// </summary>
public class ChangePasswordRequest
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

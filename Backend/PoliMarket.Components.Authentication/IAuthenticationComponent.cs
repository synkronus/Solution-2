using PoliMarket.Models.Entities;
using PoliMarket.Models.Common;
using PoliMarket.Contracts;

namespace PoliMarket.Components.Authentication;

/// <summary>
/// Interface for user authentication and authorization component
/// </summary>
public interface IAuthenticationComponent
{
    /// <summary>
    /// Authenticate user with username and password
    /// </summary>
    Task<ApiResponse<AuthenticationResult>> LoginAsync(string username, string password, string? ipAddress = null, string? userAgent = null);
    
    /// <summary>
    /// Logout user and invalidate session
    /// </summary>
    Task<ApiResponse<bool>> LogoutAsync(string token);
    
    /// <summary>
    /// Validate user session token
    /// </summary>
    Task<ApiResponse<Usuario>> ValidateTokenAsync(string token);
    
    /// <summary>
    /// Create new user account
    /// </summary>
    Task<ApiResponse<Usuario>> CreateUserAsync(CreateUserRequest request);
    
    /// <summary>
    /// Update user information
    /// </summary>
    Task<ApiResponse<Usuario>> UpdateUserAsync(string userId, UpdateUserRequest request);
    
    /// <summary>
    /// Get user by ID
    /// </summary>
    Task<ApiResponse<Usuario>> GetUserByIdAsync(string userId);
    
    /// <summary>
    /// Get all users with pagination
    /// </summary>
    Task<ApiResponse<List<Usuario>>> GetUsersAsync(int page = 1, int pageSize = 10);
    
    /// <summary>
    /// Change user password
    /// </summary>
    Task<ApiResponse<bool>> ChangePasswordAsync(string userId, string currentPassword, string newPassword);
    
    /// <summary>
    /// Check if user has specific role
    /// </summary>
    bool HasRole(Usuario user, UserRole role);
    
    /// <summary>
    /// Check if user can access resource
    /// </summary>
    bool CanAccess(Usuario user, string resource, string action);
}

/// <summary>
/// Authentication result containing user info and token
/// </summary>
public class AuthenticationResult
{
    public Usuario Usuario { get; set; } = null!;
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}

/// <summary>
/// Request model for creating new user
/// </summary>
public class CreateUserRequest
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public UserRole Rol { get; set; }
    public string? VendedorId { get; set; }
    public string? EmpleadoRHId { get; set; }
}

/// <summary>
/// Request model for updating user
/// </summary>
public class UpdateUserRequest
{
    public string? Email { get; set; }
    public string? Nombre { get; set; }
    public string? Apellido { get; set; }
    public UserRole? Rol { get; set; }
    public string? VendedorId { get; set; }
    public string? EmpleadoRHId { get; set; }
    public bool? Activo { get; set; }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PoliMarket.Components.Infrastructure.Data;
using PoliMarket.Models.Entities;
using PoliMarket.Models.Common;
using PoliMarket.Contracts;
using BCrypt.Net;

namespace PoliMarket.Components.Authentication;

/// <summary>
/// Implementation of user authentication and authorization component
/// </summary>
public class AuthenticationComponent : IAuthenticationComponent
{
    private readonly PoliMarketDbContext _context;
    private readonly ILogger<AuthenticationComponent> _logger;

    public AuthenticationComponent(PoliMarketDbContext context, ILogger<AuthenticationComponent> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ApiResponse<AuthenticationResult>> LoginAsync(string username, string password, string? ipAddress = null, string? userAgent = null)
    {
        try
        {
            _logger.LogInformation("Login attempt for user: {Username}", username);

            var user = await _context.Usuarios
                .Include(u => u.Vendedor)
                .Include(u => u.EmpleadoRH)
                .FirstOrDefaultAsync(u => u.Username == username && u.Activo);

            if (user == null)
            {
                _logger.LogWarning("Login failed: User not found - {Username}", username);
                return ApiResponse<AuthenticationResult>.ErrorResult("Usuario o contraseña incorrectos");
            }

            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                _logger.LogWarning("Login failed: Invalid password for user - {Username}", username);
                return ApiResponse<AuthenticationResult>.ErrorResult("Usuario o contraseña incorrectos");
            }

            // Create session
            var session = new UserSession
            {
                Id = Guid.NewGuid().ToString(),
                UserId = user.Id,
                Token = GenerateToken(),
                FechaExpiracion = DateTime.UtcNow.AddHours(8),
                IPAddress = ipAddress,
                UserAgent = userAgent,
                Usuario = user
            };

            _context.UserSessions.Add(session);

            // Update last login
            user.UltimoLogin = DateTime.UtcNow;
            user.FechaActualizacion = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var result = new AuthenticationResult
            {
                Usuario = user,
                Token = session.Token,
                ExpiresAt = session.FechaExpiracion
            };

            _logger.LogInformation("Login successful for user: {Username}", username);
            return ApiResponse<AuthenticationResult>.SuccessResult(result, "Login exitoso");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for user: {Username}", username);
            return ApiResponse<AuthenticationResult>.ErrorResult("Error interno del servidor", ex.Message);
        }
    }

    public async Task<ApiResponse<bool>> LogoutAsync(string token)
    {
        try
        {
            var session = await _context.UserSessions
                .FirstOrDefaultAsync(s => s.Token == token && s.Activa);

            if (session != null)
            {
                session.Activa = false;
                await _context.SaveChangesAsync();
                _logger.LogInformation("User logged out successfully");
            }

            return ApiResponse<bool>.SuccessResult(true, "Logout exitoso");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            return ApiResponse<bool>.ErrorResult("Error interno del servidor", ex.Message);
        }
    }

    public async Task<ApiResponse<Usuario>> ValidateTokenAsync(string token)
    {
        try
        {
            var session = await _context.UserSessions
                .Include(s => s.Usuario)
                .ThenInclude(u => u.Vendedor)
                .Include(s => s.Usuario)
                .ThenInclude(u => u.EmpleadoRH)
                .FirstOrDefaultAsync(s => s.Token == token && s.Activa && s.FechaExpiracion > DateTime.UtcNow);

            if (session == null)
            {
                return ApiResponse<Usuario>.ErrorResult("Token inválido o expirado");
            }

            return ApiResponse<Usuario>.SuccessResult(session.Usuario, "Token válido");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating token");
            return ApiResponse<Usuario>.ErrorResult("Error interno del servidor", ex.Message);
        }
    }

    public async Task<ApiResponse<Usuario>> CreateUserAsync(CreateUserRequest request)
    {
        try
        {
            // Check if username or email already exists
            var existingUser = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Username == request.Username || u.Email == request.Email);

            if (existingUser != null)
            {
                return ApiResponse<Usuario>.ErrorResult("El usuario o email ya existe");
            }

            var user = new Usuario
            {
                Id = Guid.NewGuid().ToString(),
                Username = request.Username,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Nombre = request.Nombre,
                Apellido = request.Apellido,
                Rol = request.Rol,
                VendedorId = request.VendedorId,
                EmpleadoRHId = request.EmpleadoRHId,
                Activo = true,
                FechaCreacion = DateTime.UtcNow
            };

            _context.Usuarios.Add(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation("User created successfully: {Username}", request.Username);
            return ApiResponse<Usuario>.SuccessResult(user, "Usuario creado exitosamente");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user: {Username}", request.Username);
            return ApiResponse<Usuario>.ErrorResult("Error interno del servidor", ex.Message);
        }
    }

    public async Task<ApiResponse<Usuario>> UpdateUserAsync(string userId, UpdateUserRequest request)
    {
        try
        {
            var user = await _context.Usuarios.FindAsync(userId);
            if (user == null)
            {
                return ApiResponse<Usuario>.ErrorResult("Usuario no encontrado");
            }

            if (!string.IsNullOrEmpty(request.Email)) user.Email = request.Email;
            if (!string.IsNullOrEmpty(request.Nombre)) user.Nombre = request.Nombre;
            if (!string.IsNullOrEmpty(request.Apellido)) user.Apellido = request.Apellido;
            if (request.Rol.HasValue) user.Rol = request.Rol.Value;
            if (request.VendedorId != null) user.VendedorId = request.VendedorId;
            if (request.EmpleadoRHId != null) user.EmpleadoRHId = request.EmpleadoRHId;
            if (request.Activo.HasValue) user.Activo = request.Activo.Value;

            user.FechaActualizacion = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("User updated successfully: {UserId}", userId);
            return ApiResponse<Usuario>.SuccessResult(user, "Usuario actualizado exitosamente");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user: {UserId}", userId);
            return ApiResponse<Usuario>.ErrorResult("Error interno del servidor", ex.Message);
        }
    }

    public async Task<ApiResponse<Usuario>> GetUserByIdAsync(string userId)
    {
        try
        {
            var user = await _context.Usuarios
                .Include(u => u.Vendedor)
                .Include(u => u.EmpleadoRH)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return ApiResponse<Usuario>.ErrorResult("Usuario no encontrado");
            }

            return ApiResponse<Usuario>.SuccessResult(user, "Usuario encontrado");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user: {UserId}", userId);
            return ApiResponse<Usuario>.ErrorResult("Error interno del servidor", ex.Message);
        }
    }

    public async Task<ApiResponse<List<Usuario>>> GetUsersAsync(int page = 1, int pageSize = 10)
    {
        try
        {
            var users = await _context.Usuarios
                .Include(u => u.Vendedor)
                .Include(u => u.EmpleadoRH)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return ApiResponse<List<Usuario>>.SuccessResult(users, $"Se encontraron {users.Count} usuarios");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting users");
            return ApiResponse<List<Usuario>>.ErrorResult("Error interno del servidor", ex.Message);
        }
    }

    public async Task<ApiResponse<bool>> ChangePasswordAsync(string userId, string currentPassword, string newPassword)
    {
        try
        {
            var user = await _context.Usuarios.FindAsync(userId);
            if (user == null)
            {
                return ApiResponse<bool>.ErrorResult("Usuario no encontrado");
            }

            if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
            {
                return ApiResponse<bool>.ErrorResult("Contraseña actual incorrecta");
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.FechaActualizacion = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Password changed successfully for user: {UserId}", userId);
            return ApiResponse<bool>.SuccessResult(true, "Contraseña cambiada exitosamente");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password for user: {UserId}", userId);
            return ApiResponse<bool>.ErrorResult("Error interno del servidor", ex.Message);
        }
    }

    public bool HasRole(Usuario user, UserRole role)
    {
        return user.Rol == role || user.Rol == UserRole.Admin;
    }

    public bool CanAccess(Usuario user, string resource, string action)
    {
        // Admin can access everything
        if (user.Rol == UserRole.Admin) return true;

        // Role-based access control
        return resource.ToLower() switch
        {
            "users" => user.Rol == UserRole.Admin,
            "sales" => user.Rol is UserRole.SalesRep or UserRole.Admin,
            "inventory" => user.Rol is UserRole.InventoryManager or UserRole.Admin,
            "hr" => user.Rol is UserRole.HRManager or UserRole.Admin,
            "deliveries" => user.Rol is UserRole.DeliveryManager or UserRole.Admin,
            _ => false
        };
    }

    private string GenerateToken()
    {
        return Convert.ToBase64String(Guid.NewGuid().ToByteArray()) + 
               Convert.ToBase64String(Guid.NewGuid().ToByteArray());
    }
}

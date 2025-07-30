namespace PoliMarket.Models.Entities;

/// <summary>
/// Represents a system user with role-based access
/// </summary>
public class Usuario
{
    public string Id { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public UserRole Rol { get; set; } = UserRole.SalesRep;
    public bool Activo { get; set; } = true;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime? FechaActualizacion { get; set; }
    public DateTime? UltimoLogin { get; set; }
    
    // Role-specific IDs
    public string? VendedorId { get; set; }
    public string? EmpleadoRHId { get; set; }
    
    // Navigation properties
    public Vendedor? Vendedor { get; set; }
    public EmpleadoRH? EmpleadoRH { get; set; }
}

/// <summary>
/// User roles in the system
/// </summary>
public enum UserRole
{
    Admin = 1,
    HRManager = 2,
    SalesRep = 3,
    InventoryManager = 4,
    DeliveryManager = 5
}

/// <summary>
/// Represents a user session for authentication
/// </summary>
public class UserSession
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime FechaExpiracion { get; set; }
    public bool Activa { get; set; } = true;
    public string? IPAddress { get; set; }
    public string? UserAgent { get; set; }
    
    // Navigation properties
    public Usuario Usuario { get; set; } = null!;
}

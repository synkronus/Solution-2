namespace PoliMarket.Models.Entities;

/// <summary>
/// Represents a seller in the system (RF1)
/// </summary>
public class Vendedor
{
    public string CodigoVendedor { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Territorio { get; set; } = string.Empty;
    public double Comision { get; set; }
    public bool Autorizado { get; set; }
    public DateTime FechaAutorizacion { get; set; }
    public string EmpleadoRHAutorizo { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime? FechaActualizacion { get; set; }
    public bool Activo { get; set; } = true;
}

/// <summary>
/// Represents an HR employee who can authorize sellers
/// </summary>
public class EmpleadoRH
{
    public string Id { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Cargo { get; set; } = string.Empty;
    public string Departamento { get; set; } = string.Empty;
    public DateTime FechaIngreso { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Request model for seller authorization
/// </summary>
public class AuthorizationRequest
{
    public string CodigoVendedor { get; set; } = string.Empty;
    public string EmpleadoRH { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Territorio { get; set; } = string.Empty;
    public double Comision { get; set; }
}

/// <summary>
/// Response model for seller validation
/// </summary>
public class ValidationResponse
{
    public bool IsValid { get; set; }
    public string Reason { get; set; } = string.Empty;
    public Vendedor? Vendedor { get; set; }
}

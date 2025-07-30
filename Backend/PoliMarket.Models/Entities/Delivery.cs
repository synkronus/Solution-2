namespace PoliMarket.Models.Entities;

/// <summary>
/// Represents a delivery (RF4)
/// </summary>
public class Entrega : IAuditableEntity
{
    public string Id { get; set; } = string.Empty;
    public string IdVenta { get; set; } = string.Empty;
    public DateTime FechaProgramada { get; set; }
    public string Direccion { get; set; } = string.Empty;
    public string Cliente { get; set; } = string.Empty;
    public string Estado { get; set; } = "Programada"; // Programada, EnRuta, Entregada, Cancelada
    public string Transportista { get; set; } = string.Empty;
    public DateTime? FechaEntrega { get; set; }
    public string? Observaciones { get; set; }
    public double? Latitud { get; set; }
    public double? Longitud { get; set; }

    // Audit columns
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime? FechaActualizacion { get; set; }
    public string CreadoPor { get; set; } = string.Empty;
    public string? ActualizadoPor { get; set; }

    // Navigation properties
    public Venta? Venta { get; set; }
    public List<SeguimientoEntrega> Seguimientos { get; set; } = new();
}

/// <summary>
/// Represents delivery tracking information
/// </summary>
public class SeguimientoEntrega : IAuditableEntity
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string IdEntrega { get; set; } = string.Empty;
    public DateTime FechaHora { get; set; } = DateTime.UtcNow;
    public string Estado { get; set; } = string.Empty;
    public string Ubicacion { get; set; } = string.Empty;
    public string? Comentarios { get; set; }
    public double? Latitud { get; set; }
    public double? Longitud { get; set; }

    // Audit columns
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime? FechaActualizacion { get; set; }
    public string CreadoPor { get; set; } = string.Empty;
    public string? ActualizadoPor { get; set; }

    // Navigation properties
    public Entrega? Entrega { get; set; }
}

/// <summary>
/// Represents a delivery route
/// </summary>
public class RutaEntrega
{
    public string Id { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
    public string TransportistaId { get; set; } = string.Empty;
    public string Estado { get; set; } = "Planificada"; // Planificada, EnProgreso, Completada
    public List<string> EntregasIds { get; set; } = new();
    public double DistanciaTotal { get; set; }
    public TimeSpan TiempoEstimado { get; set; }
    public DateTime? HoraInicio { get; set; }
    public DateTime? HoraFin { get; set; }
}

/// <summary>
/// Represents a delivery person/driver
/// </summary>
public class Transportista
{
    public string Id { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string LicenciaConducir { get; set; } = string.Empty;
    public string VehiculoPlaca { get; set; } = string.Empty;
    public string VehiculoTipo { get; set; } = string.Empty;
    public bool Disponible { get; set; } = true;
    public bool Activo { get; set; } = true;
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public List<RutaEntrega> Rutas { get; set; } = new();
}

/// <summary>
/// Request model for scheduling a delivery
/// </summary>
public class ScheduleDeliveryRequest
{
    public string IdVenta { get; set; } = string.Empty;
    public DateTime FechaProgramada { get; set; }
    public string Direccion { get; set; } = string.Empty;
    public string Cliente { get; set; } = string.Empty;
    public string? TransportistaId { get; set; }
    public string? Observaciones { get; set; }
}

/// <summary>
/// Request model for updating delivery status
/// </summary>
public class UpdateDeliveryStatusRequest
{
    public string IdEntrega { get; set; } = string.Empty;
    public string NuevoEstado { get; set; } = string.Empty;
    public string? Comentarios { get; set; }
    public double? Latitud { get; set; }
    public double? Longitud { get; set; }
}

/// <summary>
/// Response model for delivery operations
/// </summary>
public class DeliveryOperationResponse
{
    public string EntregaId { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public DateTime FechaOperacion { get; set; } = DateTime.UtcNow;
    public string Mensaje { get; set; } = string.Empty;
}

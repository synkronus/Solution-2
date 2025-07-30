using System.Text.Json.Serialization;

namespace PoliMarket.Models.Entities;

/// <summary>
/// Represents a product in the inventory (RF3)
/// </summary>
public class Producto
{
    public string Id { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public double Precio { get; set; }
    public string Categoria { get; set; } = string.Empty;
    public bool Estado { get; set; } = true;
    public int Stock { get; set; }
    public int StockMinimo { get; set; } = 10;
    public int StockMaximo { get; set; } = 1000;
    public string UnidadMedida { get; set; } = "Unidad";
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime? FechaActualizacion { get; set; }
    
    // Navigation properties
    [JsonIgnore] // Prevent circular reference during JSON serialization
    public List<MovimientoInventario> Movimientos { get; set; } = new();
    [JsonIgnore] // Prevent circular reference during JSON serialization
    public List<DetalleVenta> DetallesVenta { get; set; } = new();
}

/// <summary>
/// Represents an inventory movement
/// </summary>
public class MovimientoInventario
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string IdProducto { get; set; } = string.Empty;
    public string TipoMovimiento { get; set; } = string.Empty; // "Entrada", "Salida", "Ajuste"
    public int Cantidad { get; set; }
    public int StockAnterior { get; set; }
    public int StockNuevo { get; set; }
    public string Motivo { get; set; } = string.Empty;
    public string? DocumentoReferencia { get; set; }
    public DateTime FechaMovimiento { get; set; } = DateTime.UtcNow;
    public string UsuarioResponsable { get; set; } = string.Empty;
    
    // Navigation properties
    [JsonIgnore] // Prevent circular reference during JSON serialization
    public Producto? Producto { get; set; }
}

/// <summary>
/// Represents inventory availability information
/// </summary>
public class DisponibilidadInventario
{
    public string IdProducto { get; set; } = string.Empty;
    public string NombreProducto { get; set; } = string.Empty;
    public int StockActual { get; set; }
    public int StockDisponible { get; set; }
    public int StockReservado { get; set; }
    public bool DisponibleParaVenta { get; set; }
    public string Estado { get; set; } = string.Empty;
    public DateTime FechaConsulta { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Request model for stock availability check
/// </summary>
public class AvailabilityCheckRequest
{
    public string IdProducto { get; set; } = string.Empty;
    public int CantidadRequerida { get; set; }
}

/// <summary>
/// Request model for stock update
/// </summary>
public class StockUpdateRequest
{
    public string IdProducto { get; set; } = string.Empty;
    public string TipoMovimiento { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public string Motivo { get; set; } = string.Empty;
    public string? DocumentoReferencia { get; set; }
    public string UsuarioResponsable { get; set; } = string.Empty;
}

/// <summary>
/// Response model for stock operations
/// </summary>
public class StockOperationResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int StockAnterior { get; set; }
    public int StockNuevo { get; set; }
    public string MovimientoId { get; set; } = string.Empty;
    public DateTime FechaOperacion { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Represents a stock alert
/// </summary>
public class AlertaStock
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string IdProducto { get; set; } = string.Empty;
    public string NombreProducto { get; set; } = string.Empty;
    public string TipoAlerta { get; set; } = string.Empty; // "StockBajo", "StockAgotado", "StockExcesivo"
    public int StockActual { get; set; }
    public int StockMinimo { get; set; }
    public int StockMaximo { get; set; }
    public string Mensaje { get; set; } = string.Empty;
    public DateTime FechaAlerta { get; set; } = DateTime.UtcNow;
    public bool Procesada { get; set; } = false;
    public DateTime? FechaProcesamiento { get; set; }
}

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PoliMarket.Models.Entities;

/// <summary>
/// Represents a sale transaction (RF2)
/// </summary>
public class Venta : IAuditableEntity
{
    public string Id { get; set; } = string.Empty;
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
    public string IdCliente { get; set; } = string.Empty;
    public string IdVendedor { get; set; } = string.Empty;
    public double Total { get; set; }
    public string Estado { get; set; } = "Pendiente";
    public List<DetalleVenta> Detalles { get; set; } = new();
    public string? Observaciones { get; set; }

    // Audit columns
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime? FechaActualizacion { get; set; }
    public string CreadoPor { get; set; } = string.Empty;
    public string? ActualizadoPor { get; set; }

    // Navigation properties
    public Cliente? Cliente { get; set; }
    public Vendedor? Vendedor { get; set; }
}

/// <summary>
/// Represents a sale detail line item
/// </summary>
public class DetalleVenta
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string IdVenta { get; set; } = string.Empty;
    public string IdProducto { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public double Precio { get; set; }
    public double Descuento { get; set; } = 0;
    public double Subtotal => (Cantidad * Precio) - Descuento;
    
    // Navigation properties
    [JsonIgnore] // Prevent circular reference during JSON serialization
    public Venta? Venta { get; set; }
    public Producto? Producto { get; set; }
}

/// <summary>
/// Represents a customer
/// </summary>
public class Cliente
{
    public string Id { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string TipoCliente { get; set; } = "Regular";
    public double LimiteCredito { get; set; } = 0;
    public bool Activo { get; set; } = true;
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
    public DateTime? FechaActualizacion { get; set; }
    
    // Navigation properties
    [JsonIgnore] // Prevent circular reference during JSON serialization
    public List<Venta> Ventas { get; set; } = new();
}

/// <summary>
/// Request model for creating a sale
/// </summary>
public class CreateSaleRequest
{
    [Required(ErrorMessage = "El ID del cliente es requerido")]
    [StringLength(50, ErrorMessage = "El ID del cliente no puede exceder 50 caracteres")]
    public string IdCliente { get; set; } = string.Empty;

    [Required(ErrorMessage = "El ID del vendedor es requerido")]
    [StringLength(20, ErrorMessage = "El ID del vendedor no puede exceder 20 caracteres")]
    public string IdVendedor { get; set; } = string.Empty;

    [Required(ErrorMessage = "Los detalles de la venta son requeridos")]
    [MinLength(1, ErrorMessage = "Debe incluir al menos un producto en la venta")]
    public List<SaleDetailRequest> Detalles { get; set; } = new();

    [StringLength(500, ErrorMessage = "Las observaciones no pueden exceder 500 caracteres")]
    public string? Observaciones { get; set; }
}

/// <summary>
/// Request model for sale detail
/// </summary>
public class SaleDetailRequest
{
    [Required(ErrorMessage = "El ID del producto es requerido")]
    [StringLength(50, ErrorMessage = "El ID del producto no puede exceder 50 caracteres")]
    public string IdProducto { get; set; } = string.Empty;

    [Required(ErrorMessage = "La cantidad es requerida")]
    [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
    public int Cantidad { get; set; }

    [Required(ErrorMessage = "El precio es requerido")]
    [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0")]
    public double Precio { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "El descuento no puede ser negativo")]
    public double Descuento { get; set; } = 0;
}

/// <summary>
/// Response model for sale processing
/// </summary>
public class SaleProcessingResponse
{
    public string VentaId { get; set; } = string.Empty;
    public double Total { get; set; }
    public string Estado { get; set; } = string.Empty;
    public DateTime FechaProcesamiento { get; set; } = DateTime.UtcNow;
    public List<string> Warnings { get; set; } = new();
}

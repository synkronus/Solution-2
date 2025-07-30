namespace PoliMarket.Models.Entities;

/// <summary>
/// Represents a supplier (RF5)
/// </summary>
public class Proveedor
{
    public string Id { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Contacto { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
    public string TipoProveedor { get; set; } = "Regular";
    public double Calificacion { get; set; } = 0;
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
    public DateTime? FechaActualizacion { get; set; }
    
    // Navigation properties
    public List<OrdenCompra> OrdenesCompra { get; set; } = new();
}

/// <summary>
/// Represents a purchase order
/// </summary>
public class OrdenCompra
{
    public string Id { get; set; } = string.Empty;
    public string IdProveedor { get; set; } = string.Empty;
    public DateTime FechaOrden { get; set; } = DateTime.UtcNow;
    public DateTime? FechaEntregaEsperada { get; set; }
    public string Estado { get; set; } = "Pendiente"; // Pendiente, Aprobada, Enviada, Recibida, Cancelada
    public double Total { get; set; }
    public string? Observaciones { get; set; }
    public List<DetalleOrdenCompra> Detalles { get; set; } = new();
    public string UsuarioCreador { get; set; } = string.Empty;
    public string? UsuarioAprobador { get; set; }
    public DateTime? FechaAprobacion { get; set; }
    
    // Navigation properties
    public Proveedor? Proveedor { get; set; }
}

/// <summary>
/// Represents a purchase order detail line item
/// </summary>
public class DetalleOrdenCompra
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string IdOrdenCompra { get; set; } = string.Empty;
    public string IdProducto { get; set; } = string.Empty;
    public int CantidadSolicitada { get; set; }
    public int CantidadRecibida { get; set; } = 0;
    public double PrecioUnitario { get; set; }
    public double Subtotal => CantidadSolicitada * PrecioUnitario;
    
    // Navigation properties
    public OrdenCompra? OrdenCompra { get; set; }
    public Producto? Producto { get; set; }
}

/// <summary>
/// Request model for creating a purchase order
/// </summary>
public class CreatePurchaseOrderRequest
{
    public string IdProveedor { get; set; } = string.Empty;
    public DateTime? FechaEntregaEsperada { get; set; }
    public List<PurchaseOrderDetailRequest> Detalles { get; set; } = new();
    public string? Observaciones { get; set; }
    public string UsuarioCreador { get; set; } = string.Empty;
}

/// <summary>
/// Request model for purchase order detail
/// </summary>
public class PurchaseOrderDetailRequest
{
    public string IdProducto { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public double PrecioUnitario { get; set; }
}

/// <summary>
/// Response model for purchase order processing
/// </summary>
public class PurchaseOrderResponse
{
    public string OrdenId { get; set; } = string.Empty;
    public double Total { get; set; }
    public string Estado { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public List<string> Warnings { get; set; } = new();
}

/// <summary>
/// Request model for supplier evaluation
/// </summary>
public class SupplierEvaluationRequest
{
    public string IdProveedor { get; set; } = string.Empty;
    public double CalificacionCalidad { get; set; }
    public double CalificacionTiempo { get; set; }
    public double CalificacionPrecio { get; set; }
    public string? Comentarios { get; set; }
    public string EvaluadorId { get; set; } = string.Empty;
}

/// <summary>
/// Response model for supplier performance
/// </summary>
public class SupplierPerformanceResponse
{
    public string IdProveedor { get; set; } = string.Empty;
    public string NombreProveedor { get; set; } = string.Empty;
    public double CalificacionPromedio { get; set; }
    public int TotalOrdenes { get; set; }
    public int OrdenesATiempo { get; set; }
    public double PorcentajePuntualidad { get; set; }
    public DateTime UltimaEvaluacion { get; set; }
}

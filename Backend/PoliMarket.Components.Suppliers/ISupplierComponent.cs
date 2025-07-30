using PoliMarket.Models.Entities;

namespace PoliMarket.Components.Suppliers;

/// <summary>
/// Interface for Supplier Component (RF5)
/// Defines supplier management operations
/// </summary>
public interface ISupplierComponent
{
    // Basic CRUD Operations
    Task<List<Proveedor>> GetAllSuppliersAsync();
    Task<Proveedor?> GetSupplierByIdAsync(string id);
    Task<Proveedor> CreateSupplierAsync(Proveedor supplier);
    Task<Proveedor> UpdateSupplierAsync(Proveedor supplier);
    Task<bool> DeactivateSupplierAsync(string id);
    
    // Query Operations
    Task<List<Proveedor>> GetActiveSuppliersAsync();
    Task<List<Proveedor>> GetSuppliersByCategoryAsync(string category);
    
    // Performance Operations
    Task<bool> EvaluateSupplierAsync(string id, double rating, string? comments = null);
    Task<SupplierPerformanceMetrics?> GetSupplierPerformanceAsync(string id);
}

/// <summary>
/// Supplier performance metrics
/// </summary>
public class SupplierPerformanceMetrics
{
    public string Id { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public int TotalPedidos { get; set; }
    public int PedidosATiempo { get; set; }
    public double CalificacionPromedio { get; set; }
    public double TiempoEntregaPromedio { get; set; }
    public DateTime? UltimaEvaluacion { get; set; }
}

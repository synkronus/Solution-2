using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PoliMarket.Components.Infrastructure.Data;
using PoliMarket.Models.Entities;

namespace PoliMarket.Components.Suppliers;

/// <summary>
/// Supplier Component Implementation (RF5)
/// Handles supplier management with database persistence
/// </summary>
public class SupplierComponent : ISupplierComponent
{
    private readonly PoliMarketDbContext _context;
    private readonly ILogger<SupplierComponent> _logger;

    public SupplierComponent(PoliMarketDbContext context, ILogger<SupplierComponent> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<Proveedor>> GetAllSuppliersAsync()
    {
        _logger.LogDebug("Getting all suppliers");
        return await _context.Proveedores
            .OrderBy(p => p.Nombre)
            .ToListAsync();
    }

    public async Task<Proveedor?> GetSupplierByIdAsync(string id)
    {
        _logger.LogDebug("Getting supplier by ID: {SupplierId}", id);
        return await _context.Proveedores
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Proveedor> CreateSupplierAsync(Proveedor supplier)
    {
        _logger.LogDebug("Creating supplier: {SupplierName}", supplier.Nombre);
        
        supplier.Id = Guid.NewGuid().ToString();
        supplier.FechaRegistro = DateTime.UtcNow;
        supplier.Activo = true;
        
        _context.Proveedores.Add(supplier);
        await _context.SaveChangesAsync();
        
        return supplier;
    }

    public async Task<Proveedor> UpdateSupplierAsync(Proveedor supplier)
    {
        _logger.LogDebug("Updating supplier: {SupplierId}", supplier.Id);
        
        var existing = await _context.Proveedores.FindAsync(supplier.Id);
        if (existing == null)
        {
            throw new ArgumentException($"Supplier with ID {supplier.Id} not found");
        }

        existing.Nombre = supplier.Nombre;
        existing.Contacto = supplier.Contacto;
        existing.Direccion = supplier.Direccion;
        existing.Telefono = supplier.Telefono;
        existing.Email = supplier.Email;
        existing.TipoProveedor = supplier.TipoProveedor;
        existing.FechaActualizacion = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeactivateSupplierAsync(string id)
    {
        _logger.LogDebug("Deactivating supplier: {SupplierId}", id);
        
        var supplier = await _context.Proveedores.FindAsync(id);
        if (supplier == null)
        {
            return false;
        }

        supplier.Activo = false;
        supplier.FechaActualizacion = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        
        return true;
    }

    public async Task<List<Proveedor>> GetActiveSuppliersAsync()
    {
        _logger.LogDebug("Getting active suppliers");
        return await _context.Proveedores
            .Where(p => p.Activo)
            .OrderBy(p => p.Nombre)
            .ToListAsync();
    }

    public async Task<List<Proveedor>> GetSuppliersByCategoryAsync(string category)
    {
        _logger.LogDebug("Getting suppliers by category: {Category}", category);
        return await _context.Proveedores
            .Where(p => p.TipoProveedor.Equals(category, StringComparison.OrdinalIgnoreCase))
            .OrderBy(p => p.Nombre)
            .ToListAsync();
    }

    public async Task<bool> EvaluateSupplierAsync(string id, double rating, string? comments = null)
    {
        _logger.LogDebug("Evaluating supplier: {SupplierId} with rating: {Rating}", id, rating);
        
        var supplier = await _context.Proveedores.FindAsync(id);
        if (supplier == null)
        {
            return false;
        }

        supplier.Calificacion = rating;
        supplier.FechaActualizacion = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        
        return true;
    }

    public async Task<SupplierPerformanceMetrics?> GetSupplierPerformanceAsync(string id)
    {
        _logger.LogDebug("Getting performance metrics for supplier: {SupplierId}", id);
        
        var supplier = await _context.Proveedores.FindAsync(id);
        if (supplier == null)
        {
            return null;
        }

        // Calculate performance metrics based on purchase orders
        var orders = await _context.OrdenesCompra
            .Where(o => o.IdProveedor == id)
            .ToListAsync();

        var onTimeOrders = orders.Count(o => o.Estado == "Recibida");
        var avgDeliveryTime = orders.Any() ? orders.Average(o => 
            o.FechaEntregaEsperada.HasValue ? 
            (o.FechaEntregaEsperada.Value - o.FechaOrden).TotalDays : 0) : 0;

        return new SupplierPerformanceMetrics
        {
            Id = supplier.Id,
            Nombre = supplier.Nombre,
            TotalPedidos = orders.Count,
            PedidosATiempo = onTimeOrders,
            CalificacionPromedio = supplier.Calificacion,
            TiempoEntregaPromedio = avgDeliveryTime,
            UltimaEvaluacion = supplier.FechaActualizacion
        };
    }
}

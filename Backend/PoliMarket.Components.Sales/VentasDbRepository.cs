using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PoliMarket.Components.Infrastructure.Data;
using PoliMarket.Models.Entities;

namespace PoliMarket.Components.Sales;

/// <summary>
/// Database-backed repository implementation for Sales Component
/// Uses Entity Framework Core with SQLite database
/// </summary>
public class VentasDbRepository : IVentasRepository
{
    private readonly PoliMarketDbContext _context;
    private readonly ILogger<VentasDbRepository> _logger;

    public VentasDbRepository(PoliMarketDbContext context, ILogger<VentasDbRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    #region Sale Operations

    public async Task<Venta?> GetSaleByIdAsync(string ventaId)
    {
        _logger.LogDebug("Getting sale by ID: {VentaId}", ventaId);
        
        return await _context.Ventas
            .Include(v => v.Detalles)
            .FirstOrDefaultAsync(v => v.Id == ventaId);
    }

    public async Task<List<Venta>> GetSalesBySellerAsync(string vendedorId, DateTime? fechaInicio = null, DateTime? fechaFin = null)
    {
        _logger.LogDebug("Getting sales for seller: {VendedorId}", vendedorId);
        
        var query = _context.Ventas
            .Include(v => v.Detalles)
            .Where(v => v.IdVendedor == vendedorId);
        
        if (fechaInicio.HasValue)
            query = query.Where(v => v.Fecha >= fechaInicio.Value);
            
        if (fechaFin.HasValue)
            query = query.Where(v => v.Fecha <= fechaFin.Value);
            
        return await query.OrderByDescending(v => v.Fecha).ToListAsync();
    }

    public async Task<List<Venta>> GetSalesByCustomerAsync(string clienteId, DateTime? fechaInicio = null, DateTime? fechaFin = null)
    {
        _logger.LogDebug("Getting sales for customer: {ClienteId}", clienteId);
        
        var query = _context.Ventas
            .Include(v => v.Detalles)
            .Where(v => v.IdCliente == clienteId);
        
        if (fechaInicio.HasValue)
            query = query.Where(v => v.Fecha >= fechaInicio.Value);
            
        if (fechaFin.HasValue)
            query = query.Where(v => v.Fecha <= fechaFin.Value);
            
        return await query.OrderByDescending(v => v.Fecha).ToListAsync();
    }

    public async Task<Venta> CreateSaleAsync(Venta venta)
    {
        _logger.LogDebug("Creating sale: {VentaId}", venta.Id);
        
        venta.FechaCreacion = DateTime.UtcNow;
        venta.Fecha = DateTime.UtcNow;
        
        _context.Ventas.Add(venta);
        await _context.SaveChangesAsync();
        
        return venta;
    }

    public async Task<Venta> UpdateSaleAsync(Venta venta)
    {
        _logger.LogDebug("Updating sale: {VentaId}", venta.Id);
        
        venta.FechaActualizacion = DateTime.UtcNow;
        _context.Ventas.Update(venta);
        await _context.SaveChangesAsync();
        
        return venta;
    }

    public async Task<bool> DeleteSaleAsync(string ventaId)
    {
        _logger.LogDebug("Deleting sale: {VentaId}", ventaId);
        
        var venta = await _context.Ventas.FindAsync(ventaId);
        if (venta != null)
        {
            _context.Ventas.Remove(venta);
            await _context.SaveChangesAsync();
            return true;
        }
        
        return false;
    }

    #endregion

    #region Customer Operations

    public async Task<Cliente?> GetCustomerByIdAsync(string clienteId)
    {
        _logger.LogDebug("Getting customer by ID: {ClienteId}", clienteId);
        
        return await _context.Clientes
            .FirstOrDefaultAsync(c => c.Id == clienteId);
    }

    public async Task<List<Cliente>> GetActiveCustomersAsync()
    {
        _logger.LogDebug("Getting all active customers");
        
        return await _context.Clientes
            .Where(c => c.Activo)
            .OrderBy(c => c.Nombre)
            .ToListAsync();
    }

    public async Task<Cliente> CreateCustomerAsync(Cliente cliente)
    {
        _logger.LogDebug("Creating customer: {ClienteId}", cliente.Id);
        
        cliente.FechaRegistro = DateTime.UtcNow;
        cliente.Activo = true;
        
        _context.Clientes.Add(cliente);
        await _context.SaveChangesAsync();
        
        return cliente;
    }

    public async Task<Cliente> UpdateCustomerAsync(Cliente cliente)
    {
        _logger.LogDebug("Updating customer: {ClienteId}", cliente.Id);
        
        cliente.FechaActualizacion = DateTime.UtcNow;
        _context.Clientes.Update(cliente);
        await _context.SaveChangesAsync();
        
        return cliente;
    }

    public async Task<bool> DeactivateCustomerAsync(string clienteId)
    {
        _logger.LogDebug("Deactivating customer: {ClienteId}", clienteId);
        
        var cliente = await _context.Clientes.FindAsync(clienteId);
        if (cliente != null)
        {
            cliente.Activo = false;
            cliente.FechaActualizacion = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }
        
        return false;
    }

    #endregion
}

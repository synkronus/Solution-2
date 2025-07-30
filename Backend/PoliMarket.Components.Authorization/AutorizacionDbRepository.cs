using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PoliMarket.Components.Infrastructure.Data;
using PoliMarket.Models.Entities;

namespace PoliMarket.Components.Authorization;

/// <summary>
/// Database-backed repository implementation for Authorization Component
/// Uses Entity Framework Core with SQLite database
/// </summary>
public class AutorizacionDbRepository : IAutorizacionRepository
{
    private readonly PoliMarketDbContext _context;
    private readonly ILogger<AutorizacionDbRepository> _logger;

    public AutorizacionDbRepository(PoliMarketDbContext context, ILogger<AutorizacionDbRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    #region Vendedor Operations

    public async Task<Vendedor?> GetVendedorByCodeAsync(string codigoVendedor)
    {
        _logger.LogDebug("Getting seller by code: {CodigoVendedor}", codigoVendedor);
        
        return await _context.Vendedores
            .FirstOrDefaultAsync(v => v.CodigoVendedor == codigoVendedor);
    }

    public async Task<List<Vendedor>> GetAuthorizedVendedoresAsync()
    {
        _logger.LogDebug("Getting all authorized sellers");
        
        return await _context.Vendedores
            .Where(v => v.Autorizado && v.Activo)
            .ToListAsync();
    }

    public async Task<List<Vendedor>> GetPendingSellersAsync()
    {
        _logger.LogDebug("Getting all pending sellers");
        
        return await _context.Vendedores
            .Where(v => !v.Autorizado && v.Activo)
            .ToListAsync();
    }

    public async Task<Vendedor> CreateVendedorAsync(Vendedor vendedor)
    {
        _logger.LogDebug("Creating seller: {CodigoVendedor}", vendedor.CodigoVendedor);
        
        vendedor.FechaCreacion = DateTime.UtcNow;
        _context.Vendedores.Add(vendedor);
        await _context.SaveChangesAsync();
        
        return vendedor;
    }

    public async Task<Vendedor> UpdateVendedorAsync(Vendedor vendedor)
    {
        _logger.LogDebug("Updating seller: {CodigoVendedor}", vendedor.CodigoVendedor);
        
        vendedor.FechaActualizacion = DateTime.UtcNow;
        _context.Vendedores.Update(vendedor);
        await _context.SaveChangesAsync();
        
        return vendedor;
    }

    public async Task<bool> DeleteVendedorAsync(string codigoVendedor)
    {
        _logger.LogDebug("Deleting seller: {CodigoVendedor}", codigoVendedor);
        
        var vendedor = await _context.Vendedores
            .FirstOrDefaultAsync(v => v.CodigoVendedor == codigoVendedor);
            
        if (vendedor != null)
        {
            _context.Vendedores.Remove(vendedor);
            await _context.SaveChangesAsync();
            return true;
        }
        
        return false;
    }

    #endregion

    #region HR Employee Operations

    public async Task<EmpleadoRH?> GetHREmployeeByIdAsync(string empleadoId)
    {
        _logger.LogDebug("Getting HR employee by ID: {EmpleadoId}", empleadoId);
        
        return await _context.EmpleadosRH
            .FirstOrDefaultAsync(e => e.Id == empleadoId);
    }

    public async Task<List<EmpleadoRH>> GetActiveHREmployeesAsync()
    {
        _logger.LogDebug("Getting all active HR employees");
        
        return await _context.EmpleadosRH
            .Where(e => e.Activo)
            .ToListAsync();
    }

    public async Task<List<EmpleadoRH>> GetAllHREmployeesAsync()
    {
        _logger.LogDebug("Getting all HR employees");
        
        return await _context.EmpleadosRH.ToListAsync();
    }

    public async Task<EmpleadoRH> CreateHREmployeeAsync(EmpleadoRH empleado)
    {
        _logger.LogDebug("Creating HR employee: {EmpleadoId}", empleado.Id);
        
        empleado.FechaCreacion = DateTime.UtcNow;
        _context.EmpleadosRH.Add(empleado);
        await _context.SaveChangesAsync();
        
        return empleado;
    }

    public async Task<EmpleadoRH> UpdateHREmployeeAsync(EmpleadoRH empleado)
    {
        _logger.LogDebug("Updating HR employee: {EmpleadoId}", empleado.Id);
        
        _context.EmpleadosRH.Update(empleado);
        await _context.SaveChangesAsync();
        
        return empleado;
    }

    public async Task<bool> DeactivateHREmployeeAsync(string empleadoId)
    {
        _logger.LogDebug("Deactivating HR employee: {EmpleadoId}", empleadoId);
        
        var empleado = await _context.EmpleadosRH
            .FirstOrDefaultAsync(e => e.Id == empleadoId);
            
        if (empleado != null)
        {
            empleado.Activo = false;
            await _context.SaveChangesAsync();
            return true;
        }
        
        return false;
    }

    #endregion
}

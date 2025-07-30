using Microsoft.Extensions.Logging;
using PoliMarket.Models.Entities;

namespace PoliMarket.Components.Authorization;

/// <summary>
/// Repository implementation for Authorization Component
/// Uses in-memory storage for demonstration (will be replaced with EF Core)
/// </summary>
public class AutorizacionRepository : IAutorizacionRepository
{
    private readonly ILogger<AutorizacionRepository> _logger;
    private static readonly List<Vendedor> _vendedores = new()
    {
        new Vendedor
        {
            CodigoVendedor = "V001",
            Nombre = "María González",
            Territorio = "Bogotá",
            Comision = 5.0,
            Autorizado = true,
            FechaAutorizacion = DateTime.UtcNow.AddDays(-30),
            EmpleadoRHAutorizo = "RH001",
            FechaCreacion = DateTime.UtcNow.AddDays(-30),
            Activo = true
        },
        new Vendedor
        {
            CodigoVendedor = "V002",
            Nombre = "Carlos Rodríguez",
            Territorio = "Medellín",
            Comision = 4.5,
            Autorizado = true,
            FechaAutorizacion = DateTime.UtcNow.AddDays(-25),
            EmpleadoRHAutorizo = "RH001",
            FechaCreacion = DateTime.UtcNow.AddDays(-25),
            Activo = true
        },
        new Vendedor
        {
            CodigoVendedor = "V003",
            Nombre = "Ana Martínez",
            Territorio = "Cali",
            Comision = 5.5,
            Autorizado = true,
            FechaAutorizacion = DateTime.UtcNow.AddDays(-20),
            EmpleadoRHAutorizo = "RH002",
            FechaCreacion = DateTime.UtcNow.AddDays(-20),
            Activo = true
        },
        new Vendedor
        {
            CodigoVendedor = "V004",
            Nombre = "Luis Hernández",
            Territorio = "Barranquilla",
            Comision = 4.0,
            Autorizado = true,
            FechaAutorizacion = DateTime.UtcNow.AddDays(-15),
            EmpleadoRHAutorizo = "RH001",
            FechaCreacion = DateTime.UtcNow.AddDays(-15),
            Activo = true
        },
        new Vendedor
        {
            CodigoVendedor = "V005",
            Nombre = "Patricia Silva",
            Territorio = "Cartagena",
            Comision = 6.0,
            Autorizado = true,
            FechaAutorizacion = DateTime.UtcNow.AddDays(-10),
            EmpleadoRHAutorizo = "RH002",
            FechaCreacion = DateTime.UtcNow.AddDays(-10),
            Activo = true
        },
        new Vendedor
        {
            CodigoVendedor = "DEMO",
            Nombre = "Vendedor Demo",
            Territorio = "Nacional",
            Comision = 5.0,
            Autorizado = true,
            FechaAutorizacion = DateTime.UtcNow,
            EmpleadoRHAutorizo = "RH001",
            FechaCreacion = DateTime.UtcNow,
            Activo = true
        }
    };
    private static readonly List<EmpleadoRH> _empleadosRH = new()
    {
        new EmpleadoRH 
        { 
            Id = "RH001", 
            Nombre = "Ana García", 
            Cargo = "Gerente RH", 
            Departamento = "Recursos Humanos",
            Email = "ana.garcia@polimarket.com",
            Telefono = "+57 300 123 4567",
            FechaIngreso = DateTime.UtcNow.AddYears(-3),
            Activo = true
        },
        new EmpleadoRH 
        { 
            Id = "RH002", 
            Nombre = "Carlos López", 
            Cargo = "Analista RH", 
            Departamento = "Recursos Humanos",
            Email = "carlos.lopez@polimarket.com",
            Telefono = "+57 300 765 4321",
            FechaIngreso = DateTime.UtcNow.AddYears(-1),
            Activo = true
        }
    };

    public AutorizacionRepository(ILogger<AutorizacionRepository> logger)
    {
        _logger = logger;
    }

    #region Vendedor Operations

    public async Task<Vendedor?> GetVendedorByCodeAsync(string codigoVendedor)
    {
        _logger.LogDebug("Getting seller by code: {CodigoVendedor}", codigoVendedor);
        
        await Task.Delay(10); // Simulate async operation
        return _vendedores.FirstOrDefault(v => v.CodigoVendedor == codigoVendedor);
    }

    public async Task<List<Vendedor>> GetAuthorizedVendedoresAsync()
    {
        _logger.LogDebug("Getting all authorized sellers");

        await Task.Delay(10); // Simulate async operation
        return _vendedores.Where(v => v.Autorizado && v.Activo).ToList();
    }

    public async Task<List<Vendedor>> GetPendingSellersAsync()
    {
        _logger.LogDebug("Getting all pending sellers");

        await Task.Delay(10); // Simulate async operation
        return _vendedores.Where(v => !v.Autorizado && v.Activo).ToList();
    }

    public async Task<Vendedor> CreateVendedorAsync(Vendedor vendedor)
    {
        _logger.LogDebug("Creating seller: {CodigoVendedor}", vendedor.CodigoVendedor);
        
        await Task.Delay(10); // Simulate async operation
        
        vendedor.FechaCreacion = DateTime.UtcNow;
        _vendedores.Add(vendedor);
        
        return vendedor;
    }

    public async Task<Vendedor> UpdateVendedorAsync(Vendedor vendedor)
    {
        _logger.LogDebug("Updating seller: {CodigoVendedor}", vendedor.CodigoVendedor);
        
        await Task.Delay(10); // Simulate async operation
        
        var existingVendedor = _vendedores.FirstOrDefault(v => v.CodigoVendedor == vendedor.CodigoVendedor);
        if (existingVendedor != null)
        {
            var index = _vendedores.IndexOf(existingVendedor);
            vendedor.FechaActualizacion = DateTime.UtcNow;
            _vendedores[index] = vendedor;
        }
        
        return vendedor;
    }

    public async Task<bool> DeleteVendedorAsync(string codigoVendedor)
    {
        _logger.LogDebug("Deleting seller: {CodigoVendedor}", codigoVendedor);
        
        await Task.Delay(10); // Simulate async operation
        
        var vendedor = _vendedores.FirstOrDefault(v => v.CodigoVendedor == codigoVendedor);
        if (vendedor != null)
        {
            _vendedores.Remove(vendedor);
            return true;
        }
        
        return false;
    }

    #endregion

    #region HR Employee Operations

    public async Task<EmpleadoRH?> GetHREmployeeByIdAsync(string empleadoId)
    {
        _logger.LogDebug("Getting HR employee by ID: {EmpleadoId}", empleadoId);
        
        await Task.Delay(10); // Simulate async operation
        return _empleadosRH.FirstOrDefault(e => e.Id == empleadoId);
    }

    public async Task<List<EmpleadoRH>> GetActiveHREmployeesAsync()
    {
        _logger.LogDebug("Getting all active HR employees");

        await Task.Delay(10); // Simulate async operation
        return _empleadosRH.Where(e => e.Activo).ToList();
    }

    public async Task<List<EmpleadoRH>> GetAllHREmployeesAsync()
    {
        _logger.LogDebug("Getting all HR employees");

        await Task.Delay(10); // Simulate async operation
        return _empleadosRH.ToList();
    }

    public async Task<EmpleadoRH> CreateHREmployeeAsync(EmpleadoRH empleado)
    {
        _logger.LogDebug("Creating HR employee: {EmpleadoId}", empleado.Id);
        
        await Task.Delay(10); // Simulate async operation
        
        empleado.FechaCreacion = DateTime.UtcNow;
        _empleadosRH.Add(empleado);
        
        return empleado;
    }

    public async Task<EmpleadoRH> UpdateHREmployeeAsync(EmpleadoRH empleado)
    {
        _logger.LogDebug("Updating HR employee: {EmpleadoId}", empleado.Id);
        
        await Task.Delay(10); // Simulate async operation
        
        var existingEmpleado = _empleadosRH.FirstOrDefault(e => e.Id == empleado.Id);
        if (existingEmpleado != null)
        {
            var index = _empleadosRH.IndexOf(existingEmpleado);
            _empleadosRH[index] = empleado;
        }
        
        return empleado;
    }

    public async Task<bool> DeactivateHREmployeeAsync(string empleadoId)
    {
        _logger.LogDebug("Deactivating HR employee: {EmpleadoId}", empleadoId);
        
        await Task.Delay(10); // Simulate async operation
        
        var empleado = _empleadosRH.FirstOrDefault(e => e.Id == empleadoId);
        if (empleado != null)
        {
            empleado.Activo = false;
            return true;
        }
        
        return false;
    }

    #endregion
}

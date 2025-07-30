using Microsoft.Extensions.Logging;
using PoliMarket.Models.Entities;

namespace PoliMarket.Components.Sales;

/// <summary>
/// Repository implementation for Sales Component
/// Uses in-memory storage for demonstration (will be replaced with EF Core)
/// </summary>
public class VentasRepository : IVentasRepository
{
    private readonly ILogger<VentasRepository> _logger;
    private static readonly List<Venta> _ventas = new()
    {
        new Venta
        {
            Id = "VTA001",
            Fecha = DateTime.UtcNow.AddDays(-5),
            IdCliente = "CLI001",
            IdVendedor = "V001",
            Total = 2850000,
            Estado = "Procesada",
            Observaciones = "Venta corporativa - Equipos de oficina",
            FechaCreacion = DateTime.UtcNow.AddDays(-5),
            Detalles = new List<DetalleVenta>
            {
                new DetalleVenta { IdProducto = "P001", Cantidad = 1, Precio = 2500000, Descuento = 0 },
                new DetalleVenta { IdProducto = "P002", Cantidad = 1, Precio = 350000, Descuento = 0 }
            }
        },
        new Venta
        {
            Id = "VTA002",
            Fecha = DateTime.UtcNow.AddDays(-4),
            IdCliente = "CLI002",
            IdVendedor = "V001",
            Total = 1250000,
            Estado = "Procesada",
            Observaciones = "Cliente frecuente",
            FechaCreacion = DateTime.UtcNow.AddDays(-4),
            Detalles = new List<DetalleVenta>
            {
                new DetalleVenta { IdProducto = "P003", Cantidad = 1, Precio = 800000, Descuento = 0 },
                new DetalleVenta { IdProducto = "P004", Cantidad = 1, Precio = 450000, Descuento = 0 }
            }
        },
        new Venta
        {
            Id = "VTA003",
            Fecha = DateTime.UtcNow.AddDays(-3),
            IdCliente = "CLI003",
            IdVendedor = "V002",
            Total = 5200000,
            Estado = "Procesada",
            Observaciones = "Pedido especial corporativo",
            FechaCreacion = DateTime.UtcNow.AddDays(-3),
            Detalles = new List<DetalleVenta>
            {
                new DetalleVenta { IdProducto = "P001", Cantidad = 2, Precio = 2500000, Descuento = 100000 },
                new DetalleVenta { IdProducto = "P002", Cantidad = 1, Precio = 350000, Descuento = 50000 }
            }
        },
        new Venta
        {
            Id = "VTA004",
            Fecha = DateTime.UtcNow.AddDays(-2),
            IdCliente = "CLI004",
            IdVendedor = "V003",
            Total = 1600000,
            Estado = "Pendiente",
            Observaciones = "Esperando confirmación de entrega",
            FechaCreacion = DateTime.UtcNow.AddDays(-2),
            Detalles = new List<DetalleVenta>
            {
                new DetalleVenta { IdProducto = "P003", Cantidad = 2, Precio = 800000, Descuento = 0 }
            }
        },
        new Venta
        {
            Id = "VTA005",
            Fecha = DateTime.UtcNow.AddDays(-1),
            IdCliente = "CLI005",
            IdVendedor = "DEMO",
            Total = 3750000,
            Estado = "Procesada",
            Observaciones = "Venta demo - Múltiples productos",
            FechaCreacion = DateTime.UtcNow.AddDays(-1),
            Detalles = new List<DetalleVenta>
            {
                new DetalleVenta { IdProducto = "P001", Cantidad = 1, Precio = 2500000, Descuento = 0 },
                new DetalleVenta { IdProducto = "P002", Cantidad = 2, Precio = 350000, Descuento = 0 },
                new DetalleVenta { IdProducto = "P004", Cantidad = 1, Precio = 450000, Descuento = 0 }
            }
        }
    };
    private static readonly List<Cliente> _clientes = new()
    {
        new Cliente
        {
            Id = "CLI001",
            Nombre = "Empresa ABC S.A.S",
            Direccion = "Calle 123 #45-67, Bogotá",
            Telefono = "+57 1 234 5678",
            Email = "contacto@empresaabc.com",
            TipoCliente = "Corporativo",
            LimiteCredito = 10000000,
            Activo = true,
            FechaRegistro = DateTime.UtcNow.AddDays(-60)
        },
        new Cliente
        {
            Id = "CLI002",
            Nombre = "Juan Pérez",
            Direccion = "Carrera 45 #12-34, Medellín",
            Telefono = "+57 300 123 4567",
            Email = "juan.perez@email.com",
            TipoCliente = "Regular",
            LimiteCredito = 1000000,
            Activo = true,
            FechaRegistro = DateTime.UtcNow.AddDays(-45)
        },
        new Cliente
        {
            Id = "CLI003",
            Nombre = "Corporación XYZ Ltda",
            Direccion = "Avenida 68 #25-30, Bogotá",
            Telefono = "+57 1 345 6789",
            Email = "ventas@corpxyz.com",
            TipoCliente = "Corporativo",
            LimiteCredito = 15000000,
            Activo = true,
            FechaRegistro = DateTime.UtcNow.AddDays(-40)
        },
        new Cliente
        {
            Id = "CLI004",
            Nombre = "María Rodríguez",
            Direccion = "Calle 50 #15-20, Cali",
            Telefono = "+57 300 987 6543",
            Email = "maria.rodriguez@email.com",
            TipoCliente = "VIP",
            LimiteCredito = 5000000,
            Activo = true,
            FechaRegistro = DateTime.UtcNow.AddDays(-35)
        },
        new Cliente
        {
            Id = "CLI005",
            Nombre = "Distribuidora Valle S.A.S",
            Direccion = "Carrera 100 #80-45, Cali",
            Telefono = "+57 2 456 7890",
            Email = "info@distvalle.com",
            TipoCliente = "Corporativo",
            LimiteCredito = 20000000,
            Activo = true,
            FechaRegistro = DateTime.UtcNow.AddDays(-30)
        },
        new Cliente
        {
            Id = "CLI006",
            Nombre = "Carlos Mendoza",
            Direccion = "Calle 72 #11-25, Barranquilla",
            Telefono = "+57 300 555 1234",
            Email = "carlos.mendoza@email.com",
            TipoCliente = "Regular",
            LimiteCredito = 2000000,
            Activo = true,
            FechaRegistro = DateTime.UtcNow.AddDays(-25)
        },
        new Cliente
        {
            Id = "CLI007",
            Nombre = "TechSolutions Ltda",
            Direccion = "Zona Rosa, Carrera 15 #93-47, Bogotá",
            Telefono = "+57 1 789 0123",
            Email = "contacto@techsolutions.com",
            TipoCliente = "Corporativo",
            LimiteCredito = 12000000,
            Activo = true,
            FechaRegistro = DateTime.UtcNow.AddDays(-20)
        },
        new Cliente
        {
            Id = "CLI008",
            Nombre = "Laura Gómez",
            Direccion = "Poblado, Carrera 43A #5-15, Medellín",
            Telefono = "+57 300 111 2222",
            Email = "laura.gomez@email.com",
            TipoCliente = "VIP",
            LimiteCredito = 3000000,
            Activo = true,
            FechaRegistro = DateTime.UtcNow.AddDays(-15)
        }
    };

    public VentasRepository(ILogger<VentasRepository> logger)
    {
        _logger = logger;
    }

    #region Sale Operations

    public async Task<Venta?> GetSaleByIdAsync(string ventaId)
    {
        _logger.LogDebug("Getting sale by ID: {VentaId}", ventaId);
        
        await Task.Delay(10); // Simulate async operation
        return _ventas.FirstOrDefault(v => v.Id == ventaId);
    }

    public async Task<List<Venta>> GetSalesBySellerAsync(string vendedorId, DateTime? fechaInicio = null, DateTime? fechaFin = null)
    {
        _logger.LogDebug("Getting sales for seller: {VendedorId}", vendedorId);
        
        await Task.Delay(10); // Simulate async operation
        
        var query = _ventas.Where(v => v.IdVendedor == vendedorId);
        
        if (fechaInicio.HasValue)
            query = query.Where(v => v.Fecha >= fechaInicio.Value);
            
        if (fechaFin.HasValue)
            query = query.Where(v => v.Fecha <= fechaFin.Value);
            
        return query.ToList();
    }

    public async Task<List<Venta>> GetSalesByCustomerAsync(string clienteId, DateTime? fechaInicio = null, DateTime? fechaFin = null)
    {
        _logger.LogDebug("Getting sales for customer: {ClienteId}", clienteId);
        
        await Task.Delay(10); // Simulate async operation
        
        var query = _ventas.Where(v => v.IdCliente == clienteId);
        
        if (fechaInicio.HasValue)
            query = query.Where(v => v.Fecha >= fechaInicio.Value);
            
        if (fechaFin.HasValue)
            query = query.Where(v => v.Fecha <= fechaFin.Value);
            
        return query.ToList();
    }

    public async Task<Venta> CreateSaleAsync(Venta venta)
    {
        _logger.LogDebug("Creating sale: {VentaId}", venta.Id);
        
        await Task.Delay(10); // Simulate async operation
        
        venta.FechaCreacion = DateTime.UtcNow;
        _ventas.Add(venta);
        
        return venta;
    }

    public async Task<Venta> UpdateSaleAsync(Venta venta)
    {
        _logger.LogDebug("Updating sale: {VentaId}", venta.Id);
        
        await Task.Delay(10); // Simulate async operation
        
        var existingVenta = _ventas.FirstOrDefault(v => v.Id == venta.Id);
        if (existingVenta != null)
        {
            var index = _ventas.IndexOf(existingVenta);
            venta.FechaActualizacion = DateTime.UtcNow;
            _ventas[index] = venta;
        }
        
        return venta;
    }

    public async Task<bool> DeleteSaleAsync(string ventaId)
    {
        _logger.LogDebug("Deleting sale: {VentaId}", ventaId);
        
        await Task.Delay(10); // Simulate async operation
        
        var venta = _ventas.FirstOrDefault(v => v.Id == ventaId);
        if (venta != null)
        {
            _ventas.Remove(venta);
            return true;
        }
        
        return false;
    }

    #endregion

    #region Customer Operations

    public async Task<Cliente?> GetCustomerByIdAsync(string clienteId)
    {
        _logger.LogDebug("Getting customer by ID: {ClienteId}", clienteId);
        
        await Task.Delay(10); // Simulate async operation
        return _clientes.FirstOrDefault(c => c.Id == clienteId);
    }

    public async Task<List<Cliente>> GetActiveCustomersAsync()
    {
        _logger.LogDebug("Getting all active customers");
        
        await Task.Delay(10); // Simulate async operation
        return _clientes.Where(c => c.Activo).ToList();
    }

    public async Task<Cliente> CreateCustomerAsync(Cliente cliente)
    {
        _logger.LogDebug("Creating customer: {ClienteId}", cliente.Id);
        
        await Task.Delay(10); // Simulate async operation
        
        cliente.FechaRegistro = DateTime.UtcNow;
        _clientes.Add(cliente);
        
        return cliente;
    }

    public async Task<Cliente> UpdateCustomerAsync(Cliente cliente)
    {
        _logger.LogDebug("Updating customer: {ClienteId}", cliente.Id);
        
        await Task.Delay(10); // Simulate async operation
        
        var existingCliente = _clientes.FirstOrDefault(c => c.Id == cliente.Id);
        if (existingCliente != null)
        {
            var index = _clientes.IndexOf(existingCliente);
            cliente.FechaActualizacion = DateTime.UtcNow;
            _clientes[index] = cliente;
        }
        
        return cliente;
    }

    public async Task<bool> DeactivateCustomerAsync(string clienteId)
    {
        _logger.LogDebug("Deactivating customer: {ClienteId}", clienteId);
        
        await Task.Delay(10); // Simulate async operation
        
        var cliente = _clientes.FirstOrDefault(c => c.Id == clienteId);
        if (cliente != null)
        {
            cliente.Activo = false;
            cliente.FechaActualizacion = DateTime.UtcNow;
            return true;
        }
        
        return false;
    }

    #endregion
}

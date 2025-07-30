using PoliMarket.Models.Entities;

namespace PoliMarket.Components.Sales;

/// <summary>
/// Repository interface for Sales Component data access
/// </summary>
public interface IVentasRepository
{
    // Sale operations
    Task<Venta?> GetSaleByIdAsync(string ventaId);
    Task<List<Venta>> GetSalesBySellerAsync(string vendedorId, DateTime? fechaInicio = null, DateTime? fechaFin = null);
    Task<List<Venta>> GetSalesByCustomerAsync(string clienteId, DateTime? fechaInicio = null, DateTime? fechaFin = null);
    Task<Venta> CreateSaleAsync(Venta venta);
    Task<Venta> UpdateSaleAsync(Venta venta);
    Task<bool> DeleteSaleAsync(string ventaId);

    // Customer operations
    Task<Cliente?> GetCustomerByIdAsync(string clienteId);
    Task<List<Cliente>> GetActiveCustomersAsync();
    Task<Cliente> CreateCustomerAsync(Cliente cliente);
    Task<Cliente> UpdateCustomerAsync(Cliente cliente);
    Task<bool> DeactivateCustomerAsync(string clienteId);
}

using PoliMarket.Models.Entities;
using PoliMarket.Models.Common;
using PoliMarket.Contracts;

namespace PoliMarket.Components.Customers;

/// <summary>
/// Interface for Customer Management Component
/// Provides customer management and relationship services
/// </summary>
public interface ICustomersComponent
{
    /// <summary>
    /// Creates a new customer
    /// </summary>
    /// <param name="cliente">Customer information</param>
    /// <returns>API response with created customer</returns>
    Task<ApiResponse<Cliente>> CreateCustomerAsync(Cliente cliente);

    /// <summary>
    /// Gets customer by ID
    /// </summary>
    /// <param name="clienteId">Customer ID</param>
    /// <returns>API response with customer information</returns>
    Task<ApiResponse<Cliente>> GetCustomerByIdAsync(string clienteId);

    /// <summary>
    /// Updates an existing customer
    /// </summary>
    /// <param name="clienteId">Customer ID</param>
    /// <param name="cliente">Updated customer information</param>
    /// <returns>API response with updated customer</returns>
    Task<ApiResponse<Cliente>> UpdateCustomerAsync(string clienteId, Cliente cliente);

    /// <summary>
    /// Deletes a customer (soft delete)
    /// </summary>
    /// <param name="clienteId">Customer ID</param>
    /// <returns>API response with deletion result</returns>
    Task<ApiResponse<bool>> DeleteCustomerAsync(string clienteId);

    /// <summary>
    /// Gets all customers with pagination and filtering
    /// </summary>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <param name="tipoCliente">Customer type filter</param>
    /// <param name="activo">Active status filter</param>
    /// <param name="searchTerm">Search term for name/email</param>
    /// <returns>API response with list of customers</returns>
    Task<ApiResponse<CustomerListResponse>> GetCustomersAsync(int page = 1, int pageSize = 10, string? tipoCliente = null, bool? activo = null, string? searchTerm = null);

    /// <summary>
    /// Gets customer types
    /// </summary>
    /// <returns>API response with list of customer types</returns>
    Task<ApiResponse<List<string>>> GetCustomerTypesAsync();

    /// <summary>
    /// Updates customer credit limit
    /// </summary>
    /// <param name="clienteId">Customer ID</param>
    /// <param name="nuevoLimite">New credit limit</param>
    /// <param name="usuarioResponsable">User responsible for the change</param>
    /// <returns>API response with credit limit update result</returns>
    Task<ApiResponse<Cliente>> UpdateCreditLimitAsync(string clienteId, double nuevoLimite, string usuarioResponsable);

    /// <summary>
    /// Gets customer sales history
    /// </summary>
    /// <param name="clienteId">Customer ID</param>
    /// <param name="fechaInicio">Start date filter</param>
    /// <param name="fechaFin">End date filter</param>
    /// <returns>API response with customer sales history</returns>
    Task<ApiResponse<List<Venta>>> GetCustomerSalesHistoryAsync(string clienteId, DateTime? fechaInicio = null, DateTime? fechaFin = null);
}

/// <summary>
/// Response model for customer list with pagination
/// </summary>
public class CustomerListResponse
{
    public List<Cliente> Customers { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

/// <summary>
/// Request model for creating/updating customers
/// </summary>
public class CreateCustomerRequest
{
    public string Nombre { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string TipoCliente { get; set; } = "Regular";
    public double LimiteCredito { get; set; } = 0;
}

/// <summary>
/// Request model for updating credit limit
/// </summary>
public class UpdateCreditLimitRequest
{
    public double NuevoLimite { get; set; }
    public string UsuarioResponsable { get; set; } = string.Empty;
    public string? Motivo { get; set; }
}

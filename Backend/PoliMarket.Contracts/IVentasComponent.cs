using PoliMarket.Models.Common;
using PoliMarket.Models.Entities;

namespace PoliMarket.Contracts;

/// <summary>
/// Interface for Sales Component (RF2)
/// Provides sales processing and transaction management services
/// Reusability Level: High (85%) - Standard commercial processes
/// </summary>
public interface IVentasComponent
{
    /// <summary>
    /// Processes a new sale transaction
    /// </summary>
    /// <param name="request">Sale creation request</param>
    /// <returns>API response with sale processing result</returns>
    Task<ApiResponse<SaleProcessingResponse>> ProcessSaleAsync(CreateSaleRequest request);

    /// <summary>
    /// Calculates total for a sale including taxes and discounts
    /// </summary>
    /// <param name="detalles">List of sale details</param>
    /// <returns>API response with calculated total</returns>
    Task<ApiResponse<double>> CalculateSaleTotalAsync(List<SaleDetailRequest> detalles);

    /// <summary>
    /// Applies discount to an existing sale
    /// </summary>
    /// <param name="ventaId">Sale ID</param>
    /// <param name="descuento">Discount amount</param>
    /// <param name="motivo">Discount reason</param>
    /// <returns>API response with updated sale</returns>
    Task<ApiResponse<Venta>> ApplyDiscountAsync(string ventaId, double descuento, string motivo);

    /// <summary>
    /// Gets sale by ID
    /// </summary>
    /// <param name="ventaId">Sale ID</param>
    /// <returns>API response with sale information</returns>
    Task<ApiResponse<Venta>> GetSaleByIdAsync(string ventaId);

    /// <summary>
    /// Gets sales by seller
    /// </summary>
    /// <param name="vendedorId">Seller ID</param>
    /// <param name="fechaInicio">Start date filter</param>
    /// <param name="fechaFin">End date filter</param>
    /// <returns>API response with list of sales</returns>
    Task<ApiResponse<List<Venta>>> GetSalesBySellerAsync(string vendedorId, DateTime? fechaInicio = null, DateTime? fechaFin = null);

    /// <summary>
    /// Updates sale status
    /// </summary>
    /// <param name="ventaId">Sale ID</param>
    /// <param name="nuevoEstado">New status</param>
    /// <returns>API response with update result</returns>
    Task<ApiResponse<Venta>> UpdateSaleStatusAsync(string ventaId, string nuevoEstado);

    /// <summary>
    /// Cancels a sale
    /// </summary>
    /// <param name="ventaId">Sale ID</param>
    /// <param name="motivo">Cancellation reason</param>
    /// <returns>API response with cancellation result</returns>
    Task<ApiResponse<bool>> CancelSaleAsync(string ventaId, string motivo);
}

/// <summary>
/// Interface for Customer Management Component
/// Provides customer relationship management services
/// Reusability Level: High (80%) - Standard CRM functionality
/// </summary>
public interface IClientesComponent
{
    /// <summary>
    /// Registers a new customer
    /// </summary>
    /// <param name="cliente">Customer information</param>
    /// <returns>API response with created customer</returns>
    Task<ApiResponse<Cliente>> RegisterCustomerAsync(Cliente cliente);

    /// <summary>
    /// Gets customer by ID
    /// </summary>
    /// <param name="clienteId">Customer ID</param>
    /// <returns>API response with customer information</returns>
    Task<ApiResponse<Cliente>> GetCustomerByIdAsync(string clienteId);

    /// <summary>
    /// Gets all active customers
    /// </summary>
    /// <returns>API response with list of customers</returns>
    Task<ApiResponse<List<Cliente>>> GetActiveCustomersAsync();

    /// <summary>
    /// Updates customer information
    /// </summary>
    /// <param name="cliente">Updated customer information</param>
    /// <returns>API response with update result</returns>
    Task<ApiResponse<Cliente>> UpdateCustomerAsync(Cliente cliente);

    /// <summary>
    /// Gets customer purchase history
    /// </summary>
    /// <param name="clienteId">Customer ID</param>
    /// <param name="fechaInicio">Start date filter</param>
    /// <param name="fechaFin">End date filter</param>
    /// <returns>API response with purchase history</returns>
    Task<ApiResponse<List<Venta>>> GetCustomerHistoryAsync(string clienteId, DateTime? fechaInicio = null, DateTime? fechaFin = null);

    /// <summary>
    /// Updates customer credit limit
    /// </summary>
    /// <param name="clienteId">Customer ID</param>
    /// <param name="nuevoLimite">New credit limit</param>
    /// <returns>API response with update result</returns>
    Task<ApiResponse<Cliente>> UpdateCreditLimitAsync(string clienteId, double nuevoLimite);

    /// <summary>
    /// Deactivates a customer
    /// </summary>
    /// <param name="clienteId">Customer ID</param>
    /// <returns>API response with deactivation result</returns>
    Task<ApiResponse<bool>> DeactivateCustomerAsync(string clienteId);
}

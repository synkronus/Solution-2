using PoliMarket.Models.Common;
using PoliMarket.Models.Entities;

namespace PoliMarket.Components.Delivery;

/// <summary>
/// Interface for Delivery Component (RF4)
/// Handles delivery scheduling, tracking, and management operations
/// </summary>
public interface IEntregasComponent
{
    /// <summary>
    /// Gets all deliveries for a specific vendor
    /// </summary>
    /// <param name="vendorId">Vendor ID to filter deliveries</param>
    /// <returns>API response with list of deliveries</returns>
    Task<ApiResponse<List<Entrega>>> GetDeliveriesByVendorAsync(string vendorId);

    /// <summary>
    /// Gets all deliveries
    /// </summary>
    /// <returns>API response with list of all deliveries</returns>
    Task<ApiResponse<List<Entrega>>> GetAllDeliveriesAsync();

    /// <summary>
    /// Gets a specific delivery by ID
    /// </summary>
    /// <param name="deliveryId">Delivery ID</param>
    /// <returns>API response with delivery details</returns>
    Task<ApiResponse<Entrega>> GetDeliveryByIdAsync(string deliveryId);

    /// <summary>
    /// Schedules a new delivery
    /// </summary>
    /// <param name="request">Schedule delivery request</param>
    /// <returns>API response with scheduled delivery</returns>
    Task<ApiResponse<Entrega>> ScheduleDeliveryAsync(ScheduleDeliveryRequest request);

    /// <summary>
    /// Updates delivery status
    /// </summary>
    /// <param name="request">Update delivery status request</param>
    /// <returns>API response with updated delivery</returns>
    Task<ApiResponse<Entrega>> UpdateDeliveryStatusAsync(UpdateDeliveryStatusRequest request);

    /// <summary>
    /// Gets pending deliveries
    /// </summary>
    /// <returns>API response with list of pending deliveries</returns>
    Task<ApiResponse<List<Entrega>>> GetPendingDeliveriesAsync();

    /// <summary>
    /// Confirms a delivery
    /// </summary>
    /// <param name="deliveryId">Delivery ID to confirm</param>
    /// <returns>API response with confirmation result</returns>
    Task<ApiResponse<bool>> ConfirmDeliveryAsync(string deliveryId);

    /// <summary>
    /// Gets delivery tracking information
    /// </summary>
    /// <param name="deliveryId">Delivery ID</param>
    /// <returns>API response with tracking information</returns>
    Task<ApiResponse<List<SeguimientoEntrega>>> GetDeliveryTrackingAsync(string deliveryId);

    /// <summary>
    /// Creates deliveries automatically for completed sales
    /// </summary>
    /// <param name="saleId">Sale ID to create delivery for</param>
    /// <returns>API response with created delivery</returns>
    Task<ApiResponse<Entrega>> CreateDeliveryFromSaleAsync(string saleId);
}

using PoliMarket.Models.Common;
using PoliMarket.Models.Entities;

namespace PoliMarket.Contracts;

/// <summary>
/// Interface for Inventory Component (RF3)
/// Provides inventory management and stock control services
/// Reusability Level: Very High (90%) - Universal inventory system
/// </summary>
public interface IInventarioComponent
{
    /// <summary>
    /// Checks product availability for a given quantity
    /// </summary>
    /// <param name="request">Availability check request</param>
    /// <returns>API response with availability information</returns>
    Task<ApiResponse<DisponibilidadInventario>> CheckAvailabilityAsync(AvailabilityCheckRequest request);

    /// <summary>
    /// Updates product stock
    /// </summary>
    /// <param name="request">Stock update request</param>
    /// <returns>API response with stock operation result</returns>
    Task<ApiResponse<StockOperationResponse>> UpdateStockAsync(StockUpdateRequest request);

    /// <summary>
    /// Gets current stock for a product
    /// </summary>
    /// <param name="productoId">Product ID</param>
    /// <returns>API response with current stock information</returns>
    Task<ApiResponse<DisponibilidadInventario>> GetCurrentStockAsync(string productoId);

    /// <summary>
    /// Gets stock movement history for a product
    /// </summary>
    /// <param name="productoId">Product ID</param>
    /// <param name="fechaInicio">Start date filter</param>
    /// <param name="fechaFin">End date filter</param>
    /// <returns>API response with movement history</returns>
    Task<ApiResponse<List<MovimientoInventario>>> GetStockMovementsAsync(string productoId, DateTime? fechaInicio = null, DateTime? fechaFin = null);

    /// <summary>
    /// Generates stock alerts based on minimum/maximum levels
    /// </summary>
    /// <returns>API response with list of stock alerts</returns>
    Task<ApiResponse<List<AlertaStock>>> GenerateStockAlertsAsync();

    /// <summary>
    /// Reserves stock for a sale
    /// </summary>
    /// <param name="productoId">Product ID</param>
    /// <param name="cantidad">Quantity to reserve</param>
    /// <param name="ventaId">Sale ID for reference</param>
    /// <returns>API response with reservation result</returns>
    Task<ApiResponse<bool>> ReserveStockAsync(string productoId, int cantidad, string ventaId);

    /// <summary>
    /// Releases reserved stock
    /// </summary>
    /// <param name="productoId">Product ID</param>
    /// <param name="cantidad">Quantity to release</param>
    /// <param name="ventaId">Sale ID for reference</param>
    /// <returns>API response with release result</returns>
    Task<ApiResponse<bool>> ReleaseStockAsync(string productoId, int cantidad, string ventaId);

    /// <summary>
    /// Processes stock adjustment
    /// </summary>
    /// <param name="productoId">Product ID</param>
    /// <param name="nuevoStock">New stock quantity</param>
    /// <param name="motivo">Adjustment reason</param>
    /// <param name="usuarioResponsable">User responsible for adjustment</param>
    /// <returns>API response with adjustment result</returns>
    Task<ApiResponse<StockOperationResponse>> AdjustStockAsync(string productoId, int nuevoStock, string motivo, string usuarioResponsable);
}

/// <summary>
/// Interface for Product Management Component
/// Provides product catalog and pricing management services
/// Reusability Level: Medium (70%) - Adaptable product catalog
/// </summary>
public interface IProductosComponent
{
    /// <summary>
    /// Creates a new product
    /// </summary>
    /// <param name="producto">Product information</param>
    /// <returns>API response with created product</returns>
    Task<ApiResponse<Producto>> CreateProductAsync(Producto producto);

    /// <summary>
    /// Gets product by ID
    /// </summary>
    /// <param name="productoId">Product ID</param>
    /// <returns>API response with product information</returns>
    Task<ApiResponse<Producto>> GetProductByIdAsync(string productoId);

    /// <summary>
    /// Gets all active products
    /// </summary>
    /// <returns>API response with list of products</returns>
    Task<ApiResponse<List<Producto>>> GetActiveProductsAsync();

    /// <summary>
    /// Gets products by category
    /// </summary>
    /// <param name="categoria">Category name</param>
    /// <returns>API response with list of products in category</returns>
    Task<ApiResponse<List<Producto>>> GetProductsByCategoryAsync(string categoria);

    /// <summary>
    /// Updates product information
    /// </summary>
    /// <param name="producto">Updated product information</param>
    /// <returns>API response with update result</returns>
    Task<ApiResponse<Producto>> UpdateProductAsync(Producto producto);

    /// <summary>
    /// Updates product price
    /// </summary>
    /// <param name="productoId">Product ID</param>
    /// <param name="nuevoPrecio">New price</param>
    /// <returns>API response with update result</returns>
    Task<ApiResponse<Producto>> UpdateProductPriceAsync(string productoId, double nuevoPrecio);

    /// <summary>
    /// Deactivates a product
    /// </summary>
    /// <param name="productoId">Product ID</param>
    /// <returns>API response with deactivation result</returns>
    Task<ApiResponse<bool>> DeactivateProductAsync(string productoId);

    /// <summary>
    /// Searches products by name or description
    /// </summary>
    /// <param name="searchTerm">Search term</param>
    /// <returns>API response with matching products</returns>
    Task<ApiResponse<List<Producto>>> SearchProductsAsync(string searchTerm);

    /// <summary>
    /// Gets all product categories
    /// </summary>
    /// <returns>API response with list of categories</returns>
    Task<ApiResponse<List<string>>> GetCategoriesAsync();
}

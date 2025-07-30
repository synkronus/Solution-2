using PoliMarket.Models.Entities;
using PoliMarket.Models.Common;
using PoliMarket.Contracts;

namespace PoliMarket.Components.Products;

/// <summary>
/// Interface for Product Management Component
/// Provides product catalog and pricing management services
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
    /// Updates an existing product
    /// </summary>
    /// <param name="productoId">Product ID</param>
    /// <param name="producto">Updated product information</param>
    /// <returns>API response with updated product</returns>
    Task<ApiResponse<Producto>> UpdateProductAsync(string productoId, Producto producto);

    /// <summary>
    /// Deletes a product (soft delete)
    /// </summary>
    /// <param name="productoId">Product ID</param>
    /// <returns>API response with deletion result</returns>
    Task<ApiResponse<bool>> DeleteProductAsync(string productoId);

    /// <summary>
    /// Gets all products with pagination and filtering
    /// </summary>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <param name="categoria">Category filter</param>
    /// <param name="activo">Active status filter</param>
    /// <param name="searchTerm">Search term for name/description</param>
    /// <returns>API response with list of products</returns>
    Task<ApiResponse<ProductListResponse>> GetProductsAsync(int page = 1, int pageSize = 10, string? categoria = null, bool? activo = null, string? searchTerm = null);

    /// <summary>
    /// Gets all product categories
    /// </summary>
    /// <returns>API response with list of categories</returns>
    Task<ApiResponse<List<string>>> GetCategoriesAsync();

    /// <summary>
    /// Updates product price
    /// </summary>
    /// <param name="productoId">Product ID</param>
    /// <param name="nuevoPrecio">New price</param>
    /// <param name="usuarioResponsable">User responsible for the change</param>
    /// <returns>API response with price update result</returns>
    Task<ApiResponse<Producto>> UpdateProductPriceAsync(string productoId, double nuevoPrecio, string usuarioResponsable);

    /// <summary>
    /// Gets products with low stock
    /// </summary>
    /// <returns>API response with list of low stock products</returns>
    Task<ApiResponse<List<Producto>>> GetLowStockProductsAsync();
}

/// <summary>
/// Response model for product list with pagination
/// </summary>
public class ProductListResponse
{
    public List<Producto> Products { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

/// <summary>
/// Request model for creating/updating products
/// </summary>
public class CreateProductRequest
{
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public double Precio { get; set; }
    public string Categoria { get; set; } = string.Empty;
    public int Stock { get; set; }
    public int StockMinimo { get; set; } = 10;
    public int StockMaximo { get; set; } = 1000;
    public string UnidadMedida { get; set; } = "Unidad";
}

/// <summary>
/// Request model for updating product price
/// </summary>
public class UpdatePriceRequest
{
    public double NuevoPrecio { get; set; }
    public string UsuarioResponsable { get; set; } = string.Empty;
    public string? Motivo { get; set; }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PoliMarket.Components.Infrastructure.Data;
using PoliMarket.Models.Entities;
using PoliMarket.Models.Common;
using PoliMarket.Contracts;

namespace PoliMarket.Components.Products;

/// <summary>
/// Implementation of Product Management Component
/// Provides product catalog and pricing management services
/// </summary>
public class ProductosComponent : IProductosComponent
{
    private readonly PoliMarketDbContext _context;
    private readonly ILogger<ProductosComponent> _logger;

    public ProductosComponent(PoliMarketDbContext context, ILogger<ProductosComponent> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ApiResponse<Producto>> CreateProductAsync(Producto producto)
    {
        try
        {
            _logger.LogInformation("Creating product: {ProductName}", producto.Nombre);

            // Generate ID if not provided
            if (string.IsNullOrEmpty(producto.Id))
            {
                producto.Id = GenerateProductId();
            }

            // Check if product ID already exists
            var existingProduct = await _context.Productos.FindAsync(producto.Id);
            if (existingProduct != null)
            {
                return ApiResponse<Producto>.ErrorResult("El ID del producto ya existe");
            }

            producto.FechaCreacion = DateTime.UtcNow;
            producto.Estado = true;

            _context.Productos.Add(producto);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Product created successfully: {ProductId}", producto.Id);
            return ApiResponse<Producto>.SuccessResult(producto, "Producto creado exitosamente");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product: {ProductName}", producto.Nombre);
            return ApiResponse<Producto>.ErrorResult("Error interno del servidor", ex.Message);
        }
    }

    public async Task<ApiResponse<Producto>> GetProductByIdAsync(string productoId)
    {
        try
        {
            _logger.LogInformation("Getting product: {ProductId}", productoId);

            var producto = await _context.Productos
                .Include(p => p.Movimientos)
                .FirstOrDefaultAsync(p => p.Id == productoId);

            if (producto == null)
            {
                return ApiResponse<Producto>.ErrorResult("Producto no encontrado");
            }

            return ApiResponse<Producto>.SuccessResult(producto, "Producto encontrado");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting product: {ProductId}", productoId);
            return ApiResponse<Producto>.ErrorResult("Error interno del servidor", ex.Message);
        }
    }

    public async Task<ApiResponse<Producto>> UpdateProductAsync(string productoId, Producto producto)
    {
        try
        {
            _logger.LogInformation("Updating product: {ProductId}", productoId);

            var existingProduct = await _context.Productos.FindAsync(productoId);
            if (existingProduct == null)
            {
                return ApiResponse<Producto>.ErrorResult("Producto no encontrado");
            }

            // Update properties
            existingProduct.Nombre = producto.Nombre;
            existingProduct.Descripcion = producto.Descripcion;
            existingProduct.Precio = producto.Precio;
            existingProduct.Categoria = producto.Categoria;
            existingProduct.Stock = producto.Stock;
            existingProduct.StockMinimo = producto.StockMinimo;
            existingProduct.StockMaximo = producto.StockMaximo;
            existingProduct.UnidadMedida = producto.UnidadMedida;
            existingProduct.Estado = producto.Estado;
            existingProduct.FechaActualizacion = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Product updated successfully: {ProductId}", productoId);
            return ApiResponse<Producto>.SuccessResult(existingProduct, "Producto actualizado exitosamente");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product: {ProductId}", productoId);
            return ApiResponse<Producto>.ErrorResult("Error interno del servidor", ex.Message);
        }
    }

    public async Task<ApiResponse<bool>> DeleteProductAsync(string productoId)
    {
        try
        {
            _logger.LogInformation("Deleting product: {ProductId}", productoId);

            var producto = await _context.Productos.FindAsync(productoId);
            if (producto == null)
            {
                return ApiResponse<bool>.ErrorResult("Producto no encontrado");
            }

            // Soft delete
            producto.Estado = false;
            producto.FechaActualizacion = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Product deleted successfully: {ProductId}", productoId);
            return ApiResponse<bool>.SuccessResult(true, "Producto eliminado exitosamente");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product: {ProductId}", productoId);
            return ApiResponse<bool>.ErrorResult("Error interno del servidor", ex.Message);
        }
    }

    public async Task<ApiResponse<ProductListResponse>> GetProductsAsync(int page = 1, int pageSize = 10, string? categoria = null, bool? activo = null, string? searchTerm = null)
    {
        try
        {
            _logger.LogInformation("Getting products - Page: {Page}, PageSize: {PageSize}", page, pageSize);

            var query = _context.Productos.AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(categoria))
            {
                query = query.Where(p => p.Categoria == categoria);
            }

            if (activo.HasValue)
            {
                query = query.Where(p => p.Estado == activo.Value);
            }

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(p => p.Nombre.Contains(searchTerm) || p.Descripcion.Contains(searchTerm));
            }

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            var productos = await query
                .OrderBy(p => p.Nombre)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var response = new ProductListResponse
            {
                Products = productos,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = totalPages
            };

            return ApiResponse<ProductListResponse>.SuccessResult(response, $"Se encontraron {totalCount} productos");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting products");
            return ApiResponse<ProductListResponse>.ErrorResult("Error interno del servidor", ex.Message);
        }
    }

    public async Task<ApiResponse<List<string>>> GetCategoriesAsync()
    {
        try
        {
            _logger.LogInformation("Getting product categories");

            var categories = await _context.Productos
                .Where(p => p.Estado)
                .Select(p => p.Categoria)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            return ApiResponse<List<string>>.SuccessResult(categories, $"Se encontraron {categories.Count} categorías");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting categories");
            return ApiResponse<List<string>>.ErrorResult("Error interno del servidor", ex.Message);
        }
    }

    public async Task<ApiResponse<Producto>> UpdateProductPriceAsync(string productoId, double nuevoPrecio, string usuarioResponsable)
    {
        try
        {
            _logger.LogInformation("Updating price for product: {ProductId} to {NewPrice}", productoId, nuevoPrecio);

            var producto = await _context.Productos.FindAsync(productoId);
            if (producto == null)
            {
                return ApiResponse<Producto>.ErrorResult("Producto no encontrado");
            }

            var precioAnterior = producto.Precio;
            producto.Precio = nuevoPrecio;
            producto.FechaActualizacion = DateTime.UtcNow;

            // Create price change log (could be implemented as a separate entity)
            _logger.LogInformation("Price changed for product {ProductId}: {OldPrice} -> {NewPrice} by {User}", 
                productoId, precioAnterior, nuevoPrecio, usuarioResponsable);

            await _context.SaveChangesAsync();

            return ApiResponse<Producto>.SuccessResult(producto, "Precio actualizado exitosamente");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating price for product: {ProductId}", productoId);
            return ApiResponse<Producto>.ErrorResult("Error interno del servidor", ex.Message);
        }
    }

    public async Task<ApiResponse<List<Producto>>> GetLowStockProductsAsync()
    {
        try
        {
            _logger.LogInformation("Getting low stock products");

            var lowStockProducts = await _context.Productos
                .Where(p => p.Estado && p.Stock <= p.StockMinimo)
                .OrderBy(p => p.Stock)
                .ToListAsync();

            return ApiResponse<List<Producto>>.SuccessResult(lowStockProducts, $"Se encontraron {lowStockProducts.Count} productos con stock bajo");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting low stock products");
            return ApiResponse<List<Producto>>.ErrorResult("Error interno del servidor", ex.Message);
        }
    }

    private string GenerateProductId()
    {
        return $"PROD{DateTime.UtcNow:yyyyMMddHHmmss}{Random.Shared.Next(100, 999)}";
    }
}

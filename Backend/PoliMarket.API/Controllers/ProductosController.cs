using Microsoft.AspNetCore.Mvc;
using PoliMarket.Components.Products;
using PoliMarket.Models.Entities;

namespace PoliMarket.API.Controllers;

/// <summary>
/// Products Controller - Product Catalog Management
/// Handles CRUD operations for products and catalog management
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Tags("Products")]
public class ProductosController : ControllerBase
{
    private readonly IProductosComponent _productosComponent;
    private readonly ILogger<ProductosController> _logger;

    public ProductosController(IProductosComponent productosComponent, ILogger<ProductosController> logger)
    {
        _productosComponent = productosComponent;
        _logger = logger;
    }

    /// <summary>
    /// Get all products with pagination and filtering
    /// </summary>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10)</param>
    /// <param name="categoria">Category filter</param>
    /// <param name="activo">Active status filter</param>
    /// <param name="searchTerm">Search term for name/description</param>
    /// <returns>List of products with pagination info</returns>
    [HttpGet]
    public async Task<IActionResult> GetProducts(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? categoria = null,
        [FromQuery] bool? activo = null,
        [FromQuery] string? searchTerm = null)
    {
        var result = await _productosComponent.GetProductsAsync(page, pageSize, categoria, activo, searchTerm);
        return Ok(result);
    }

    /// <summary>
    /// Get product by ID
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <returns>Product information</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetProduct(string id)
    {
        var result = await _productosComponent.GetProductByIdAsync(id);
        
        if (result.Success)
        {
            return Ok(result);
        }
        
        return NotFound(result);
    }

    /// <summary>
    /// Create new product
    /// </summary>
    /// <param name="request">Product creation request</param>
    /// <returns>Created product information</returns>
    [HttpPost]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest request)
    {
        var producto = new Producto
        {
            Nombre = request.Nombre,
            Descripcion = request.Descripcion,
            Precio = request.Precio,
            Categoria = request.Categoria,
            Stock = request.Stock,
            StockMinimo = request.StockMinimo,
            StockMaximo = request.StockMaximo,
            UnidadMedida = request.UnidadMedida
        };

        var result = await _productosComponent.CreateProductAsync(producto);
        
        if (result.Success)
        {
            return CreatedAtAction(nameof(GetProduct), new { id = result.Data?.Id }, result);
        }
        
        return BadRequest(result);
    }

    /// <summary>
    /// Update existing product
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <param name="request">Product update request</param>
    /// <returns>Updated product information</returns>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(string id, [FromBody] CreateProductRequest request)
    {
        var producto = new Producto
        {
            Id = id,
            Nombre = request.Nombre,
            Descripcion = request.Descripcion,
            Precio = request.Precio,
            Categoria = request.Categoria,
            Stock = request.Stock,
            StockMinimo = request.StockMinimo,
            StockMaximo = request.StockMaximo,
            UnidadMedida = request.UnidadMedida,
            Estado = true
        };

        var result = await _productosComponent.UpdateProductAsync(id, producto);
        
        if (result.Success)
        {
            return Ok(result);
        }
        
        return BadRequest(result);
    }

    /// <summary>
    /// Delete product (soft delete)
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <returns>Deletion result</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(string id)
    {
        var result = await _productosComponent.DeleteProductAsync(id);
        
        if (result.Success)
        {
            return Ok(result);
        }
        
        return BadRequest(result);
    }

    /// <summary>
    /// Get all product categories
    /// </summary>
    /// <returns>List of categories</returns>
    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories()
    {
        var result = await _productosComponent.GetCategoriesAsync();
        return Ok(result);
    }

    /// <summary>
    /// Update product price
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <param name="request">Price update request</param>
    /// <returns>Updated product information</returns>
    [HttpPatch("{id}/price")]
    public async Task<IActionResult> UpdatePrice(string id, [FromBody] UpdatePriceRequest request)
    {
        var result = await _productosComponent.UpdateProductPriceAsync(id, request.NuevoPrecio, request.UsuarioResponsable);
        
        if (result.Success)
        {
            return Ok(result);
        }
        
        return BadRequest(result);
    }

    /// <summary>
    /// Get products with low stock
    /// </summary>
    /// <returns>List of low stock products</returns>
    [HttpGet("low-stock")]
    public async Task<IActionResult> GetLowStockProducts()
    {
        var result = await _productosComponent.GetLowStockProductsAsync();
        return Ok(result);
    }
}

using Microsoft.AspNetCore.Mvc;
using PoliMarket.Models.Common;
using PoliMarket.Models.Entities;
using PoliMarket.Components.Suppliers;

namespace PoliMarket.API.Controllers;

/// <summary>
/// Suppliers Controller (RF5) - Component-Based Architecture
/// Handles supplier management and performance tracking operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ProveedoresController : ControllerBase
{
    private readonly ILogger<ProveedoresController> _logger;
    private readonly ISupplierComponent _supplierComponent;

    public ProveedoresController(ILogger<ProveedoresController> logger, ISupplierComponent supplierComponent)
    {
        _logger = logger;
        _supplierComponent = supplierComponent;
    }

    /// <summary>
    /// Gets all suppliers
    /// </summary>
    /// <returns>API response with list of suppliers</returns>
    [HttpGet("suppliers")]
    [ProducesResponseType(typeof(ApiResponse<List<Proveedor>>), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<ApiResponse<List<Proveedor>>>> GetSuppliers()
    {
        try
        {
            _logger.LogInformation("Request received for all suppliers");

            var suppliers = await _supplierComponent.GetAllSuppliersAsync();
            var result = ApiResponse<List<Proveedor>>.SuccessResult(suppliers, "Suppliers retrieved successfully");
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in GetSuppliers");
            return StatusCode(500, ApiResponse<List<Proveedor>>.ErrorResult("Internal server error"));
        }
    }

    /// <summary>
    /// Gets supplier by ID
    /// </summary>
    /// <param name="id">Supplier ID</param>
    /// <returns>API response with supplier information</returns>
    [HttpGet("supplier/{id}")]
    [ProducesResponseType(typeof(ApiResponse<Proveedor>), 200)]
    [ProducesResponseType(typeof(ApiResponse<Proveedor>), 404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<ApiResponse<Proveedor>>> GetSupplierById(string id)
    {
        try
        {
            _logger.LogInformation("Request for supplier information: {SupplierId}", id);

            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest(ApiResponse<Proveedor>.ErrorResult("Supplier ID is required"));
            }

            var supplier = await _supplierComponent.GetSupplierByIdAsync(id);
            if (supplier == null)
            {
                return NotFound(ApiResponse<Proveedor>.ErrorResult("Supplier not found"));
            }

            var result = ApiResponse<Proveedor>.SuccessResult(supplier, "Supplier found");
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in GetSupplierById");
            return StatusCode(500, ApiResponse<Proveedor>.ErrorResult("Internal server error"));
        }
    }

    /// <summary>
    /// Creates a new supplier
    /// </summary>
    /// <param name="supplier">Supplier information</param>
    /// <returns>API response with created supplier</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<Proveedor>), 200)]
    [ProducesResponseType(typeof(ApiResponse<Proveedor>), 400)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<ApiResponse<Proveedor>>> CreateSupplier([FromBody] Proveedor supplier)
    {
        try
        {
            _logger.LogInformation("Supplier creation request for: {SupplierName}", supplier.Nombre);

            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<Proveedor>.ErrorResult("Invalid supplier data", 
                    ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()));
            }

            // Set default values
            supplier.Id = Guid.NewGuid().ToString();
            supplier.FechaRegistro = DateTime.Now;
            supplier.Activo = true;
            supplier.Calificacion = 0.0;

            // Create supplier using component
            var createdSupplier = await _supplierComponent.CreateSupplierAsync(supplier);

            var result = ApiResponse<Proveedor>.SuccessResult(createdSupplier, "Supplier created successfully");
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in CreateSupplier");
            return StatusCode(500, ApiResponse<Proveedor>.ErrorResult("Internal server error"));
        }
    }

    /// <summary>
    /// Updates an existing supplier
    /// </summary>
    /// <param name="id">Supplier ID</param>
    /// <param name="supplier">Updated supplier information</param>
    /// <returns>API response with updated supplier</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<Proveedor>), 200)]
    [ProducesResponseType(typeof(ApiResponse<Proveedor>), 400)]
    [ProducesResponseType(typeof(ApiResponse<Proveedor>), 404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<ApiResponse<Proveedor>>> UpdateSupplier(string id, [FromBody] Proveedor supplier)
    {
        try
        {
            _logger.LogInformation("Supplier update request: {SupplierId}", id);

            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<Proveedor>.ErrorResult("Invalid supplier data"));
            }

            // Update supplier using component
            supplier.Id = id; // Ensure the ID is set
            var updatedSupplier = await _supplierComponent.UpdateSupplierAsync(supplier);

            var result = ApiResponse<Proveedor>.SuccessResult(updatedSupplier, "Supplier updated successfully");
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in UpdateSupplier");
            return StatusCode(500, ApiResponse<Proveedor>.ErrorResult("Internal server error"));
        }
    }

    /// <summary>
    /// Deactivates a supplier
    /// </summary>
    /// <param name="id">Supplier ID</param>
    /// <returns>API response with deactivation result</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    [ProducesResponseType(typeof(ApiResponse<bool>), 404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<ApiResponse<bool>>> DeactivateSupplier(string id)
    {
        try
        {
            _logger.LogInformation("Supplier deactivation request: {SupplierId}", id);

            // Deactivate supplier using component
            var success = await _supplierComponent.DeactivateSupplierAsync(id);
            if (!success)
            {
                return NotFound(ApiResponse<bool>.ErrorResult("Supplier not found"));
            }

            var result = ApiResponse<bool>.SuccessResult(true, "Supplier deactivated successfully");
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in DeactivateSupplier");
            return StatusCode(500, ApiResponse<bool>.ErrorResult("Internal server error"));
        }
    }

    /// <summary>
    /// Evaluates supplier performance
    /// </summary>
    /// <param name="id">Supplier ID</param>
    /// <param name="request">Evaluation request</param>
    /// <returns>API response with evaluation result</returns>
    [HttpPost("{id}/evaluate")]
    [ProducesResponseType(typeof(ApiResponse<Proveedor>), 200)]
    [ProducesResponseType(typeof(ApiResponse<Proveedor>), 400)]
    [ProducesResponseType(typeof(ApiResponse<Proveedor>), 404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<ApiResponse<Proveedor>>> EvaluateSupplier(string id, [FromBody] SupplierEvaluationRequest request)
    {
        try
        {
            _logger.LogInformation("Supplier evaluation request: {SupplierId}", id);

            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<Proveedor>.ErrorResult("Invalid evaluation data"));
            }

            // Evaluate supplier using component
            var success = await _supplierComponent.EvaluateSupplierAsync(id, request.Rating, request.Comentarios);
            if (!success)
            {
                return NotFound(ApiResponse<Proveedor>.ErrorResult("Supplier not found"));
            }

            // Get updated supplier to return
            var supplier = await _supplierComponent.GetSupplierByIdAsync(id);
            var result = ApiResponse<Proveedor>.SuccessResult(supplier!, "Supplier evaluated successfully");
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in EvaluateSupplier");
            return StatusCode(500, ApiResponse<Proveedor>.ErrorResult("Internal server error"));
        }
    }

    /// <summary>
    /// Gets supplier performance metrics
    /// </summary>
    /// <param name="id">Supplier ID</param>
    /// <returns>API response with performance metrics</returns>
    [HttpGet("{id}/performance")]
    [ProducesResponseType(typeof(ApiResponse<SupplierPerformanceMetrics>), 200)]
    [ProducesResponseType(typeof(ApiResponse<SupplierPerformanceMetrics>), 404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<ApiResponse<SupplierPerformanceMetrics>>> GetSupplierPerformance(string id)
    {
        try
        {
            _logger.LogInformation("Supplier performance request: {SupplierId}", id);

            // Get performance metrics using component
            var metrics = await _supplierComponent.GetSupplierPerformanceAsync(id);
            if (metrics == null)
            {
                return NotFound(ApiResponse<SupplierPerformanceMetrics>.ErrorResult("Supplier not found"));
            }

            var result = ApiResponse<SupplierPerformanceMetrics>.SuccessResult(metrics, "Performance metrics retrieved");
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in GetSupplierPerformance");
            return StatusCode(500, ApiResponse<SupplierPerformanceMetrics>.ErrorResult("Internal server error"));
        }
    }

    /// <summary>
    /// Gets suppliers by category
    /// </summary>
    /// <param name="category">Category name</param>
    /// <returns>API response with suppliers in category</returns>
    [HttpGet("category/{category}")]
    [ProducesResponseType(typeof(ApiResponse<List<Proveedor>>), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<ApiResponse<List<Proveedor>>>> GetSuppliersByCategory(string category)
    {
        try
        {
            _logger.LogInformation("Request for suppliers in category: {Category}", category);

            // Get suppliers by category using component
            var suppliers = await _supplierComponent.GetSuppliersByCategoryAsync(category);

            var result = ApiResponse<List<Proveedor>>.SuccessResult(suppliers, $"Suppliers in category '{category}' retrieved");
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in GetSuppliersByCategory");
            return StatusCode(500, ApiResponse<List<Proveedor>>.ErrorResult("Internal server error"));
        }
    }
}

/// <summary>
/// Supplier evaluation request
/// </summary>
public class SupplierEvaluationRequest
{
    public double Rating { get; set; }
    public string Comments { get; set; } = string.Empty;
    public string Comentarios { get; set; } = string.Empty;
    public string EvaluatedBy { get; set; } = string.Empty;
}

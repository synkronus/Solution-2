using Microsoft.AspNetCore.Mvc;
using PoliMarket.Contracts;
using PoliMarket.Models.Common;
using PoliMarket.Models.Entities;

namespace PoliMarket.API.Controllers;

/// <summary>
/// Inventory Controller (RF3) - Component-Based Architecture
/// Handles inventory management and stock control operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class InventarioController : ControllerBase
{
    private readonly IInventarioComponent _inventarioComponent;
    private readonly ILogger<InventarioController> _logger;

    public InventarioController(
        IInventarioComponent inventarioComponent,
        ILogger<InventarioController> logger)
    {
        _inventarioComponent = inventarioComponent;
        _logger = logger;
    }

    /// <summary>
    /// Checks product availability for a given quantity
    /// </summary>
    /// <param name="request">Availability check request</param>
    /// <returns>API response with availability information</returns>
    [HttpPost("check-availability")]
    [ProducesResponseType(typeof(ApiResponse<DisponibilidadInventario>), 200)]
    [ProducesResponseType(typeof(ApiResponse<DisponibilidadInventario>), 400)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<ApiResponse<DisponibilidadInventario>>> CheckAvailability([FromBody] AvailabilityCheckRequest request)
    {
        try
        {
            _logger.LogInformation("Availability check request for product: {ProductId}", request.IdProducto);

            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<DisponibilidadInventario>.ErrorResult("Invalid request data", 
                    ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()));
            }

            var result = await _inventarioComponent.CheckAvailabilityAsync(request);
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in CheckAvailability");
            return StatusCode(500, ApiResponse<DisponibilidadInventario>.ErrorResult("Internal server error"));
        }
    }

    /// <summary>
    /// Gets current stock for a product
    /// </summary>
    /// <param name="productoId">Product ID</param>
    /// <returns>API response with current stock information</returns>
    [HttpGet("stock/{productoId}")]
    [ProducesResponseType(typeof(ApiResponse<DisponibilidadInventario>), 200)]
    [ProducesResponseType(typeof(ApiResponse<DisponibilidadInventario>), 404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<ApiResponse<DisponibilidadInventario>>> GetCurrentStock(string productoId)
    {
        try
        {
            _logger.LogInformation("Current stock request for product: {ProductId}", productoId);

            if (string.IsNullOrWhiteSpace(productoId))
            {
                return BadRequest(ApiResponse<DisponibilidadInventario>.ErrorResult("Product ID is required"));
            }

            var result = await _inventarioComponent.GetCurrentStockAsync(productoId);
            
            if (result.Success)
            {
                return Ok(result);
            }
            
            return NotFound(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in GetCurrentStock");
            return StatusCode(500, ApiResponse<DisponibilidadInventario>.ErrorResult("Internal server error"));
        }
    }

    /// <summary>
    /// Updates product stock
    /// </summary>
    /// <param name="request">Stock update request</param>
    /// <returns>API response with stock operation result</returns>
    [HttpPut("stock")]
    [ProducesResponseType(typeof(ApiResponse<StockOperationResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<StockOperationResponse>), 400)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<ApiResponse<StockOperationResponse>>> UpdateStock([FromBody] StockUpdateRequest request)
    {
        try
        {
            _logger.LogInformation("Stock update request for product: {ProductId}", request.IdProducto);

            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<StockOperationResponse>.ErrorResult("Invalid request data"));
            }

            var result = await _inventarioComponent.UpdateStockAsync(request);
            
            if (result.Success)
            {
                return Ok(result);
            }
            
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in UpdateStock");
            return StatusCode(500, ApiResponse<StockOperationResponse>.ErrorResult("Internal server error"));
        }
    }

    /// <summary>
    /// Gets stock movement history for a product
    /// </summary>
    /// <param name="productoId">Product ID</param>
    /// <param name="fechaInicio">Start date filter (optional)</param>
    /// <param name="fechaFin">End date filter (optional)</param>
    /// <returns>API response with movement history</returns>
    [HttpGet("movements/{productoId}")]
    [ProducesResponseType(typeof(ApiResponse<List<MovimientoInventario>>), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<ApiResponse<List<MovimientoInventario>>>> GetStockMovements(
        string productoId, 
        [FromQuery] DateTime? fechaInicio = null, 
        [FromQuery] DateTime? fechaFin = null)
    {
        try
        {
            _logger.LogInformation("Stock movements request for product: {ProductId}", productoId);

            if (string.IsNullOrWhiteSpace(productoId))
            {
                return BadRequest(ApiResponse<List<MovimientoInventario>>.ErrorResult("Product ID is required"));
            }

            var result = await _inventarioComponent.GetStockMovementsAsync(productoId, fechaInicio, fechaFin);
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in GetStockMovements");
            return StatusCode(500, ApiResponse<List<MovimientoInventario>>.ErrorResult("Internal server error"));
        }
    }

    /// <summary>
    /// Generates stock alerts based on minimum/maximum levels
    /// </summary>
    /// <returns>API response with list of stock alerts</returns>
    [HttpGet("alerts")]
    [ProducesResponseType(typeof(ApiResponse<List<AlertaStock>>), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<ApiResponse<List<AlertaStock>>>> GenerateStockAlerts()
    {
        try
        {
            _logger.LogInformation("Stock alerts generation request");

            var result = await _inventarioComponent.GenerateStockAlertsAsync();
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in GenerateStockAlerts");
            return StatusCode(500, ApiResponse<List<AlertaStock>>.ErrorResult("Internal server error"));
        }
    }

    /// <summary>
    /// Reserves stock for a sale
    /// </summary>
    /// <param name="request">Stock reservation request</param>
    /// <returns>API response with reservation result</returns>
    [HttpPost("reserve")]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    [ProducesResponseType(typeof(ApiResponse<bool>), 400)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<ApiResponse<bool>>> ReserveStock([FromBody] ReserveStockRequest request)
    {
        try
        {
            _logger.LogInformation("Stock reservation request for product: {ProductId}", request.IdProducto);

            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<bool>.ErrorResult("Invalid request data"));
            }

            var result = await _inventarioComponent.ReserveStockAsync(request.IdProducto, request.Cantidad, request.VentaId);
            
            if (result.Success)
            {
                return Ok(result);
            }
            
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in ReserveStock");
            return StatusCode(500, ApiResponse<bool>.ErrorResult("Internal server error"));
        }
    }

    /// <summary>
    /// Processes stock adjustment
    /// </summary>
    /// <param name="request">Stock adjustment request</param>
    /// <returns>API response with adjustment result</returns>
    [HttpPost("adjust")]
    [ProducesResponseType(typeof(ApiResponse<StockOperationResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<StockOperationResponse>), 400)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<ApiResponse<StockOperationResponse>>> AdjustStock([FromBody] AdjustStockRequest request)
    {
        try
        {
            _logger.LogInformation("Stock adjustment request for product: {ProductId}", request.IdProducto);

            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<StockOperationResponse>.ErrorResult("Invalid request data"));
            }

            var result = await _inventarioComponent.AdjustStockAsync(
                request.IdProducto, 
                request.NuevoStock, 
                request.Motivo, 
                request.UsuarioResponsable);
            
            if (result.Success)
            {
                return Ok(result);
            }
            
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in AdjustStock");
            return StatusCode(500, ApiResponse<StockOperationResponse>.ErrorResult("Internal server error"));
        }
    }
}

/// <summary>
/// Request model for stock reservation
/// </summary>
public class ReserveStockRequest
{
    public string IdProducto { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public string VentaId { get; set; } = string.Empty;
}

/// <summary>
/// Request model for stock adjustment
/// </summary>
public class AdjustStockRequest
{
    public string IdProducto { get; set; } = string.Empty;
    public int NuevoStock { get; set; }
    public string Motivo { get; set; } = string.Empty;
    public string UsuarioResponsable { get; set; } = string.Empty;
}

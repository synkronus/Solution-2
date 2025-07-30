using Microsoft.AspNetCore.Mvc;
using PoliMarket.Contracts;
using PoliMarket.Models.Common;
using PoliMarket.Models.Entities;

namespace PoliMarket.API.Controllers;

/// <summary>
/// Sales Controller (RF2) - Component-Based Architecture
/// Handles sales processing and transaction management operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class VentasController : ControllerBase
{
    private readonly IVentasComponent _ventasComponent;
    private readonly ILogger<VentasController> _logger;

    public VentasController(
        IVentasComponent ventasComponent,
        ILogger<VentasController> logger)
    {
        _ventasComponent = ventasComponent;
        _logger = logger;
    }

    /// <summary>
    /// Processes a new sale transaction
    /// </summary>
    /// <param name="request">Sale creation request</param>
    /// <returns>API response with sale processing result</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<SaleProcessingResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<SaleProcessingResponse>), 400)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<ApiResponse<SaleProcessingResponse>>> ProcessSale([FromBody] CreateSaleRequest request)
    {
        try
        {
            _logger.LogInformation("Sale processing request received for customer: {IdCliente}", request.IdCliente);

            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<SaleProcessingResponse>.ErrorResult("Invalid request data", 
                    ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()));
            }

            var result = await _ventasComponent.ProcessSaleAsync(request);
            
            if (result.Success)
            {
                return Ok(result);
            }
            
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in ProcessSale");
            return StatusCode(500, ApiResponse<SaleProcessingResponse>.ErrorResult("Internal server error"));
        }
    }

    /// <summary>
    /// Calculates total for a sale including taxes and discounts
    /// </summary>
    /// <param name="detalles">List of sale details</param>
    /// <returns>API response with calculated total</returns>
    [HttpPost("calculate-total")]
    [ProducesResponseType(typeof(ApiResponse<double>), 200)]
    [ProducesResponseType(typeof(ApiResponse<double>), 400)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<ApiResponse<double>>> CalculateTotal([FromBody] List<SaleDetailRequest> detalles)
    {
        try
        {
            _logger.LogInformation("Total calculation request received for {Count} items", detalles.Count);

            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<double>.ErrorResult("Invalid request data"));
            }

            var result = await _ventasComponent.CalculateSaleTotalAsync(detalles);
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in CalculateTotal");
            return StatusCode(500, ApiResponse<double>.ErrorResult("Internal server error"));
        }
    }

    /// <summary>
    /// Gets sale by ID
    /// </summary>
    /// <param name="ventaId">Sale ID</param>
    /// <returns>API response with sale information</returns>
    [HttpGet("{ventaId}")]
    [ProducesResponseType(typeof(ApiResponse<Venta>), 200)]
    [ProducesResponseType(typeof(ApiResponse<Venta>), 404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<ApiResponse<Venta>>> GetSaleById(string ventaId)
    {
        try
        {
            _logger.LogInformation("Request for sale information: {VentaId}", ventaId);

            if (string.IsNullOrWhiteSpace(ventaId))
            {
                return BadRequest(ApiResponse<Venta>.ErrorResult("Sale ID is required"));
            }

            var result = await _ventasComponent.GetSaleByIdAsync(ventaId);
            
            if (result.Success)
            {
                return Ok(result);
            }
            
            return NotFound(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in GetSaleById");
            return StatusCode(500, ApiResponse<Venta>.ErrorResult("Internal server error"));
        }
    }

    /// <summary>
    /// Gets sales by seller
    /// </summary>
    /// <param name="vendedorId">Seller ID</param>
    /// <param name="fechaInicio">Start date filter (optional)</param>
    /// <param name="fechaFin">End date filter (optional)</param>
    /// <returns>API response with list of sales</returns>
    [HttpGet("by-seller/{vendedorId}")]
    [ProducesResponseType(typeof(ApiResponse<List<Venta>>), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<ApiResponse<List<Venta>>>> GetSalesBySeller(
        string vendedorId, 
        [FromQuery] DateTime? fechaInicio = null, 
        [FromQuery] DateTime? fechaFin = null)
    {
        try
        {
            _logger.LogInformation("Request for sales by seller: {VendedorId}", vendedorId);

            if (string.IsNullOrWhiteSpace(vendedorId))
            {
                return BadRequest(ApiResponse<List<Venta>>.ErrorResult("Seller ID is required"));
            }

            var result = await _ventasComponent.GetSalesBySellerAsync(vendedorId, fechaInicio, fechaFin);
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in GetSalesBySeller");
            return StatusCode(500, ApiResponse<List<Venta>>.ErrorResult("Internal server error"));
        }
    }

    /// <summary>
    /// Applies discount to an existing sale
    /// </summary>
    /// <param name="ventaId">Sale ID</param>
    /// <param name="request">Discount application request</param>
    /// <returns>API response with updated sale</returns>
    [HttpPut("{ventaId}/discount")]
    [ProducesResponseType(typeof(ApiResponse<Venta>), 200)]
    [ProducesResponseType(typeof(ApiResponse<Venta>), 400)]
    [ProducesResponseType(typeof(ApiResponse<Venta>), 404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<ApiResponse<Venta>>> ApplyDiscount(
        string ventaId, 
        [FromBody] ApplyDiscountRequest request)
    {
        try
        {
            _logger.LogInformation("Discount application request for sale: {VentaId}", ventaId);

            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<Venta>.ErrorResult("Invalid request data"));
            }

            var result = await _ventasComponent.ApplyDiscountAsync(ventaId, request.Descuento, request.Motivo);
            
            if (result.Success)
            {
                return Ok(result);
            }
            
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in ApplyDiscount");
            return StatusCode(500, ApiResponse<Venta>.ErrorResult("Internal server error"));
        }
    }

    /// <summary>
    /// Cancels a sale
    /// </summary>
    /// <param name="ventaId">Sale ID</param>
    /// <param name="request">Cancellation request</param>
    /// <returns>API response with cancellation result</returns>
    [HttpDelete("{ventaId}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    [ProducesResponseType(typeof(ApiResponse<bool>), 400)]
    [ProducesResponseType(typeof(ApiResponse<bool>), 404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<ApiResponse<bool>>> CancelSale(
        string ventaId, 
        [FromBody] CancelSaleRequest request)
    {
        try
        {
            _logger.LogInformation("Sale cancellation request: {VentaId}", ventaId);

            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<bool>.ErrorResult("Invalid request data"));
            }

            var result = await _ventasComponent.CancelSaleAsync(ventaId, request.Motivo);
            
            if (result.Success)
            {
                return Ok(result);
            }
            
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in CancelSale");
            return StatusCode(500, ApiResponse<bool>.ErrorResult("Internal server error"));
        }
    }
}

/// <summary>
/// Request model for applying discount
/// </summary>
public class ApplyDiscountRequest
{
    public double Descuento { get; set; }
    public string Motivo { get; set; } = string.Empty;
}

/// <summary>
/// Request model for cancelling sale
/// </summary>
public class CancelSaleRequest
{
    public string Motivo { get; set; } = string.Empty;
}

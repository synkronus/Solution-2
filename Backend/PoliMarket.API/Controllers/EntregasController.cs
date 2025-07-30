using Microsoft.AspNetCore.Mvc;
using PoliMarket.Models.Common;
using PoliMarket.Models.Entities;
using PoliMarket.Components.Delivery;

namespace PoliMarket.API.Controllers;

/// <summary>
/// Deliveries Controller (RF4) - Component-Based Architecture
/// Handles delivery scheduling and logistics operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class EntregasController : ControllerBase
{
    private readonly ILogger<EntregasController> _logger;
    private readonly IEntregasComponent _entregasComponent;

    public EntregasController(ILogger<EntregasController> logger, IEntregasComponent entregasComponent)
    {
        _logger = logger;
        _entregasComponent = entregasComponent;
    }

    /// <summary>
    /// Gets deliveries filtered by vendor ID (query parameter)
    /// </summary>
    /// <param name="vendorId">Optional vendor ID to filter deliveries</param>
    /// <returns>API response with list of deliveries</returns>
    [HttpGet("deliveries")]
    [ProducesResponseType(typeof(ApiResponse<List<Entrega>>), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<ApiResponse<List<Entrega>>>> GetDeliveries([FromQuery] string? vendorId = null)
    {
        try
        {
            _logger.LogInformation("Request received for deliveries with vendorId: {VendorId}", vendorId);

            ApiResponse<List<Entrega>> result;

            if (!string.IsNullOrEmpty(vendorId))
            {
                result = await _entregasComponent.GetDeliveriesByVendorAsync(vendorId);
            }
            else
            {
                result = await _entregasComponent.GetAllDeliveriesAsync();
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in GetDeliveries");
            return StatusCode(500, ApiResponse<List<Entrega>>.ErrorResult("Internal server error"));
        }
    }

    /// <summary>
    /// Gets delivery by ID
    /// </summary>
    /// <param name="id">Delivery ID</param>
    /// <returns>API response with delivery information</returns>
    [HttpGet("delivery/{id}")]
    [ProducesResponseType(typeof(ApiResponse<Entrega>), 200)]
    [ProducesResponseType(typeof(ApiResponse<Entrega>), 404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<ApiResponse<Entrega>>> GetDeliveryById(string id)
    {
        try
        {
            _logger.LogInformation("Request for delivery information: {DeliveryId}", id);

            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest(ApiResponse<Entrega>.ErrorResult("Delivery ID is required"));
            }

            var result = await _entregasComponent.GetDeliveryByIdAsync(id);

            if (!result.Success)
            {
                return NotFound(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in GetDeliveryById");
            return StatusCode(500, ApiResponse<Entrega>.ErrorResult("Internal server error"));
        }
    }

    /// <summary>
    /// Schedules a new delivery
    /// </summary>
    /// <param name="request">Schedule delivery request</param>
    /// <returns>API response with scheduled delivery</returns>
    [HttpPost("schedule")]
    [ProducesResponseType(typeof(ApiResponse<Entrega>), 200)]
    [ProducesResponseType(typeof(ApiResponse<Entrega>), 400)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<ApiResponse<Entrega>>> ScheduleDelivery([FromBody] ScheduleDeliveryRequest request)
    {
        try
        {
            _logger.LogInformation("Delivery scheduling request for client: {Cliente}", request.Cliente);

            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<Entrega>.ErrorResult("Invalid delivery data",
                    ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()));
            }

            var result = await _entregasComponent.ScheduleDeliveryAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in ScheduleDelivery");
            return StatusCode(500, ApiResponse<Entrega>.ErrorResult("Internal server error"));
        }
    }

    /// <summary>
    /// Gets pending deliveries
    /// </summary>
    /// <returns>API response with list of pending deliveries</returns>
    [HttpGet("pending")]
    [ProducesResponseType(typeof(ApiResponse<List<Entrega>>), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<ApiResponse<List<Entrega>>>> GetPendingDeliveries()
    {
        try
        {
            _logger.LogInformation("Request received for pending deliveries");

            var result = await _entregasComponent.GetPendingDeliveriesAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in GetPendingDeliveries");
            return StatusCode(500, ApiResponse<List<Entrega>>.ErrorResult("Internal server error"));
        }
    }

    /// <summary>
    /// Confirms a delivery
    /// </summary>
    /// <param name="id">Delivery ID</param>
    /// <returns>API response with confirmation result</returns>
    [HttpPost("confirm/{id}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    [ProducesResponseType(typeof(ApiResponse<bool>), 404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<ApiResponse<bool>>> ConfirmDelivery(string id)
    {
        try
        {
            _logger.LogInformation("Delivery confirmation request: {DeliveryId}", id);

            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest(ApiResponse<bool>.ErrorResult("Delivery ID is required"));
            }

            // Mock implementation
            var result = ApiResponse<bool>.SuccessResult(true, "Delivery confirmed successfully");
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in ConfirmDelivery");
            return StatusCode(500, ApiResponse<bool>.ErrorResult("Internal server error"));
        }
    }

    /// <summary>
    /// Gets delivery tracking information
    /// </summary>
    /// <param name="id">Delivery ID</param>
    /// <returns>API response with tracking information</returns>
    [HttpGet("tracking/{id}")]
    [ProducesResponseType(typeof(ApiResponse<DeliveryTrackingInfo>), 200)]
    [ProducesResponseType(typeof(ApiResponse<DeliveryTrackingInfo>), 404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<ApiResponse<DeliveryTrackingInfo>>> GetDeliveryTracking(string id)
    {
        try
        {
            _logger.LogInformation("Delivery tracking request: {DeliveryId}", id);

            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest(ApiResponse<DeliveryTrackingInfo>.ErrorResult("Delivery ID is required"));
            }

            // Mock tracking info
            var trackingInfo = new DeliveryTrackingInfo
            {
                Id = id,
                UbicacionActual = "Ruta de entrega - Zona Norte",
                EstadoDetallado = "En tránsito hacia destino",
                TiempoEstimado = "2 horas aproximadamente",
                HistorialMovimientos = new List<TrackingMovimiento>
                {
                    new TrackingMovimiento { Fecha = DateTime.Now.AddHours(-2).ToString("yyyy-MM-ddTHH:mm:ss"), Ubicacion = "Centro de distribución", Estado = "Despachado", Observaciones = "Paquete despachado desde centro de distribución" },
                    new TrackingMovimiento { Fecha = DateTime.Now.AddHours(-1).ToString("yyyy-MM-ddTHH:mm:ss"), Ubicacion = "Punto de control Norte", Estado = "En tránsito", Observaciones = "Paquete en ruta hacia destino" },
                    new TrackingMovimiento { Fecha = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"), Ubicacion = "Zona de entrega", Estado = "Próximo a entregar", Observaciones = "Transportista se dirige al destino final" }
                }
            };

            var result = ApiResponse<DeliveryTrackingInfo>.SuccessResult(trackingInfo, "Tracking information retrieved");
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in GetDeliveryTracking");
            return StatusCode(500, ApiResponse<DeliveryTrackingInfo>.ErrorResult("Internal server error"));
        }
    }

    /// <summary>
    /// Updates delivery status
    /// </summary>
    /// <param name="request">Status update request</param>
    /// <returns>API response with updated delivery</returns>
    [HttpPut("update-status")]
    [ProducesResponseType(typeof(ApiResponse<Entrega>), 200)]
    [ProducesResponseType(typeof(ApiResponse<Entrega>), 400)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<ApiResponse<Entrega>>> UpdateDeliveryStatus([FromBody] DeliveryUpdateRequest request)
    {
        try
        {
            _logger.LogInformation("Delivery status update request: {DeliveryId}", request.DeliveryId);

            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<Entrega>.ErrorResult("Invalid request data"));
            }

            // Mock implementation
            var updatedDelivery = new Entrega
            {
                Id = request.DeliveryId,
                Estado = request.NewStatus,
                Observaciones = request.Notes
            };

            var result = ApiResponse<Entrega>.SuccessResult(updatedDelivery, "Delivery status updated successfully");
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in UpdateDeliveryStatus");
            return StatusCode(500, ApiResponse<Entrega>.ErrorResult("Internal server error"));
        }
    }
}

/// <summary>
/// Delivery tracking information
/// </summary>
public class DeliveryTrackingInfo
{
    public string Id { get; set; } = string.Empty;
    public string UbicacionActual { get; set; } = string.Empty;
    public string EstadoDetallado { get; set; } = string.Empty;
    public string TiempoEstimado { get; set; } = string.Empty;
    public List<TrackingMovimiento> HistorialMovimientos { get; set; } = new();
}

/// <summary>
/// Tracking movement
/// </summary>
public class TrackingMovimiento
{
    public string Fecha { get; set; } = string.Empty;
    public string Ubicacion { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public string Observaciones { get; set; } = string.Empty;
}

/// <summary>
/// Delivery update request
/// </summary>
public class DeliveryUpdateRequest
{
    public string DeliveryId { get; set; } = string.Empty;
    public string NewStatus { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
}

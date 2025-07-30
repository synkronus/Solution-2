using Microsoft.AspNetCore.Mvc;
using PoliMarket.Contracts;
using PoliMarket.Models.Common;

namespace PoliMarket.API.Controllers;

/// <summary>
/// Integration Controller - Component-Based Architecture
/// Handles system integration, orchestration, and health monitoring
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class IntegracionController : ControllerBase
{
    private readonly IIntegracionComponent _integracionComponent;
    private readonly ILogger<IntegracionController> _logger;

    public IntegracionController(
        IIntegracionComponent integracionComponent,
        ILogger<IntegracionController> logger)
    {
        _integracionComponent = integracionComponent;
        _logger = logger;
    }

    /// <summary>
    /// Executes a business transaction across multiple components
    /// </summary>
    /// <param name="request">Business transaction request</param>
    /// <returns>API response with transaction result</returns>
    [HttpPost("execute-transaction")]
    [ProducesResponseType(typeof(ApiResponse<TransactionResult>), 200)]
    [ProducesResponseType(typeof(ApiResponse<TransactionResult>), 400)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<ApiResponse<TransactionResult>>> ExecuteBusinessTransaction([FromBody] BusinessTransactionRequest request)
    {
        try
        {
            _logger.LogInformation("Business transaction execution request: {TransactionType}", request.TransactionType);

            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<TransactionResult>.ErrorResult("Invalid request data", 
                    ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()));
            }

            var result = await _integracionComponent.ExecuteBusinessTransactionAsync(request);
            
            if (result.Success)
            {
                return Ok(result);
            }
            
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in ExecuteBusinessTransaction");
            return StatusCode(500, ApiResponse<TransactionResult>.ErrorResult("Internal server error"));
        }
    }

    /// <summary>
    /// Checks system health and component availability
    /// </summary>
    /// <returns>API response with health check results</returns>
    [HttpGet("health")]
    [ProducesResponseType(typeof(ApiResponse<SystemHealthResponse>), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<ApiResponse<SystemHealthResponse>>> CheckSystemHealth()
    {
        try
        {
            _logger.LogInformation("System health check request");

            var result = await _integracionComponent.CheckSystemHealthAsync();
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in CheckSystemHealth");
            return StatusCode(500, ApiResponse<SystemHealthResponse>.ErrorResult("Internal server error"));
        }
    }

    /// <summary>
    /// Synchronizes data between components
    /// </summary>
    /// <param name="request">Data synchronization request</param>
    /// <returns>API response with synchronization result</returns>
    [HttpPost("synchronize")]
    [ProducesResponseType(typeof(ApiResponse<SyncResult>), 200)]
    [ProducesResponseType(typeof(ApiResponse<SyncResult>), 400)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<ApiResponse<SyncResult>>> SynchronizeData([FromBody] DataSyncRequest request)
    {
        try
        {
            _logger.LogInformation("Data synchronization request from {Source} to {Target}", 
                request.SourceComponent, request.TargetComponent);

            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<SyncResult>.ErrorResult("Invalid request data"));
            }

            var result = await _integracionComponent.SynchronizeDataAsync(request);
            
            if (result.Success)
            {
                return Ok(result);
            }
            
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in SynchronizeData");
            return StatusCode(500, ApiResponse<SyncResult>.ErrorResult("Internal server error"));
        }
    }

    /// <summary>
    /// Publishes an event to all interested components
    /// </summary>
    /// <param name="eventData">Event data to publish</param>
    /// <returns>API response with publish result</returns>
    [HttpPost("publish-event")]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    [ProducesResponseType(typeof(ApiResponse<bool>), 400)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<ApiResponse<bool>>> PublishEvent([FromBody] ComponentEvent eventData)
    {
        try
        {
            _logger.LogInformation("Event publishing request: {EventType} from {SourceComponent}", 
                eventData.EventType, eventData.SourceComponent);

            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<bool>.ErrorResult("Invalid request data"));
            }

            var result = await _integracionComponent.PublishEventAsync(eventData);
            
            if (result.Success)
            {
                return Ok(result);
            }
            
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in PublishEvent");
            return StatusCode(500, ApiResponse<bool>.ErrorResult("Internal server error"));
        }
    }

    /// <summary>
    /// Gets component configuration
    /// </summary>
    /// <param name="componentName">Component name</param>
    /// <returns>API response with configuration data</returns>
    [HttpGet("configuration/{componentName}")]
    [ProducesResponseType(typeof(ApiResponse<Dictionary<string, object>>), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<ApiResponse<Dictionary<string, object>>>> GetComponentConfiguration(string componentName)
    {
        try
        {
            _logger.LogInformation("Configuration request for component: {ComponentName}", componentName);

            if (string.IsNullOrWhiteSpace(componentName))
            {
                return BadRequest(ApiResponse<Dictionary<string, object>>.ErrorResult("Component name is required"));
            }

            var result = await _integracionComponent.GetComponentConfigurationAsync(componentName);
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in GetComponentConfiguration");
            return StatusCode(500, ApiResponse<Dictionary<string, object>>.ErrorResult("Internal server error"));
        }
    }

    /// <summary>
    /// Gets system status overview
    /// </summary>
    /// <returns>API response with system status</returns>
    [HttpGet("status")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<ApiResponse<object>>> GetSystemStatus()
    {
        try
        {
            _logger.LogInformation("System status request");

            var healthResult = await _integracionComponent.CheckSystemHealthAsync();
            
            var status = new
            {
                SystemName = "PoliMarket CBSE",
                Version = "1.0.0",
                Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development",
                Timestamp = DateTime.UtcNow,
                Health = healthResult.Data,
                ComponentsCount = healthResult.Data?.ComponentsHealth?.Count ?? 0,
                HealthyComponents = healthResult.Data?.ComponentsHealth?.Values.Count(h => h.IsHealthy) ?? 0
            };

            return Ok(ApiResponse<object>.SuccessResult(status, "System status retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in GetSystemStatus");
            return StatusCode(500, ApiResponse<object>.ErrorResult("Internal server error"));
        }
    }
}

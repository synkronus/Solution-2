using Microsoft.AspNetCore.Mvc;
using PoliMarket.Contracts;
using PoliMarket.Models.Common;
using PoliMarket.Models.Entities;

namespace PoliMarket.API.Controllers;

/// <summary>
/// Authorization Controller (RF1) - Component-Based Architecture
/// Handles seller authorization and HR management operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AutorizacionController : ControllerBase
{
    private readonly IAutorizacionComponent _autorizacionComponent;
    private readonly ILogger<AutorizacionController> _logger;

    public AutorizacionController(
        IAutorizacionComponent autorizacionComponent,
        ILogger<AutorizacionController> logger)
    {
        _autorizacionComponent = autorizacionComponent;
        _logger = logger;
    }

    /// <summary>
    /// Authorizes a new seller in the system
    /// </summary>
    /// <param name="request">Authorization request with seller details</param>
    /// <returns>API response with authorized seller information</returns>
    [HttpPost("authorize")]
    [ProducesResponseType(typeof(ApiResponse<Vendedor>), 200)]
    [ProducesResponseType(typeof(ApiResponse<Vendedor>), 400)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<ApiResponse<Vendedor>>> AuthorizeVendedor([FromBody] AuthorizationRequest request)
    {
        try
        {
            _logger.LogInformation("Authorization request received for seller: {CodigoVendedor}", request.CodigoVendedor);

            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<Vendedor>.ErrorResult("Invalid request data", 
                    ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()));
            }

            var result = await _autorizacionComponent.AuthorizeVendedorAsync(request);
            
            if (result.Success)
            {
                return Ok(result);
            }
            
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in AuthorizeVendedor");
            return StatusCode(500, ApiResponse<Vendedor>.ErrorResult("Internal server error"));
        }
    }

    /// <summary>
    /// Validates if a seller is currently authorized
    /// </summary>
    /// <param name="codigoVendedor">Seller code to validate</param>
    /// <returns>API response with validation result</returns>
    [HttpGet("validate/{codigoVendedor}")]
    [ProducesResponseType(typeof(ApiResponse<ValidationResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<ValidationResponse>), 404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<ApiResponse<ValidationResponse>>> ValidateAuthorization(string codigoVendedor)
    {
        try
        {
            _logger.LogInformation("Validation request received for seller: {CodigoVendedor}", codigoVendedor);

            if (string.IsNullOrWhiteSpace(codigoVendedor))
            {
                return BadRequest(ApiResponse<ValidationResponse>.ErrorResult("Seller code is required"));
            }

            var result = await _autorizacionComponent.ValidateAuthorizationAsync(codigoVendedor);
            
            if (result.Success)
            {
                return Ok(result);
            }
            
            return NotFound(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in ValidateAuthorization");
            return StatusCode(500, ApiResponse<ValidationResponse>.ErrorResult("Internal server error"));
        }
    }

    /// <summary>
    /// Retrieves all authorized sellers
    /// </summary>
    /// <returns>API response with list of authorized sellers</returns>
    [HttpGet("vendedores")]
    [ProducesResponseType(typeof(ApiResponse<List<Vendedor>>), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<ApiResponse<List<Vendedor>>>> GetAuthorizedVendedores()
    {
        try
        {
            _logger.LogInformation("Request received for all authorized sellers");

            var result = await _autorizacionComponent.GetAuthorizedVendedoresAsync();
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in GetAuthorizedVendedores");
            return StatusCode(500, ApiResponse<List<Vendedor>>.ErrorResult("Internal server error"));
        }
    }

    /// <summary>
    /// Updates seller authorization status
    /// </summary>
    /// <param name="codigoVendedor">Seller code</param>
    /// <param name="request">Authorization update request</param>
    /// <returns>API response with update result</returns>
    [HttpPut("vendedores/{codigoVendedor}/authorization")]
    [ProducesResponseType(typeof(ApiResponse<Vendedor>), 200)]
    [ProducesResponseType(typeof(ApiResponse<Vendedor>), 400)]
    [ProducesResponseType(typeof(ApiResponse<Vendedor>), 404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<ApiResponse<Vendedor>>> UpdateAuthorizationStatus(
        string codigoVendedor, 
        [FromBody] UpdateAuthorizationRequest request)
    {
        try
        {
            _logger.LogInformation("Authorization update request for seller: {CodigoVendedor}", codigoVendedor);

            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<Vendedor>.ErrorResult("Invalid request data"));
            }

            var result = await _autorizacionComponent.UpdateAuthorizationStatusAsync(
                codigoVendedor, request.Autorizado, request.EmpleadoRH);
            
            if (result.Success)
            {
                return Ok(result);
            }
            
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in UpdateAuthorizationStatus");
            return StatusCode(500, ApiResponse<Vendedor>.ErrorResult("Internal server error"));
        }
    }

    /// <summary>
    /// Gets seller information by code
    /// </summary>
    /// <param name="codigoVendedor">Seller code</param>
    /// <returns>API response with seller information</returns>
    [HttpGet("vendedores/{codigoVendedor}")]
    [ProducesResponseType(typeof(ApiResponse<Vendedor>), 200)]
    [ProducesResponseType(typeof(ApiResponse<Vendedor>), 404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<ApiResponse<Vendedor>>> GetVendedorByCode(string codigoVendedor)
    {
        try
        {
            _logger.LogInformation("Request for seller information: {CodigoVendedor}", codigoVendedor);

            if (string.IsNullOrWhiteSpace(codigoVendedor))
            {
                return BadRequest(ApiResponse<Vendedor>.ErrorResult("Seller code is required"));
            }

            var result = await _autorizacionComponent.GetVendedorByCodeAsync(codigoVendedor);
            
            if (result.Success)
            {
                return Ok(result);
            }
            
            return NotFound(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in GetVendedorByCode");
            return StatusCode(500, ApiResponse<Vendedor>.ErrorResult("Internal server error"));
        }
    }

    /// <summary>
    /// Validates HR employee permissions
    /// </summary>
    /// <param name="empleadoRHId">HR employee ID</param>
    /// <returns>API response with validation result</returns>
    [HttpGet("hr-employees/{empleadoRHId}/validate")]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<ApiResponse<bool>>> ValidateHREmployee(string empleadoRHId)
    {
        try
        {
            _logger.LogInformation("HR employee validation request: {EmpleadoRHId}", empleadoRHId);

            if (string.IsNullOrWhiteSpace(empleadoRHId))
            {
                return BadRequest(ApiResponse<bool>.ErrorResult("HR employee ID is required"));
            }

            var result = await _autorizacionComponent.ValidateHREmployeeAsync(empleadoRHId);
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in ValidateHREmployee");
            return StatusCode(500, ApiResponse<bool>.ErrorResult("Internal server error"));
        }
    }

    /// <summary>
    /// Gets all HR employees
    /// </summary>
    /// <returns>API response with list of HR employees</returns>
    [HttpGet("hr-employees")]
    [ProducesResponseType(typeof(ApiResponse<List<EmpleadoRH>>), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<ApiResponse<List<EmpleadoRH>>>> GetHREmployees()
    {
        try
        {
            _logger.LogInformation("Request received for all HR employees");

            var result = await _autorizacionComponent.GetHREmployeesAsync();

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in GetHREmployees");
            return StatusCode(500, ApiResponse<List<EmpleadoRH>>.ErrorResult("Internal server error"));
        }
    }

    /// <summary>
    /// Gets all sellers pending authorization
    /// </summary>
    /// <returns>API response with list of pending sellers</returns>
    [HttpGet("pending-sellers")]
    [ProducesResponseType(typeof(ApiResponse<List<Vendedor>>), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<ApiResponse<List<Vendedor>>>> GetPendingSellers()
    {
        try
        {
            _logger.LogInformation("Request received for pending sellers");

            var result = await _autorizacionComponent.GetPendingSellersAsync();

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in GetPendingSellers");
            return StatusCode(500, ApiResponse<List<Vendedor>>.ErrorResult("Internal server error"));
        }
    }
}

/// <summary>
/// Request model for updating authorization status
/// </summary>
public class UpdateAuthorizationRequest
{
    public bool Autorizado { get; set; }
    public string EmpleadoRH { get; set; } = string.Empty;
}

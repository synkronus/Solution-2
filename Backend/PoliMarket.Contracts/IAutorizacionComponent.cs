using PoliMarket.Models.Common;
using PoliMarket.Models.Entities;

namespace PoliMarket.Contracts;

/// <summary>
/// Interface for Authorization Component (RF1)
/// Provides seller authorization and permission management services
/// Reusability Level: Very High (95%) - Universal security component
/// </summary>
public interface IAutorizacionComponent
{
    /// <summary>
    /// Authorizes a new seller in the system
    /// </summary>
    /// <param name="request">Authorization request with seller details</param>
    /// <returns>API response with authorized seller information</returns>
    Task<ApiResponse<Vendedor>> AuthorizeVendedorAsync(AuthorizationRequest request);

    /// <summary>
    /// Validates if a seller is currently authorized
    /// </summary>
    /// <param name="codigoVendedor">Seller code to validate</param>
    /// <returns>API response with validation result</returns>
    Task<ApiResponse<ValidationResponse>> ValidateAuthorizationAsync(string codigoVendedor);

    /// <summary>
    /// Retrieves all authorized sellers
    /// </summary>
    /// <returns>API response with list of authorized sellers</returns>
    Task<ApiResponse<List<Vendedor>>> GetAuthorizedVendedoresAsync();

    /// <summary>
    /// Updates seller authorization status
    /// </summary>
    /// <param name="codigoVendedor">Seller code</param>
    /// <param name="autorizado">New authorization status</param>
    /// <param name="empleadoRH">HR employee making the change</param>
    /// <returns>API response with update result</returns>
    Task<ApiResponse<Vendedor>> UpdateAuthorizationStatusAsync(string codigoVendedor, bool autorizado, string empleadoRH);

    /// <summary>
    /// Validates HR employee permissions
    /// </summary>
    /// <param name="empleadoRHId">HR employee ID</param>
    /// <returns>API response with validation result</returns>
    Task<ApiResponse<bool>> ValidateHREmployeeAsync(string empleadoRHId);

    /// <summary>
    /// Gets seller by code
    /// </summary>
    /// <param name="codigoVendedor">Seller code</param>
    /// <returns>API response with seller information</returns>
    Task<ApiResponse<Vendedor>> GetVendedorByCodeAsync(string codigoVendedor);

    /// <summary>
    /// Gets all HR employees
    /// </summary>
    /// <returns>API response with list of HR employees</returns>
    Task<ApiResponse<List<EmpleadoRH>>> GetHREmployeesAsync();

    /// <summary>
    /// Gets all sellers pending authorization
    /// </summary>
    /// <returns>API response with list of pending sellers</returns>
    Task<ApiResponse<List<Vendedor>>> GetPendingSellersAsync();
}

/// <summary>
/// Interface for HR Employee management
/// </summary>
public interface IGestionEmpleadosComponent
{
    /// <summary>
    /// Registers a new HR employee
    /// </summary>
    /// <param name="empleado">HR employee information</param>
    /// <returns>API response with created employee</returns>
    Task<ApiResponse<EmpleadoRH>> RegisterEmpleadoAsync(EmpleadoRH empleado);

    /// <summary>
    /// Gets all active HR employees
    /// </summary>
    /// <returns>API response with list of HR employees</returns>
    Task<ApiResponse<List<EmpleadoRH>>> GetActiveEmpleadosAsync();

    /// <summary>
    /// Updates HR employee information
    /// </summary>
    /// <param name="empleado">Updated employee information</param>
    /// <returns>API response with update result</returns>
    Task<ApiResponse<EmpleadoRH>> UpdateEmpleadoAsync(EmpleadoRH empleado);

    /// <summary>
    /// Deactivates an HR employee
    /// </summary>
    /// <param name="empleadoId">Employee ID</param>
    /// <returns>API response with deactivation result</returns>
    Task<ApiResponse<bool>> DeactivateEmpleadoAsync(string empleadoId);
}

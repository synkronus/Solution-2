using Microsoft.Extensions.Logging;
using PoliMarket.Contracts;
using PoliMarket.Models.Common;
using PoliMarket.Models.Entities;

namespace PoliMarket.Components.Authorization;

/// <summary>
/// Authorization Component Implementation (RF1)
/// Provides seller authorization and permission management services
/// Reusability Level: Very High (95%) - Universal security component
/// </summary>
public class AutorizacionComponent : IAutorizacionComponent
{
    private readonly ILogger<AutorizacionComponent> _logger;
    private readonly IAutorizacionRepository _repository;

    public AutorizacionComponent(
        ILogger<AutorizacionComponent> logger,
        IAutorizacionRepository repository)
    {
        _logger = logger;
        _repository = repository;
    }

    public async Task<ApiResponse<Vendedor>> AuthorizeVendedorAsync(AuthorizationRequest request)
    {
        try
        {
            _logger.LogInformation("Authorizing seller: {CodigoVendedor}", request.CodigoVendedor);

            // Validate HR employee
            var hrEmployee = await _repository.GetHREmployeeByIdAsync(request.EmpleadoRH);
            if (hrEmployee == null || !hrEmployee.Activo)
            {
                return ApiResponse<Vendedor>.ErrorResult("Empleado de RH no válido o inactivo");
            }

            // Check if seller already exists
            var existingSeller = await _repository.GetVendedorByCodeAsync(request.CodigoVendedor);
            if (existingSeller != null)
            {
                return ApiResponse<Vendedor>.ErrorResult("El vendedor ya existe en el sistema");
            }

            // Create new seller
            var vendedor = new Vendedor
            {
                CodigoVendedor = request.CodigoVendedor,
                Nombre = request.Nombre,
                Territorio = request.Territorio,
                Comision = request.Comision,
                Autorizado = true,
                FechaAutorizacion = DateTime.UtcNow,
                EmpleadoRHAutorizo = request.EmpleadoRH,
                FechaCreacion = DateTime.UtcNow,
                Activo = true
            };

            var createdSeller = await _repository.CreateVendedorAsync(vendedor);
            
            _logger.LogInformation("Seller authorized successfully: {CodigoVendedor}", request.CodigoVendedor);
            
            return ApiResponse<Vendedor>.SuccessResult(createdSeller, "Vendedor autorizado exitosamente");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error authorizing seller: {CodigoVendedor}", request.CodigoVendedor);
            return ApiResponse<Vendedor>.ErrorResult("Error interno del servidor", ex.Message);
        }
    }

    public async Task<ApiResponse<ValidationResponse>> ValidateAuthorizationAsync(string codigoVendedor)
    {
        try
        {
            _logger.LogInformation("Validating seller authorization: {CodigoVendedor}", codigoVendedor);

            var vendedor = await _repository.GetVendedorByCodeAsync(codigoVendedor);
            
            var response = new ValidationResponse();

            if (vendedor == null)
            {
                response.IsValid = false;
                response.Reason = "Vendedor no encontrado";
            }
            else if (!vendedor.Activo)
            {
                response.IsValid = false;
                response.Reason = "Vendedor inactivo";
            }
            else if (!vendedor.Autorizado)
            {
                response.IsValid = false;
                response.Reason = "Vendedor no autorizado";
            }
            else
            {
                response.IsValid = true;
                response.Reason = "Vendedor autorizado y activo";
                response.Vendedor = vendedor;
            }

            return ApiResponse<ValidationResponse>.SuccessResult(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating seller: {CodigoVendedor}", codigoVendedor);
            return ApiResponse<ValidationResponse>.ErrorResult("Error interno del servidor", ex.Message);
        }
    }

    public async Task<ApiResponse<List<Vendedor>>> GetAuthorizedVendedoresAsync()
    {
        try
        {
            _logger.LogInformation("Getting all authorized sellers");

            var vendedores = await _repository.GetAuthorizedVendedoresAsync();
            
            return ApiResponse<List<Vendedor>>.SuccessResult(vendedores, $"Se encontraron {vendedores.Count} vendedores autorizados");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting authorized sellers");
            return ApiResponse<List<Vendedor>>.ErrorResult("Error interno del servidor", ex.Message);
        }
    }

    public async Task<ApiResponse<Vendedor>> UpdateAuthorizationStatusAsync(string codigoVendedor, bool autorizado, string empleadoRH)
    {
        try
        {
            _logger.LogInformation("Updating authorization status for seller: {CodigoVendedor}", codigoVendedor);

            // Validate HR employee
            var hrEmployee = await _repository.GetHREmployeeByIdAsync(empleadoRH);
            if (hrEmployee == null || !hrEmployee.Activo)
            {
                return ApiResponse<Vendedor>.ErrorResult("Empleado de RH no válido o inactivo");
            }

            var vendedor = await _repository.GetVendedorByCodeAsync(codigoVendedor);
            if (vendedor == null)
            {
                return ApiResponse<Vendedor>.ErrorResult("Vendedor no encontrado");
            }

            vendedor.Autorizado = autorizado;
            vendedor.EmpleadoRHAutorizo = empleadoRH;
            vendedor.FechaAutorizacion = DateTime.UtcNow;
            vendedor.FechaActualizacion = DateTime.UtcNow;

            var updatedSeller = await _repository.UpdateVendedorAsync(vendedor);
            
            _logger.LogInformation("Authorization status updated for seller: {CodigoVendedor}", codigoVendedor);
            
            return ApiResponse<Vendedor>.SuccessResult(updatedSeller, "Estado de autorización actualizado exitosamente");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating authorization status: {CodigoVendedor}", codigoVendedor);
            return ApiResponse<Vendedor>.ErrorResult("Error interno del servidor", ex.Message);
        }
    }

    public async Task<ApiResponse<bool>> ValidateHREmployeeAsync(string empleadoRHId)
    {
        try
        {
            _logger.LogInformation("Validating HR employee: {EmpleadoRHId}", empleadoRHId);

            var hrEmployee = await _repository.GetHREmployeeByIdAsync(empleadoRHId);
            var isValid = hrEmployee != null && hrEmployee.Activo;
            
            return ApiResponse<bool>.SuccessResult(isValid, isValid ? "Empleado de RH válido" : "Empleado de RH no válido");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating HR employee: {EmpleadoRHId}", empleadoRHId);
            return ApiResponse<bool>.ErrorResult("Error interno del servidor", ex.Message);
        }
    }

    public async Task<ApiResponse<Vendedor>> GetVendedorByCodeAsync(string codigoVendedor)
    {
        try
        {
            _logger.LogInformation("Getting seller by code: {CodigoVendedor}", codigoVendedor);

            var vendedor = await _repository.GetVendedorByCodeAsync(codigoVendedor);
            
            if (vendedor == null)
            {
                return ApiResponse<Vendedor>.ErrorResult("Vendedor no encontrado");
            }

            return ApiResponse<Vendedor>.SuccessResult(vendedor, "Vendedor encontrado exitosamente");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting seller: {CodigoVendedor}", codigoVendedor);
            return ApiResponse<Vendedor>.ErrorResult("Error interno del servidor", ex.Message);
        }
    }

    public async Task<ApiResponse<List<EmpleadoRH>>> GetHREmployeesAsync()
    {
        try
        {
            _logger.LogInformation("Getting all HR employees");

            var hrEmployees = await _repository.GetAllHREmployeesAsync();

            return ApiResponse<List<EmpleadoRH>>.SuccessResult(hrEmployees, "Empleados de RH obtenidos exitosamente");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting HR employees");
            return ApiResponse<List<EmpleadoRH>>.ErrorResult("Error interno del servidor", ex.Message);
        }
    }

    public async Task<ApiResponse<List<Vendedor>>> GetPendingSellersAsync()
    {
        try
        {
            _logger.LogInformation("Getting pending sellers");

            var pendingSellers = await _repository.GetPendingSellersAsync();

            return ApiResponse<List<Vendedor>>.SuccessResult(pendingSellers, "Vendedores pendientes obtenidos exitosamente");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending sellers");
            return ApiResponse<List<Vendedor>>.ErrorResult("Error interno del servidor", ex.Message);
        }
    }
}

using PoliMarket.Models.Entities;

namespace PoliMarket.Components.Authorization;

/// <summary>
/// Repository interface for Authorization Component data access
/// </summary>
public interface IAutorizacionRepository
{
    // Vendedor operations
    Task<Vendedor?> GetVendedorByCodeAsync(string codigoVendedor);
    Task<List<Vendedor>> GetAuthorizedVendedoresAsync();
    Task<List<Vendedor>> GetPendingSellersAsync();
    Task<Vendedor> CreateVendedorAsync(Vendedor vendedor);
    Task<Vendedor> UpdateVendedorAsync(Vendedor vendedor);
    Task<bool> DeleteVendedorAsync(string codigoVendedor);

    // HR Employee operations
    Task<EmpleadoRH?> GetHREmployeeByIdAsync(string empleadoId);
    Task<List<EmpleadoRH>> GetActiveHREmployeesAsync();
    Task<List<EmpleadoRH>> GetAllHREmployeesAsync();
    Task<EmpleadoRH> CreateHREmployeeAsync(EmpleadoRH empleado);
    Task<EmpleadoRH> UpdateHREmployeeAsync(EmpleadoRH empleado);
    Task<bool> DeactivateHREmployeeAsync(string empleadoId);
}

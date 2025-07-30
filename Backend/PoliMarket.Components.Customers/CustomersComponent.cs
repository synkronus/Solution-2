using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PoliMarket.Components.Infrastructure.Data;
using PoliMarket.Models.Entities;
using PoliMarket.Models.Common;
using PoliMarket.Contracts;

namespace PoliMarket.Components.Customers;

/// <summary>
/// Implementation of Customer Management Component
/// Provides customer management and relationship services
/// </summary>
public class CustomersComponent : ICustomersComponent
{
    private readonly PoliMarketDbContext _context;
    private readonly ILogger<CustomersComponent> _logger;

    public CustomersComponent(PoliMarketDbContext context, ILogger<CustomersComponent> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ApiResponse<Cliente>> CreateCustomerAsync(Cliente cliente)
    {
        try
        {
            _logger.LogInformation("Creating customer: {CustomerName}", cliente.Nombre);

            // Generate ID if not provided
            if (string.IsNullOrEmpty(cliente.Id))
            {
                cliente.Id = GenerateCustomerId();
            }

            // Check if customer ID already exists
            var existingCustomer = await _context.Clientes.FindAsync(cliente.Id);
            if (existingCustomer != null)
            {
                return ApiResponse<Cliente>.ErrorResult("El ID del cliente ya existe");
            }

            // Check if email already exists
            if (!string.IsNullOrEmpty(cliente.Email))
            {
                var existingEmail = await _context.Clientes
                    .FirstOrDefaultAsync(c => c.Email == cliente.Email && c.Activo);
                if (existingEmail != null)
                {
                    return ApiResponse<Cliente>.ErrorResult("El email ya está registrado");
                }
            }

            cliente.FechaRegistro = DateTime.UtcNow;
            cliente.Activo = true;

            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Customer created successfully: {CustomerId}", cliente.Id);
            return ApiResponse<Cliente>.SuccessResult(cliente, "Cliente creado exitosamente");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating customer: {CustomerName}", cliente.Nombre);
            return ApiResponse<Cliente>.ErrorResult("Error interno del servidor", ex.Message);
        }
    }

    public async Task<ApiResponse<Cliente>> GetCustomerByIdAsync(string clienteId)
    {
        try
        {
            _logger.LogInformation("Getting customer: {CustomerId}", clienteId);

            var cliente = await _context.Clientes
                .Include(c => c.Ventas)
                .FirstOrDefaultAsync(c => c.Id == clienteId);

            if (cliente == null)
            {
                return ApiResponse<Cliente>.ErrorResult("Cliente no encontrado");
            }

            return ApiResponse<Cliente>.SuccessResult(cliente, "Cliente encontrado");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customer: {CustomerId}", clienteId);
            return ApiResponse<Cliente>.ErrorResult("Error interno del servidor", ex.Message);
        }
    }

    public async Task<ApiResponse<Cliente>> UpdateCustomerAsync(string clienteId, Cliente cliente)
    {
        try
        {
            _logger.LogInformation("Updating customer: {CustomerId}", clienteId);

            var existingCustomer = await _context.Clientes.FindAsync(clienteId);
            if (existingCustomer == null)
            {
                return ApiResponse<Cliente>.ErrorResult("Cliente no encontrado");
            }

            // Check if email already exists (excluding current customer)
            if (!string.IsNullOrEmpty(cliente.Email) && cliente.Email != existingCustomer.Email)
            {
                var existingEmail = await _context.Clientes
                    .FirstOrDefaultAsync(c => c.Email == cliente.Email && c.Activo && c.Id != clienteId);
                if (existingEmail != null)
                {
                    return ApiResponse<Cliente>.ErrorResult("El email ya está registrado");
                }
            }

            // Update properties
            existingCustomer.Nombre = cliente.Nombre;
            existingCustomer.Direccion = cliente.Direccion;
            existingCustomer.Telefono = cliente.Telefono;
            existingCustomer.Email = cliente.Email;
            existingCustomer.TipoCliente = cliente.TipoCliente;
            existingCustomer.LimiteCredito = cliente.LimiteCredito;
            existingCustomer.Activo = cliente.Activo;
            existingCustomer.FechaActualizacion = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Customer updated successfully: {CustomerId}", clienteId);
            return ApiResponse<Cliente>.SuccessResult(existingCustomer, "Cliente actualizado exitosamente");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating customer: {CustomerId}", clienteId);
            return ApiResponse<Cliente>.ErrorResult("Error interno del servidor", ex.Message);
        }
    }

    public async Task<ApiResponse<bool>> DeleteCustomerAsync(string clienteId)
    {
        try
        {
            _logger.LogInformation("Deleting customer: {CustomerId}", clienteId);

            var cliente = await _context.Clientes.FindAsync(clienteId);
            if (cliente == null)
            {
                return ApiResponse<bool>.ErrorResult("Cliente no encontrado");
            }

            // Check if customer has sales
            var hasSales = await _context.Ventas.AnyAsync(v => v.IdCliente == clienteId);
            if (hasSales)
            {
                // Soft delete if has sales
                cliente.Activo = false;
                cliente.FechaActualizacion = DateTime.UtcNow;
            }
            else
            {
                // Hard delete if no sales
                _context.Clientes.Remove(cliente);
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Customer deleted successfully: {CustomerId}", clienteId);
            return ApiResponse<bool>.SuccessResult(true, "Cliente eliminado exitosamente");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting customer: {CustomerId}", clienteId);
            return ApiResponse<bool>.ErrorResult("Error interno del servidor", ex.Message);
        }
    }

    public async Task<ApiResponse<CustomerListResponse>> GetCustomersAsync(int page = 1, int pageSize = 10, string? tipoCliente = null, bool? activo = null, string? searchTerm = null)
    {
        try
        {
            _logger.LogInformation("Getting customers - Page: {Page}, PageSize: {PageSize}", page, pageSize);

            var query = _context.Clientes.AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(tipoCliente))
            {
                query = query.Where(c => c.TipoCliente == tipoCliente);
            }

            if (activo.HasValue)
            {
                query = query.Where(c => c.Activo == activo.Value);
            }

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(c => c.Nombre.Contains(searchTerm) || c.Email.Contains(searchTerm));
            }

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            var clientes = await query
                .OrderBy(c => c.Nombre)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var response = new CustomerListResponse
            {
                Customers = clientes,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = totalPages
            };

            return ApiResponse<CustomerListResponse>.SuccessResult(response, $"Se encontraron {totalCount} clientes");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customers");
            return ApiResponse<CustomerListResponse>.ErrorResult("Error interno del servidor", ex.Message);
        }
    }

    public async Task<ApiResponse<List<string>>> GetCustomerTypesAsync()
    {
        try
        {
            _logger.LogInformation("Getting customer types");

            var types = await _context.Clientes
                .Where(c => c.Activo)
                .Select(c => c.TipoCliente)
                .Distinct()
                .OrderBy(t => t)
                .ToListAsync();

            return ApiResponse<List<string>>.SuccessResult(types, $"Se encontraron {types.Count} tipos de cliente");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customer types");
            return ApiResponse<List<string>>.ErrorResult("Error interno del servidor", ex.Message);
        }
    }

    public async Task<ApiResponse<Cliente>> UpdateCreditLimitAsync(string clienteId, double nuevoLimite, string usuarioResponsable)
    {
        try
        {
            _logger.LogInformation("Updating credit limit for customer: {CustomerId} to {NewLimit}", clienteId, nuevoLimite);

            var cliente = await _context.Clientes.FindAsync(clienteId);
            if (cliente == null)
            {
                return ApiResponse<Cliente>.ErrorResult("Cliente no encontrado");
            }

            var limiteAnterior = cliente.LimiteCredito;
            cliente.LimiteCredito = nuevoLimite;
            cliente.FechaActualizacion = DateTime.UtcNow;

            // Log credit limit change
            _logger.LogInformation("Credit limit changed for customer {CustomerId}: {OldLimit} -> {NewLimit} by {User}", 
                clienteId, limiteAnterior, nuevoLimite, usuarioResponsable);

            await _context.SaveChangesAsync();

            return ApiResponse<Cliente>.SuccessResult(cliente, "Límite de crédito actualizado exitosamente");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating credit limit for customer: {CustomerId}", clienteId);
            return ApiResponse<Cliente>.ErrorResult("Error interno del servidor", ex.Message);
        }
    }

    public async Task<ApiResponse<List<Venta>>> GetCustomerSalesHistoryAsync(string clienteId, DateTime? fechaInicio = null, DateTime? fechaFin = null)
    {
        try
        {
            _logger.LogInformation("Getting sales history for customer: {CustomerId}", clienteId);

            var query = _context.Ventas
                .Include(v => v.Detalles)
                .Where(v => v.IdCliente == clienteId);

            if (fechaInicio.HasValue)
            {
                query = query.Where(v => v.Fecha >= fechaInicio.Value);
            }

            if (fechaFin.HasValue)
            {
                query = query.Where(v => v.Fecha <= fechaFin.Value);
            }

            var ventas = await query
                .OrderByDescending(v => v.Fecha)
                .ToListAsync();

            return ApiResponse<List<Venta>>.SuccessResult(ventas, $"Se encontraron {ventas.Count} ventas");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting sales history for customer: {CustomerId}", clienteId);
            return ApiResponse<List<Venta>>.ErrorResult("Error interno del servidor", ex.Message);
        }
    }

    private string GenerateCustomerId()
    {
        return $"CLI{DateTime.UtcNow:yyyyMMddHHmmss}{Random.Shared.Next(100, 999)}";
    }
}

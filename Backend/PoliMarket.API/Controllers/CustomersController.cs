using Microsoft.AspNetCore.Mvc;
using PoliMarket.Components.Customers;
using PoliMarket.Models.Entities;

namespace PoliMarket.API.Controllers;

/// <summary>
/// Customers Controller - Customer Management
/// Handles CRUD operations for customers and customer relationship management
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Tags("Customers")]
public class CustomersController : ControllerBase
{
    private readonly ICustomersComponent _customersComponent;
    private readonly ILogger<CustomersController> _logger;

    public CustomersController(ICustomersComponent customersComponent, ILogger<CustomersController> logger)
    {
        _customersComponent = customersComponent;
        _logger = logger;
    }

    /// <summary>
    /// Get all customers with pagination and filtering
    /// </summary>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10)</param>
    /// <param name="tipoCliente">Customer type filter</param>
    /// <param name="activo">Active status filter</param>
    /// <param name="searchTerm">Search term for name/email</param>
    /// <returns>List of customers with pagination info</returns>
    [HttpGet]
    public async Task<IActionResult> GetCustomers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? tipoCliente = null,
        [FromQuery] bool? activo = null,
        [FromQuery] string? searchTerm = null)
    {
        var result = await _customersComponent.GetCustomersAsync(page, pageSize, tipoCliente, activo, searchTerm);
        return Ok(result);
    }

    /// <summary>
    /// Get customer by ID
    /// </summary>
    /// <param name="id">Customer ID</param>
    /// <returns>Customer information</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetCustomer(string id)
    {
        var result = await _customersComponent.GetCustomerByIdAsync(id);
        
        if (result.Success)
        {
            return Ok(result);
        }
        
        return NotFound(result);
    }

    /// <summary>
    /// Create new customer
    /// </summary>
    /// <param name="request">Customer creation request</param>
    /// <returns>Created customer information</returns>
    [HttpPost]
    public async Task<IActionResult> CreateCustomer([FromBody] CreateCustomerRequest request)
    {
        var cliente = new Cliente
        {
            Nombre = request.Nombre,
            Direccion = request.Direccion,
            Telefono = request.Telefono,
            Email = request.Email,
            TipoCliente = request.TipoCliente,
            LimiteCredito = request.LimiteCredito
        };

        var result = await _customersComponent.CreateCustomerAsync(cliente);
        
        if (result.Success)
        {
            return CreatedAtAction(nameof(GetCustomer), new { id = result.Data?.Id }, result);
        }
        
        return BadRequest(result);
    }

    /// <summary>
    /// Update existing customer
    /// </summary>
    /// <param name="id">Customer ID</param>
    /// <param name="request">Customer update request</param>
    /// <returns>Updated customer information</returns>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCustomer(string id, [FromBody] CreateCustomerRequest request)
    {
        var cliente = new Cliente
        {
            Id = id,
            Nombre = request.Nombre,
            Direccion = request.Direccion,
            Telefono = request.Telefono,
            Email = request.Email,
            TipoCliente = request.TipoCliente,
            LimiteCredito = request.LimiteCredito,
            Activo = true
        };

        var result = await _customersComponent.UpdateCustomerAsync(id, cliente);
        
        if (result.Success)
        {
            return Ok(result);
        }
        
        return BadRequest(result);
    }

    /// <summary>
    /// Delete customer (soft delete if has sales, hard delete otherwise)
    /// </summary>
    /// <param name="id">Customer ID</param>
    /// <returns>Deletion result</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCustomer(string id)
    {
        var result = await _customersComponent.DeleteCustomerAsync(id);
        
        if (result.Success)
        {
            return Ok(result);
        }
        
        return BadRequest(result);
    }

    /// <summary>
    /// Get all customer types
    /// </summary>
    /// <returns>List of customer types</returns>
    [HttpGet("types")]
    public async Task<IActionResult> GetCustomerTypes()
    {
        var result = await _customersComponent.GetCustomerTypesAsync();
        return Ok(result);
    }

    /// <summary>
    /// Update customer credit limit
    /// </summary>
    /// <param name="id">Customer ID</param>
    /// <param name="request">Credit limit update request</param>
    /// <returns>Updated customer information</returns>
    [HttpPatch("{id}/credit-limit")]
    public async Task<IActionResult> UpdateCreditLimit(string id, [FromBody] UpdateCreditLimitRequest request)
    {
        var result = await _customersComponent.UpdateCreditLimitAsync(id, request.NuevoLimite, request.UsuarioResponsable);
        
        if (result.Success)
        {
            return Ok(result);
        }
        
        return BadRequest(result);
    }

    /// <summary>
    /// Get customer sales history
    /// </summary>
    /// <param name="id">Customer ID</param>
    /// <param name="fechaInicio">Start date filter</param>
    /// <param name="fechaFin">End date filter</param>
    /// <returns>Customer sales history</returns>
    [HttpGet("{id}/sales-history")]
    public async Task<IActionResult> GetCustomerSalesHistory(
        string id,
        [FromQuery] DateTime? fechaInicio = null,
        [FromQuery] DateTime? fechaFin = null)
    {
        var result = await _customersComponent.GetCustomerSalesHistoryAsync(id, fechaInicio, fechaFin);
        return Ok(result);
    }
}

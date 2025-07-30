using Microsoft.Extensions.Logging;
using PoliMarket.Contracts;
using PoliMarket.Models.Common;
using PoliMarket.Models.Entities;

namespace PoliMarket.Components.Sales;

/// <summary>
/// Sales Component Implementation (RF2)
/// Provides sales processing and transaction management services
/// Reusability Level: High (85%) - Standard commercial processes
/// </summary>
public class VentasComponent : IVentasComponent
{
    private readonly ILogger<VentasComponent> _logger;
    private readonly IVentasRepository _repository;
    private readonly IAutorizacionComponent _authorizationComponent;
    private readonly IInventarioComponent _inventoryComponent;

    public VentasComponent(
        ILogger<VentasComponent> logger,
        IVentasRepository repository,
        IAutorizacionComponent authorizationComponent,
        IInventarioComponent inventoryComponent)
    {
        _logger = logger;
        _repository = repository;
        _authorizationComponent = authorizationComponent;
        _inventoryComponent = inventoryComponent;
    }

    public async Task<ApiResponse<SaleProcessingResponse>> ProcessSaleAsync(CreateSaleRequest request)
    {
        try
        {
            _logger.LogInformation("Processing sale for customer: {IdCliente}", request.IdCliente);

            // Validate seller authorization
            var sellerValidation = await _authorizationComponent.ValidateAuthorizationAsync(request.IdVendedor);
            if (!sellerValidation.Success || !sellerValidation.Data!.IsValid)
            {
                return ApiResponse<SaleProcessingResponse>.ErrorResult("Vendedor no autorizado");
            }

            // Validate customer exists
            var customer = await _repository.GetCustomerByIdAsync(request.IdCliente);
            if (customer == null)
            {
                return ApiResponse<SaleProcessingResponse>.ErrorResult("Cliente no encontrado");
            }

            // Validate stock availability for all products
            var stockValidationErrors = new List<string>();
            foreach (var detalle in request.Detalles)
            {
                var availabilityCheck = new AvailabilityCheckRequest
                {
                    IdProducto = detalle.IdProducto,
                    CantidadRequerida = detalle.Cantidad
                };

                var availability = await _inventoryComponent.CheckAvailabilityAsync(availabilityCheck);
                if (!availability.Success || !availability.Data!.DisponibleParaVenta)
                {
                    stockValidationErrors.Add($"Producto {detalle.IdProducto}: Stock insuficiente");
                }
            }

            if (stockValidationErrors.Any())
            {
                return ApiResponse<SaleProcessingResponse>.ErrorResult("Problemas de stock", stockValidationErrors);
            }

            // Calculate total
            var totalResponse = await CalculateSaleTotalAsync(request.Detalles);
            if (!totalResponse.Success)
            {
                return ApiResponse<SaleProcessingResponse>.ErrorResult("Error calculando el total de la venta");
            }

            // Create sale
            var ventaId = Guid.NewGuid().ToString();
            var venta = new Venta
            {
                Id = ventaId,
                Fecha = DateTime.UtcNow,
                IdCliente = request.IdCliente,
                IdVendedor = request.IdVendedor,
                Total = totalResponse.Data,
                Estado = "Procesada",
                Observaciones = request.Observaciones,
                CreadoPor = request.IdVendedor, // Set the required CreadoPor field
                FechaCreacion = DateTime.UtcNow,
                Detalles = request.Detalles.Select(d => new DetalleVenta
                {
                    Id = Guid.NewGuid().ToString(),
                    IdVenta = ventaId, // Set the foreign key
                    IdProducto = d.IdProducto,
                    Cantidad = d.Cantidad,
                    Precio = d.Precio,
                    Descuento = d.Descuento
                }).ToList()
            };

            var createdSale = await _repository.CreateSaleAsync(venta);

            // Update inventory for each product
            foreach (var detalle in request.Detalles)
            {
                var stockUpdate = new StockUpdateRequest
                {
                    IdProducto = detalle.IdProducto,
                    TipoMovimiento = "Salida",
                    Cantidad = detalle.Cantidad,
                    Motivo = $"Venta {createdSale.Id}",
                    DocumentoReferencia = createdSale.Id,
                    UsuarioResponsable = request.IdVendedor
                };

                await _inventoryComponent.UpdateStockAsync(stockUpdate);
            }

            var response = new SaleProcessingResponse
            {
                VentaId = createdSale.Id,
                Total = createdSale.Total,
                Estado = createdSale.Estado,
                FechaProcesamiento = createdSale.Fecha
            };

            _logger.LogInformation("Sale processed successfully: {VentaId}", createdSale.Id);

            return ApiResponse<SaleProcessingResponse>.SuccessResult(response, "Venta procesada exitosamente");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing sale for customer: {IdCliente}", request.IdCliente);
            return ApiResponse<SaleProcessingResponse>.ErrorResult("Error interno del servidor", ex.Message);
        }
    }

    public async Task<ApiResponse<double>> CalculateSaleTotalAsync(List<SaleDetailRequest> detalles)
    {
        try
        {
            _logger.LogInformation("Calculating sale total for {Count} items", detalles.Count);

            double total = 0;
            foreach (var detalle in detalles)
            {
                var subtotal = (detalle.Cantidad * detalle.Precio) - detalle.Descuento;
                total += subtotal;
            }

            // Apply taxes (example: 19% IVA)
            var tax = total * 0.19;
            var finalTotal = total + tax;

            return ApiResponse<double>.SuccessResult(finalTotal, "Total calculado exitosamente");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating sale total");
            return ApiResponse<double>.ErrorResult("Error calculando el total", ex.Message);
        }
    }

    public async Task<ApiResponse<Venta>> ApplyDiscountAsync(string ventaId, double descuento, string motivo)
    {
        try
        {
            _logger.LogInformation("Applying discount to sale: {VentaId}", ventaId);

            var venta = await _repository.GetSaleByIdAsync(ventaId);
            if (venta == null)
            {
                return ApiResponse<Venta>.ErrorResult("Venta no encontrada");
            }

            if (venta.Estado != "Procesada")
            {
                return ApiResponse<Venta>.ErrorResult("No se puede aplicar descuento a una venta en estado: " + venta.Estado);
            }

            venta.Total -= descuento;
            venta.FechaActualizacion = DateTime.UtcNow;
            venta.Observaciones += $" | Descuento aplicado: ${descuento} - Motivo: {motivo}";

            var updatedSale = await _repository.UpdateSaleAsync(venta);

            return ApiResponse<Venta>.SuccessResult(updatedSale, "Descuento aplicado exitosamente");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying discount to sale: {VentaId}", ventaId);
            return ApiResponse<Venta>.ErrorResult("Error interno del servidor", ex.Message);
        }
    }

    public async Task<ApiResponse<Venta>> GetSaleByIdAsync(string ventaId)
    {
        try
        {
            _logger.LogInformation("Getting sale by ID: {VentaId}", ventaId);

            var venta = await _repository.GetSaleByIdAsync(ventaId);
            if (venta == null)
            {
                return ApiResponse<Venta>.ErrorResult("Venta no encontrada");
            }

            return ApiResponse<Venta>.SuccessResult(venta, "Venta encontrada exitosamente");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting sale: {VentaId}", ventaId);
            return ApiResponse<Venta>.ErrorResult("Error interno del servidor", ex.Message);
        }
    }

    public async Task<ApiResponse<List<Venta>>> GetSalesBySellerAsync(string vendedorId, DateTime? fechaInicio = null, DateTime? fechaFin = null)
    {
        try
        {
            _logger.LogInformation("Getting sales for seller: {VendedorId}", vendedorId);

            var ventas = await _repository.GetSalesBySellerAsync(vendedorId, fechaInicio, fechaFin);

            return ApiResponse<List<Venta>>.SuccessResult(ventas, $"Se encontraron {ventas.Count} ventas");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting sales for seller: {VendedorId}", vendedorId);
            return ApiResponse<List<Venta>>.ErrorResult("Error interno del servidor", ex.Message);
        }
    }

    public async Task<ApiResponse<Venta>> UpdateSaleStatusAsync(string ventaId, string nuevoEstado)
    {
        try
        {
            _logger.LogInformation("Updating sale status: {VentaId} to {NuevoEstado}", ventaId, nuevoEstado);

            var venta = await _repository.GetSaleByIdAsync(ventaId);
            if (venta == null)
            {
                return ApiResponse<Venta>.ErrorResult("Venta no encontrada");
            }

            venta.Estado = nuevoEstado;
            venta.FechaActualizacion = DateTime.UtcNow;

            var updatedSale = await _repository.UpdateSaleAsync(venta);

            return ApiResponse<Venta>.SuccessResult(updatedSale, "Estado de venta actualizado exitosamente");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating sale status: {VentaId}", ventaId);
            return ApiResponse<Venta>.ErrorResult("Error interno del servidor", ex.Message);
        }
    }

    public async Task<ApiResponse<bool>> CancelSaleAsync(string ventaId, string motivo)
    {
        try
        {
            _logger.LogInformation("Cancelling sale: {VentaId}", ventaId);

            var venta = await _repository.GetSaleByIdAsync(ventaId);
            if (venta == null)
            {
                return ApiResponse<bool>.ErrorResult("Venta no encontrada");
            }

            if (venta.Estado == "Cancelada")
            {
                return ApiResponse<bool>.ErrorResult("La venta ya está cancelada");
            }

            // Restore inventory
            foreach (var detalle in venta.Detalles)
            {
                var stockUpdate = new StockUpdateRequest
                {
                    IdProducto = detalle.IdProducto,
                    TipoMovimiento = "Entrada",
                    Cantidad = detalle.Cantidad,
                    Motivo = $"Cancelación venta {ventaId} - {motivo}",
                    DocumentoReferencia = ventaId,
                    UsuarioResponsable = "Sistema"
                };

                await _inventoryComponent.UpdateStockAsync(stockUpdate);
            }

            venta.Estado = "Cancelada";
            venta.FechaActualizacion = DateTime.UtcNow;
            venta.Observaciones += $" | Cancelada: {motivo}";

            await _repository.UpdateSaleAsync(venta);

            return ApiResponse<bool>.SuccessResult(true, "Venta cancelada exitosamente");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling sale: {VentaId}", ventaId);
            return ApiResponse<bool>.ErrorResult("Error interno del servidor", ex.Message);
        }
    }
}

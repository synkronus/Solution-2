using Microsoft.Extensions.Logging;
using PoliMarket.Models.Common;
using PoliMarket.Models.Entities;
using PoliMarket.Components.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace PoliMarket.Components.Delivery;

/// <summary>
/// Delivery Component (RF4) - Component-Based Architecture
/// Handles delivery scheduling, tracking, and management operations
/// </summary>
public class EntregasComponent : IEntregasComponent
{
    private readonly PoliMarketDbContext _context;
    private readonly ILogger<EntregasComponent> _logger;

    public EntregasComponent(PoliMarketDbContext context, ILogger<EntregasComponent> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ApiResponse<List<Entrega>>> GetDeliveriesByVendorAsync(string vendorId)
    {
        try
        {
            _logger.LogInformation("Getting deliveries for vendor: {VendorId}", vendorId);

            var deliveries = await _context.Entregas
                .Include(e => e.Venta)
                .Include(e => e.Seguimientos)
                .Where(e => e.Venta != null && e.Venta.IdVendedor == vendorId)
                .OrderByDescending(e => e.FechaProgramada)
                .ToListAsync();

            return ApiResponse<List<Entrega>>.SuccessResult(deliveries, $"Se encontraron {deliveries.Count} entregas");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting deliveries for vendor: {VendorId}", vendorId);
            return ApiResponse<List<Entrega>>.ErrorResult("Error interno del servidor", ex.Message);
        }
    }

    public async Task<ApiResponse<List<Entrega>>> GetAllDeliveriesAsync()
    {
        try
        {
            _logger.LogInformation("Getting all deliveries");

            var deliveries = await _context.Entregas
                .Include(e => e.Venta)
                .Include(e => e.Seguimientos)
                .OrderByDescending(e => e.FechaProgramada)
                .ToListAsync();

            return ApiResponse<List<Entrega>>.SuccessResult(deliveries, $"Se encontraron {deliveries.Count} entregas");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all deliveries");
            return ApiResponse<List<Entrega>>.ErrorResult("Error interno del servidor", ex.Message);
        }
    }

    public async Task<ApiResponse<Entrega>> GetDeliveryByIdAsync(string deliveryId)
    {
        try
        {
            _logger.LogInformation("Getting delivery by ID: {DeliveryId}", deliveryId);

            var delivery = await _context.Entregas
                .Include(e => e.Venta)
                .Include(e => e.Seguimientos)
                .FirstOrDefaultAsync(e => e.Id == deliveryId);

            if (delivery == null)
            {
                return ApiResponse<Entrega>.ErrorResult("Entrega no encontrada");
            }

            return ApiResponse<Entrega>.SuccessResult(delivery, "Entrega encontrada");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting delivery by ID: {DeliveryId}", deliveryId);
            return ApiResponse<Entrega>.ErrorResult("Error interno del servidor", ex.Message);
        }
    }

    public async Task<ApiResponse<Entrega>> ScheduleDeliveryAsync(ScheduleDeliveryRequest request)
    {
        try
        {
            _logger.LogInformation("Scheduling delivery for sale: {SaleId}", request.IdVenta);

            // Validate that the sale exists
            var sale = await _context.Ventas.FirstOrDefaultAsync(v => v.Id == request.IdVenta);
            if (sale == null)
            {
                return ApiResponse<Entrega>.ErrorResult("Venta no encontrada");
            }

            var delivery = new Entrega
            {
                Id = Guid.NewGuid().ToString(),
                IdVenta = request.IdVenta,
                FechaProgramada = request.FechaProgramada,
                Direccion = request.Direccion,
                Cliente = request.Cliente,
                Estado = "Programada",
                Transportista = request.TransportistaId ?? "Sin asignar",
                Observaciones = request.Observaciones,
                FechaCreacion = DateTime.UtcNow,
                CreadoPor = sale.IdVendedor
            };

            _context.Entregas.Add(delivery);
            await _context.SaveChangesAsync();

            return ApiResponse<Entrega>.SuccessResult(delivery, "Entrega programada exitosamente");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scheduling delivery for sale: {SaleId}", request.IdVenta);
            return ApiResponse<Entrega>.ErrorResult("Error interno del servidor", ex.Message);
        }
    }

    public async Task<ApiResponse<Entrega>> UpdateDeliveryStatusAsync(UpdateDeliveryStatusRequest request)
    {
        try
        {
            _logger.LogInformation("Updating delivery status: {DeliveryId}", request.IdEntrega);

            var delivery = await _context.Entregas.FirstOrDefaultAsync(e => e.Id == request.IdEntrega);
            if (delivery == null)
            {
                return ApiResponse<Entrega>.ErrorResult("Entrega no encontrada");
            }

            delivery.Estado = request.NuevoEstado;
            delivery.FechaActualizacion = DateTime.UtcNow;
            
            if (request.NuevoEstado == "Entregada")
            {
                delivery.FechaEntrega = DateTime.UtcNow;
            }

            // Add tracking record
            var tracking = new SeguimientoEntrega
            {
                Id = Guid.NewGuid().ToString(),
                IdEntrega = delivery.Id,
                FechaHora = DateTime.UtcNow,
                Estado = request.NuevoEstado,
                Ubicacion = "Actualización de estado",
                Comentarios = request.Comentarios,
                Latitud = request.Latitud,
                Longitud = request.Longitud,
                CreadoPor = delivery.CreadoPor
            };

            _context.SeguimientosEntrega.Add(tracking);
            await _context.SaveChangesAsync();

            return ApiResponse<Entrega>.SuccessResult(delivery, "Estado de entrega actualizado");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating delivery status: {DeliveryId}", request.IdEntrega);
            return ApiResponse<Entrega>.ErrorResult("Error interno del servidor", ex.Message);
        }
    }

    public async Task<ApiResponse<List<Entrega>>> GetPendingDeliveriesAsync()
    {
        try
        {
            _logger.LogInformation("Getting pending deliveries");

            var pendingDeliveries = await _context.Entregas
                .Include(e => e.Venta)
                .Where(e => e.Estado == "Programada" || e.Estado == "En Transito")
                .OrderBy(e => e.FechaProgramada)
                .ToListAsync();

            return ApiResponse<List<Entrega>>.SuccessResult(pendingDeliveries, $"Se encontraron {pendingDeliveries.Count} entregas pendientes");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending deliveries");
            return ApiResponse<List<Entrega>>.ErrorResult("Error interno del servidor", ex.Message);
        }
    }

    public async Task<ApiResponse<bool>> ConfirmDeliveryAsync(string deliveryId)
    {
        try
        {
            _logger.LogInformation("Confirming delivery: {DeliveryId}", deliveryId);

            var delivery = await _context.Entregas.FirstOrDefaultAsync(e => e.Id == deliveryId);
            if (delivery == null)
            {
                return ApiResponse<bool>.ErrorResult("Entrega no encontrada");
            }

            delivery.Estado = "Entregada";
            delivery.FechaEntrega = DateTime.UtcNow;
            delivery.FechaActualizacion = DateTime.UtcNow;

            // Add confirmation tracking
            var tracking = new SeguimientoEntrega
            {
                Id = Guid.NewGuid().ToString(),
                IdEntrega = delivery.Id,
                FechaHora = DateTime.UtcNow,
                Estado = "Entregada",
                Ubicacion = "Entrega confirmada",
                Comentarios = "Entrega confirmada por el sistema",
                CreadoPor = delivery.CreadoPor
            };

            _context.SeguimientosEntrega.Add(tracking);
            await _context.SaveChangesAsync();

            return ApiResponse<bool>.SuccessResult(true, "Entrega confirmada exitosamente");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error confirming delivery: {DeliveryId}", deliveryId);
            return ApiResponse<bool>.ErrorResult("Error interno del servidor", ex.Message);
        }
    }

    public async Task<ApiResponse<List<SeguimientoEntrega>>> GetDeliveryTrackingAsync(string deliveryId)
    {
        try
        {
            _logger.LogInformation("Getting tracking for delivery: {DeliveryId}", deliveryId);

            var tracking = await _context.SeguimientosEntrega
                .Where(s => s.IdEntrega == deliveryId)
                .OrderByDescending(s => s.FechaHora)
                .ToListAsync();

            return ApiResponse<List<SeguimientoEntrega>>.SuccessResult(tracking, $"Se encontraron {tracking.Count} registros de seguimiento");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting delivery tracking: {DeliveryId}", deliveryId);
            return ApiResponse<List<SeguimientoEntrega>>.ErrorResult("Error interno del servidor", ex.Message);
        }
    }

    public async Task<ApiResponse<Entrega>> CreateDeliveryFromSaleAsync(string saleId)
    {
        try
        {
            _logger.LogInformation("Creating delivery from sale: {SaleId}", saleId);

            var sale = await _context.Ventas
                .Include(v => v.Cliente)
                .FirstOrDefaultAsync(v => v.Id == saleId);

            if (sale == null)
            {
                return ApiResponse<Entrega>.ErrorResult("Venta no encontrada");
            }

            // Check if delivery already exists
            var existingDelivery = await _context.Entregas.FirstOrDefaultAsync(e => e.IdVenta == saleId);
            if (existingDelivery != null)
            {
                return ApiResponse<Entrega>.SuccessResult(existingDelivery, "La entrega ya existe para esta venta");
            }

            var delivery = new Entrega
            {
                Id = Guid.NewGuid().ToString(),
                IdVenta = saleId,
                FechaProgramada = DateTime.UtcNow.AddDays(1), // Default to next day
                Direccion = sale.Cliente?.Direccion ?? "Dirección no especificada",
                Cliente = sale.Cliente?.Nombre ?? "Cliente no especificado",
                Estado = "Programada",
                Transportista = "Sin asignar",
                FechaCreacion = DateTime.UtcNow,
                CreadoPor = sale.IdVendedor
            };

            _context.Entregas.Add(delivery);
            await _context.SaveChangesAsync();

            return ApiResponse<Entrega>.SuccessResult(delivery, "Entrega creada automáticamente");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating delivery from sale: {SaleId}", saleId);
            return ApiResponse<Entrega>.ErrorResult("Error interno del servidor", ex.Message);
        }
    }
}

using Microsoft.Extensions.Logging;
using PoliMarket.Contracts;
using PoliMarket.Models.Common;
using PoliMarket.Models.Entities;

namespace PoliMarket.Components.Inventory;

/// <summary>
/// Inventory Component Implementation (RF3)
/// Provides inventory management and stock control services
/// Reusability Level: Very High (90%) - Universal inventory system
/// </summary>
public class InventarioComponent : IInventarioComponent
{
    private readonly ILogger<InventarioComponent> _logger;
    private static readonly Dictionary<string, int> _stockData = new()
    {
        // Products from DatabaseSeeder
        ["P001"] = 150,  // Arroz Diana Premium 500g
        ["P002"] = 80,   // Aceite Girasol 1L
        ["P003"] = 120,  // Leche Entera Alpina 1L
        ["P004"] = 60,   // Pan Tajado Bimbo 450g
        ["P005"] = 200,  // Coca Cola 2L
        ["P006"] = 90,   // Detergente Ariel 1kg
        ["P007"] = 180,  // Jabón Rey 300g
        ["P008"] = 75,   // Papel Higiénico Scott 4 rollos

        // Legacy products for compatibility
        ["PROD001"] = 50,
        ["PROD002"] = 200,
        ["PROD003"] = 75
    };

    private static readonly Dictionary<string, string> _productNames = new()
    {
        ["P001"] = "Arroz Diana Premium 500g",
        ["P002"] = "Aceite Girasol 1L",
        ["P003"] = "Leche Entera Alpina 1L",
        ["P004"] = "Pan Tajado Bimbo 450g",
        ["P005"] = "Coca Cola 2L",
        ["P006"] = "Detergente Ariel 1kg",
        ["P007"] = "Jabón Rey 300g",
        ["P008"] = "Papel Higiénico Scott 4 rollos",
        ["PROD001"] = "Laptop Dell Inspiron",
        ["PROD002"] = "Mouse Inalámbrico",
        ["PROD003"] = "Monitor Samsung 24\""
    };

    public InventarioComponent(ILogger<InventarioComponent> logger)
    {
        _logger = logger;
    }

    public async Task<ApiResponse<DisponibilidadInventario>> CheckAvailabilityAsync(AvailabilityCheckRequest request)
    {
        try
        {
            _logger.LogInformation("Checking availability for product: {ProductId}", request.IdProducto);

            await Task.Delay(10); // Simulate async operation

            var currentStock = _stockData.GetValueOrDefault(request.IdProducto, 0);
            var disponible = currentStock >= request.CantidadRequerida;

            var availability = new DisponibilidadInventario
            {
                IdProducto = request.IdProducto,
                NombreProducto = _productNames.GetValueOrDefault(request.IdProducto, $"Product {request.IdProducto}"),
                StockActual = currentStock,
                StockDisponible = currentStock,
                StockReservado = 0,
                DisponibleParaVenta = disponible,
                Estado = disponible ? "Disponible" : "Stock Insuficiente",
                FechaConsulta = DateTime.UtcNow
            };

            return ApiResponse<DisponibilidadInventario>.SuccessResult(availability, "Availability checked successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking availability for product: {ProductId}", request.IdProducto);
            return ApiResponse<DisponibilidadInventario>.ErrorResult("Error checking availability", ex.Message);
        }
    }

    public async Task<ApiResponse<StockOperationResponse>> UpdateStockAsync(StockUpdateRequest request)
    {
        try
        {
            _logger.LogInformation("Updating stock for product: {ProductId}", request.IdProducto);

            await Task.Delay(10); // Simulate async operation

            var currentStock = _stockData.GetValueOrDefault(request.IdProducto, 0);
            var newStock = request.TipoMovimiento.ToLower() switch
            {
                "entrada" => currentStock + request.Cantidad,
                "salida" => currentStock - request.Cantidad,
                "ajuste" => request.Cantidad,
                _ => currentStock
            };

            if (newStock < 0)
            {
                return ApiResponse<StockOperationResponse>.ErrorResult("Stock cannot be negative");
            }

            _stockData[request.IdProducto] = newStock;

            var response = new StockOperationResponse
            {
                Success = true,
                Message = "Stock updated successfully",
                StockAnterior = currentStock,
                StockNuevo = newStock,
                MovimientoId = Guid.NewGuid().ToString(),
                FechaOperacion = DateTime.UtcNow
            };

            return ApiResponse<StockOperationResponse>.SuccessResult(response, "Stock updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating stock for product: {ProductId}", request.IdProducto);
            return ApiResponse<StockOperationResponse>.ErrorResult("Error updating stock", ex.Message);
        }
    }

    public async Task<ApiResponse<DisponibilidadInventario>> GetCurrentStockAsync(string productoId)
    {
        try
        {
            _logger.LogInformation("Getting current stock for product: {ProductId}", productoId);

            await Task.Delay(10); // Simulate async operation

            var currentStock = _stockData.GetValueOrDefault(productoId, 0);

            var availability = new DisponibilidadInventario
            {
                IdProducto = productoId,
                NombreProducto = _productNames.GetValueOrDefault(productoId, $"Product {productoId}"),
                StockActual = currentStock,
                StockDisponible = currentStock,
                StockReservado = 0,
                DisponibleParaVenta = currentStock > 0,
                Estado = currentStock > 0 ? "Disponible" : "Agotado",
                FechaConsulta = DateTime.UtcNow
            };

            return ApiResponse<DisponibilidadInventario>.SuccessResult(availability, "Current stock retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current stock for product: {ProductId}", productoId);
            return ApiResponse<DisponibilidadInventario>.ErrorResult("Error getting current stock", ex.Message);
        }
    }

    public async Task<ApiResponse<List<MovimientoInventario>>> GetStockMovementsAsync(string productoId, DateTime? fechaInicio = null, DateTime? fechaFin = null)
    {
        try
        {
            _logger.LogInformation("Getting stock movements for product: {ProductId}", productoId);

            await Task.Delay(10); // Simulate async operation

            // Mock data - in real implementation this would come from database
            var movements = new List<MovimientoInventario>();

            return ApiResponse<List<MovimientoInventario>>.SuccessResult(movements, "Stock movements retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting stock movements for product: {ProductId}", productoId);
            return ApiResponse<List<MovimientoInventario>>.ErrorResult("Error getting stock movements", ex.Message);
        }
    }

    public async Task<ApiResponse<List<AlertaStock>>> GenerateStockAlertsAsync()
    {
        try
        {
            _logger.LogInformation("Generating stock alerts");

            await Task.Delay(10); // Simulate async operation

            var alerts = new List<AlertaStock>();

            foreach (var stock in _stockData)
            {
                if (stock.Value < 10) // Low stock threshold
                {
                    alerts.Add(new AlertaStock
                    {
                        IdProducto = stock.Key,
                        NombreProducto = $"Product {stock.Key}",
                        TipoAlerta = "StockBajo",
                        StockActual = stock.Value,
                        StockMinimo = 10,
                        Mensaje = $"Stock bajo para producto {stock.Key}: {stock.Value} unidades",
                        FechaAlerta = DateTime.UtcNow
                    });
                }
            }

            return ApiResponse<List<AlertaStock>>.SuccessResult(alerts, $"Generated {alerts.Count} stock alerts");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating stock alerts");
            return ApiResponse<List<AlertaStock>>.ErrorResult("Error generating stock alerts", ex.Message);
        }
    }

    public async Task<ApiResponse<bool>> ReserveStockAsync(string productoId, int cantidad, string ventaId)
    {
        try
        {
            _logger.LogInformation("Reserving stock for product: {ProductId}, quantity: {Cantidad}", productoId, cantidad);

            await Task.Delay(10); // Simulate async operation

            var currentStock = _stockData.GetValueOrDefault(productoId, 0);
            if (currentStock >= cantidad)
            {
                // In a real implementation, this would update reserved stock
                return ApiResponse<bool>.SuccessResult(true, "Stock reserved successfully");
            }

            return ApiResponse<bool>.ErrorResult("Insufficient stock for reservation");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reserving stock for product: {ProductId}", productoId);
            return ApiResponse<bool>.ErrorResult("Error reserving stock", ex.Message);
        }
    }

    public async Task<ApiResponse<bool>> ReleaseStockAsync(string productoId, int cantidad, string ventaId)
    {
        try
        {
            _logger.LogInformation("Releasing stock for product: {ProductId}, quantity: {Cantidad}", productoId, cantidad);

            await Task.Delay(10); // Simulate async operation

            // In a real implementation, this would update reserved stock
            return ApiResponse<bool>.SuccessResult(true, "Stock released successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error releasing stock for product: {ProductId}", productoId);
            return ApiResponse<bool>.ErrorResult("Error releasing stock", ex.Message);
        }
    }

    public async Task<ApiResponse<StockOperationResponse>> AdjustStockAsync(string productoId, int nuevoStock, string motivo, string usuarioResponsable)
    {
        try
        {
            _logger.LogInformation("Adjusting stock for product: {ProductId} to {NuevoStock}", productoId, nuevoStock);

            await Task.Delay(10); // Simulate async operation

            var currentStock = _stockData.GetValueOrDefault(productoId, 0);
            _stockData[productoId] = nuevoStock;

            var response = new StockOperationResponse
            {
                Success = true,
                Message = "Stock adjusted successfully",
                StockAnterior = currentStock,
                StockNuevo = nuevoStock,
                MovimientoId = Guid.NewGuid().ToString(),
                FechaOperacion = DateTime.UtcNow
            };

            return ApiResponse<StockOperationResponse>.SuccessResult(response, "Stock adjusted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adjusting stock for product: {ProductId}", productoId);
            return ApiResponse<StockOperationResponse>.ErrorResult("Error adjusting stock", ex.Message);
        }
    }
}

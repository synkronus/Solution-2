namespace PoliMarket.Models.Entities;

/// <summary>
/// Base class for entities that require audit tracking
/// </summary>
public abstract class BaseAuditEntity
{
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime? FechaActualizacion { get; set; }
    public string CreadoPor { get; set; } = string.Empty;
    public string? ActualizadoPor { get; set; }
}

/// <summary>
/// Interface for entities that support audit tracking
/// </summary>
public interface IAuditableEntity
{
    DateTime FechaCreacion { get; set; }
    DateTime? FechaActualizacion { get; set; }
    string CreadoPor { get; set; }
    string? ActualizadoPor { get; set; }
}

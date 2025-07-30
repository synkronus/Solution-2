using Microsoft.AspNetCore.Http;
using PoliMarket.Models.Entities;

namespace PoliMarket.Components.Infrastructure.Services;

/// <summary>
/// Service for handling audit information
/// </summary>
public interface IAuditService
{
    /// <summary>
    /// Get the current user ID from the HTTP context
    /// </summary>
    string GetCurrentUserId();
    
    /// <summary>
    /// Set audit information for entity creation
    /// </summary>
    void SetCreatedAudit(IAuditableEntity entity);
    
    /// <summary>
    /// Set audit information for entity update
    /// </summary>
    void SetUpdatedAudit(IAuditableEntity entity);
}

/// <summary>
/// Implementation of audit service
/// </summary>
public class AuditService : IAuditService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string GetCurrentUserId()
    {
        // Try to get user ID from JWT token claims
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.User?.Identity?.IsAuthenticated == true)
        {
            var userIdClaim = httpContext.User.FindFirst("sub") ?? 
                             httpContext.User.FindFirst("userId") ?? 
                             httpContext.User.FindFirst("id");
            
            if (userIdClaim != null)
            {
                return userIdClaim.Value;
            }
        }

        // Fallback: try to get from custom header (for API calls)
        var userIdHeader = httpContext?.Request.Headers["X-User-Id"].FirstOrDefault();
        if (!string.IsNullOrEmpty(userIdHeader))
        {
            return userIdHeader;
        }

        // Default fallback
        return "SYSTEM";
    }

    public void SetCreatedAudit(IAuditableEntity entity)
    {
        var currentUserId = GetCurrentUserId();
        entity.FechaCreacion = DateTime.UtcNow;
        entity.CreadoPor = currentUserId;
    }

    public void SetUpdatedAudit(IAuditableEntity entity)
    {
        var currentUserId = GetCurrentUserId();
        entity.FechaActualizacion = DateTime.UtcNow;
        entity.ActualizadoPor = currentUserId;
    }
}

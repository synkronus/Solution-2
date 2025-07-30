using PoliMarket.Models.Common;

namespace PoliMarket.Contracts;

/// <summary>
/// Interface for Integration Component
/// Provides central orchestration and communication services between components
/// Reusability Level: Very High (95%) - Universal integration pattern
/// </summary>
public interface IIntegracionComponent
{
    /// <summary>
    /// Orchestrates a complete business transaction across multiple components
    /// </summary>
    /// <param name="transactionRequest">Transaction request details</param>
    /// <returns>API response with transaction result</returns>
    Task<ApiResponse<TransactionResult>> ExecuteBusinessTransactionAsync(BusinessTransactionRequest transactionRequest);

    /// <summary>
    /// Validates system health and component availability
    /// </summary>
    /// <returns>API response with health check results</returns>
    Task<ApiResponse<SystemHealthResponse>> CheckSystemHealthAsync();

    /// <summary>
    /// Synchronizes data between components
    /// </summary>
    /// <param name="syncRequest">Synchronization request</param>
    /// <returns>API response with synchronization result</returns>
    Task<ApiResponse<SyncResult>> SynchronizeDataAsync(DataSyncRequest syncRequest);

    /// <summary>
    /// Publishes an event to all interested components
    /// </summary>
    /// <param name="eventData">Event data to publish</param>
    /// <returns>API response with publish result</returns>
    Task<ApiResponse<bool>> PublishEventAsync(ComponentEvent eventData);

    /// <summary>
    /// Gets system configuration for components
    /// </summary>
    /// <param name="componentName">Component name</param>
    /// <returns>API response with configuration data</returns>
    Task<ApiResponse<Dictionary<string, object>>> GetComponentConfigurationAsync(string componentName);
}

/// <summary>
/// Interface for Notifications Component
/// Provides notification and alerting services
/// Reusability Level: Very High (90%) - Standard notification system
/// </summary>
public interface INotificacionesComponent
{
    /// <summary>
    /// Sends a notification to specified recipients
    /// </summary>
    /// <param name="notification">Notification details</param>
    /// <returns>API response with send result</returns>
    Task<ApiResponse<NotificationResult>> SendNotificationAsync(NotificationRequest notification);

    /// <summary>
    /// Sends an alert based on system events
    /// </summary>
    /// <param name="alert">Alert details</param>
    /// <returns>API response with alert result</returns>
    Task<ApiResponse<bool>> SendAlertAsync(AlertRequest alert);

    /// <summary>
    /// Gets notification history for a recipient
    /// </summary>
    /// <param name="recipientId">Recipient identifier</param>
    /// <param name="startDate">Start date filter</param>
    /// <param name="endDate">End date filter</param>
    /// <returns>API response with notification history</returns>
    Task<ApiResponse<List<NotificationHistory>>> GetNotificationHistoryAsync(string recipientId, DateTime? startDate = null, DateTime? endDate = null);

    /// <summary>
    /// Configures notification preferences for a recipient
    /// </summary>
    /// <param name="preferences">Notification preferences</param>
    /// <returns>API response with configuration result</returns>
    Task<ApiResponse<bool>> ConfigureNotificationPreferencesAsync(NotificationPreferences preferences);
}

#region Supporting Models

/// <summary>
/// Request model for business transactions
/// </summary>
public class BusinessTransactionRequest
{
    public string TransactionId { get; set; } = Guid.NewGuid().ToString();
    public string TransactionType { get; set; } = string.Empty;
    public Dictionary<string, object> Parameters { get; set; } = new();
    public List<string> ComponentsInvolved { get; set; } = new();
    public string InitiatedBy { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Result model for business transactions
/// </summary>
public class TransactionResult
{
    public string TransactionId { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string Status { get; set; } = string.Empty;
    public Dictionary<string, object> Results { get; set; } = new();
    public List<string> Errors { get; set; } = new();
    public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
    public TimeSpan Duration { get; set; }
}

/// <summary>
/// System health response model
/// </summary>
public class SystemHealthResponse
{
    public bool IsHealthy { get; set; }
    public Dictionary<string, ComponentHealth> ComponentsHealth { get; set; } = new();
    public DateTime CheckedAt { get; set; } = DateTime.UtcNow;
    public string OverallStatus { get; set; } = string.Empty;
}

/// <summary>
/// Component health information
/// </summary>
public class ComponentHealth
{
    public string ComponentName { get; set; } = string.Empty;
    public bool IsHealthy { get; set; }
    public string Status { get; set; } = string.Empty;
    public TimeSpan ResponseTime { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime LastChecked { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Data synchronization request
/// </summary>
public class DataSyncRequest
{
    public string SourceComponent { get; set; } = string.Empty;
    public string TargetComponent { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public Dictionary<string, object> SyncParameters { get; set; } = new();
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Synchronization result
/// </summary>
public class SyncResult
{
    public bool Success { get; set; }
    public int RecordsProcessed { get; set; }
    public int RecordsUpdated { get; set; }
    public int RecordsCreated { get; set; }
    public int RecordsDeleted { get; set; }
    public List<string> Errors { get; set; } = new();
    public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Component event for pub/sub pattern
/// </summary>
public class ComponentEvent
{
    public string EventId { get; set; } = Guid.NewGuid().ToString();
    public string EventType { get; set; } = string.Empty;
    public string SourceComponent { get; set; } = string.Empty;
    public Dictionary<string, object> EventData { get; set; } = new();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? CorrelationId { get; set; }
}

/// <summary>
/// Notification request model
/// </summary>
public class NotificationRequest
{
    public string NotificationId { get; set; } = Guid.NewGuid().ToString();
    public string Type { get; set; } = string.Empty; // Email, SMS, Push, InApp
    public List<string> Recipients { get; set; } = new();
    public string Subject { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public Dictionary<string, object> Metadata { get; set; } = new();
    public DateTime ScheduledFor { get; set; } = DateTime.UtcNow;
    public string Priority { get; set; } = "Normal"; // Low, Normal, High, Critical
}

/// <summary>
/// Notification result
/// </summary>
public class NotificationResult
{
    public string NotificationId { get; set; } = string.Empty;
    public bool Success { get; set; }
    public List<string> SuccessfulRecipients { get; set; } = new();
    public List<string> FailedRecipients { get; set; } = new();
    public string Status { get; set; } = string.Empty;
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Alert request model
/// </summary>
public class AlertRequest
{
    public string AlertId { get; set; } = Guid.NewGuid().ToString();
    public string AlertType { get; set; } = string.Empty;
    public string Severity { get; set; } = "Medium"; // Low, Medium, High, Critical
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string SourceComponent { get; set; } = string.Empty;
    public Dictionary<string, object> AlertData { get; set; } = new();
    public List<string> NotificationChannels { get; set; } = new();
}

/// <summary>
/// Notification history model
/// </summary>
public class NotificationHistory
{
    public string NotificationId { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
    public DateTime? ReadAt { get; set; }
    public string Priority { get; set; } = string.Empty;
}

/// <summary>
/// Notification preferences model
/// </summary>
public class NotificationPreferences
{
    public string RecipientId { get; set; } = string.Empty;
    public Dictionary<string, bool> ChannelPreferences { get; set; } = new(); // Email: true, SMS: false, etc.
    public Dictionary<string, string> NotificationTypes { get; set; } = new(); // Sales: High, Inventory: Medium, etc.
    public bool EnableQuietHours { get; set; } = false;
    public TimeSpan QuietHoursStart { get; set; }
    public TimeSpan QuietHoursEnd { get; set; }
}

#endregion

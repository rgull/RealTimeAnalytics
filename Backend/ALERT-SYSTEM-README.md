# Real-Time Alerting System

This document explains the implementation of the real-time alerting system for the sensor tracking application.

## Overview

The alerting system provides:

- **Real-time alert triggering** based on sensor value thresholds
- **Alert storage** in the database with full history
- **Real-time notifications** via SignalR to connected clients
- **Alert management** with filtering, pagination, and resolution
- **Custom alert creation** via API

## Backend Implementation

### 1. Enhanced Sensor Model

The `Sensor` model now includes threshold configuration:

```csharp
public class Sensor
{
    // ... existing properties ...

    // Alert threshold configuration
    public double? WarningThreshold { get; set; }
    public double? CriticalThreshold { get; set; }
    public double? MinThreshold { get; set; }
    public bool AlertEnabled { get; set; } = true;
}
```

### 2. Alert Service

The `AlertService` handles alert logic:

```csharp
public interface IAlertService
{
    Task CheckSensorReadingForAlertsAsync(SensorReading reading);
    Task CreateAlertAsync(int sensorId, string message, string severity, double? thresholdValue = null, double? actualValue = null);
    Task ResolveAlertAsync(long alertId);
    Task<IEnumerable<Alert>> GetActiveAlertsAsync();
}
```

**Key Features:**

- Automatically checks sensor readings against thresholds
- Creates alerts with appropriate severity levels
- Sends real-time notifications via SignalR
- Supports custom alert creation

### 3. Alert Triggering Logic

Alerts are triggered when sensor values exceed configured thresholds:

```csharp
// High value alerts
if (sensor.CriticalThreshold.HasValue && reading.Value >= sensor.CriticalThreshold.Value)
{
    // Create Critical alert
}
else if (sensor.WarningThreshold.HasValue && reading.Value >= sensor.WarningThreshold.Value)
{
    // Create Warning alert
}

// Low value alerts
if (sensor.MinThreshold.HasValue && reading.Value <= sensor.MinThreshold.Value)
{
    // Create Warning alert for low values
}
```

### 4. API Endpoints

The `AlertsController` provides comprehensive alert management:

```csharp
[HttpGet]
public async Task<ActionResult<IEnumerable<Alert>>> GetAlerts(
    int page = 1,
    int pageSize = 50,
    string? severity = null,
    bool? isResolved = null)

[HttpPost]
public async Task<ActionResult<Alert>> CreateAlert([FromBody] CreateAlertRequest request)

[HttpPut("{id}/resolve")]
public async Task<IActionResult> ResolveAlert(long id)

[HttpGet("statistics")]
public async Task<ActionResult<object>> GetAlertStatistics()
```

## Frontend Implementation

### 1. Real-Time Alert Notifications

The `AlertNotification` component shows toast-style notifications:

```typescript
interface AlertNotificationProps {
  alert: Alert;
  onClose: () => void;
  autoHideDuration?: number;
}
```

**Features:**

- Expandable details with threshold and actual values
- Auto-hide with different durations based on severity
- Visual indicators for different alert types

### 2. Alert Manager

The `AlertManager` component manages multiple alert notifications:

```typescript
interface AlertManagerProps {
  alerts: Alert[];
  maxNotifications?: number;
}
```

**Features:**

- Limits number of simultaneous notifications
- Manages notification lifecycle
- Positioned as fixed overlay

### 3. Enhanced Alerts Panel

The `AlertsPanel` provides comprehensive alert management:

```typescript
interface AlertsPanelProps {
  alerts: Alert[];
  onRefresh?: () => void;
}
```

**Features:**

- **Recent Alerts Tab**: Shows real-time alerts
- **Alert History Tab**: Full alert history with filtering
- **Filtering**: By severity and resolution status
- **Pagination**: For large alert datasets
- **Custom Alert Creation**: Manual alert creation
- **Alert Resolution**: Mark alerts as resolved

## Usage Examples

### 1. Creating Custom Alerts

```typescript
// Frontend
await ApiService.createAlert({
  sensorId: 1,
  message: "Custom alert message",
  severity: "Warning",
  thresholdValue: 50,
  actualValue: 55,
});
```

### 2. Filtering Alerts

```typescript
// Get only unresolved critical alerts
const criticalAlerts = await ApiService.getAlerts(
  1, // page
  20, // pageSize
  "Critical", // severity
  false // isResolved
);
```

### 3. Real-Time Alert Handling

```typescript
// SignalR automatically receives new alerts
const { newAlerts } = useSignalR();

useEffect(() => {
  if (newAlerts.length > 0) {
    // Handle new alerts
    console.log("New alert received:", newAlerts[0]);
  }
}, [newAlerts]);
```

## Configuration

### Sensor Thresholds

Sensors are configured with thresholds during initialization:

```csharp
new Sensor {
    Name = "Temperature Sensor 1",
    Type = "Temperature",
    WarningThreshold = 35.0,
    CriticalThreshold = 40.0,
    MinThreshold = 15.0,
    AlertEnabled = true
}
```

### Alert Severity Levels

- **Info**: General information alerts
- **Warning**: Values approaching thresholds
- **Error**: Values exceeding warning thresholds
- **Critical**: Values exceeding critical thresholds

## Real-Time Features

### SignalR Integration

The system uses SignalR for real-time communication:

```csharp
// Send alert to all clients
await _hubContext.Clients.All.SendAsync("NewAlert", alert);

// Send to specific sensor subscribers
await _hubContext.Clients.Group($"sensor_{sensorId}").SendAsync("NewAlert", alert);
```

### Frontend SignalR Handling

```typescript
// Listen for new alerts
hubConnection.on("NewAlert", (alert: Alert) => {
  console.log("Received new alert:", alert);
  setNewAlerts((prev) => [alert, ...prev].slice(0, 50));
});
```

## Database Schema

### Alert Table

```sql
CREATE TABLE Alerts (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    SensorId INT NOT NULL,
    Message NVARCHAR(200) NOT NULL,
    Severity NVARCHAR(50) NOT NULL,
    ThresholdValue FLOAT NULL,
    ActualValue FLOAT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    IsResolved BIT NOT NULL DEFAULT 0,
    ResolvedAt DATETIME2 NULL,
    FOREIGN KEY (SensorId) REFERENCES Sensors(Id)
);
```

### Sensor Table (Enhanced)

```sql
ALTER TABLE Sensors ADD
    WarningThreshold FLOAT NULL,
    CriticalThreshold FLOAT NULL,
    MinThreshold FLOAT NULL,
    AlertEnabled BIT NOT NULL DEFAULT 1;
```

## Testing the System

### 1. Start the Application

```bash
# Backend
cd Backend
dotnet run

# Frontend
cd Frontend
npm start
```

### 2. Monitor Alerts

- Open the dashboard at `http://localhost:3000`
- Watch for real-time alert notifications
- Check the "Alert History" tab for stored alerts
- Use the "Create Custom Alert" button to test manual alerts

### 3. Simulate Alert Conditions

The sensor simulation service automatically generates values that can trigger alerts:

- 10% chance of generating alert-triggering values
- Values are generated based on sensor type and thresholds
- Alerts are created and sent in real-time

## Best Practices

### 1. Threshold Configuration

- Set realistic thresholds based on sensor capabilities
- Use warning thresholds to catch issues early
- Configure critical thresholds for immediate attention

### 2. Alert Management

- Regularly review and resolve alerts
- Use filtering to focus on specific alert types
- Monitor alert statistics for system health

### 3. Performance Considerations

- Limit the number of simultaneous notifications
- Use pagination for large alert datasets
- Consider alert retention policies for database cleanup

## Troubleshooting

### Common Issues

1. **Alerts not triggering**: Check sensor threshold configuration
2. **Real-time notifications not working**: Verify SignalR connection
3. **Database errors**: Ensure proper database schema migration

### Debug Information

- Check browser console for SignalR connection status
- Monitor backend logs for alert creation
- Use browser developer tools to inspect API calls

## Future Enhancements

### Potential Improvements

1. **Alert Rules Engine**: More complex alert conditions
2. **Alert Escalation**: Automatic escalation based on time
3. **Alert Templates**: Predefined alert messages
4. **Email/SMS Notifications**: External notification channels
5. **Alert Analytics**: Advanced reporting and trends
6. **Machine Learning**: Predictive alerting based on patterns

This alerting system provides a robust foundation for real-time monitoring and alerting in IoT sensor applications.

# Real-Time Sensor Data Tracking System

A comprehensive full-stack system that simulates and displays real-time sensor data with advanced features including anomaly detection, auto-purge mechanisms, and a modern web dashboard.

## 🚀 Features

### Backend (.NET Core API)
- **High-Performance Data Simulation**: Generates 1000 sensor readings per second
- **Efficient In-Memory Storage**: Handles up to 100,000 data points without performance degradation
- **Auto-Purge Mechanism**: Automatically deletes data older than 24 hours
- **Real-Time Communication**: WebSocket/SignalR for scalable real-time updates
- **Anomaly Detection**: Advanced statistical analysis to detect unusual sensor readings
- **Background Services**: Automated data processing and cleanup

### Database (SQL Server)
- **SensorReadings**: Stores raw sensor data with optimized indexing
- **Sensors**: Metadata about each sensor device
- **Alerts**: Logs anomaly events for monitoring and review
- **SensorStatistics**: Pre-calculated statistics for performance optimization

### Frontend Dashboard
- **Real-Time Visualization**: Live charts showing sensor data
- **Interactive Interface**: Modern, responsive design
- **Alert Management**: Real-time alert notifications
- **Performance Metrics**: Live statistics and monitoring

## 🛠️ Technical Stack

- **Backend**: .NET 8, ASP.NET Core Web API
- **Database**: SQL Server with Entity Framework Core
- **Real-Time**: SignalR for WebSocket communication
- **Frontend**: HTML5, CSS3, JavaScript, Chart.js
- **Background Services**: .NET Hosted Services
- **Data Processing**: In-memory caching with concurrent collections

## 📋 Prerequisites

- .NET 8 SDK
- SQL Server (LocalDB or full instance)
- Visual Studio 2022 or VS Code
- Modern web browser

## 🚀 Getting Started

### 1. Clone and Setup
```bash
git clone <repository-url>
cd RealTimeSensorTrack
```

### 2. Update Connection String
Update the connection string in `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=RealTimeSensorTrackDb;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}
```

### 3. Install Dependencies
```bash
dotnet restore
```

### 4. Run the Application
```bash
dotnet run
```

### 5. Access the Dashboard
Open your browser and navigate to:
- **Dashboard**: `https://localhost:7000` or `http://localhost:5000`
- **API Documentation**: `https://localhost:7000/swagger`

## 📊 API Endpoints

### Sensors
- `GET /api/sensors` - Get all active sensors
- `GET /api/sensors/{id}` - Get specific sensor
- `POST /api/sensors` - Create new sensor
- `PUT /api/sensors/{id}` - Update sensor
- `DELETE /api/sensors/{id}` - Deactivate sensor
- `GET /api/sensors/{id}/readings` - Get recent readings for sensor
- `GET /api/sensors/{id}/statistics` - Get sensor statistics

### Sensor Readings
- `GET /api/sensorreadings` - Get recent readings
- `GET /api/sensorreadings/by-sensor/{sensorId}` - Get readings by sensor
- `GET /api/sensorreadings/by-time-range` - Get readings by time range
- `GET /api/sensorreadings/count` - Get total reading count
- `POST /api/sensorreadings` - Add new reading

### Alerts
- `GET /api/alerts` - Get alerts with filtering
- `GET /api/alerts/{id}` - Get specific alert
- `PUT /api/alerts/{id}/resolve` - Resolve alert
- `DELETE /api/alerts/{id}` - Delete alert
- `GET /api/alerts/statistics` - Get alert statistics

## ⚙️ Configuration

### Sensor Settings
```json
{
  "SensorSettings": {
    "MaxInMemoryRecords": 100000,
    "DataRetentionHours": 24,
    "SimulationRatePerSecond": 1000,
    "AnomalyThreshold": 2.0
  }
}
```

### Performance Tuning
- **MaxInMemoryRecords**: Maximum number of readings to keep in memory
- **DataRetentionHours**: Hours to retain data before auto-purge
- **SimulationRatePerSecond**: Rate of simulated sensor readings
- **AnomalyThreshold**: Z-score threshold for anomaly detection

## 🔧 Architecture

### Data Flow
1. **Sensor Simulation Service** generates readings at 1000/second
2. **In-Memory Data Service** stores recent readings efficiently
3. **Anomaly Detection Service** analyzes readings for anomalies
4. **SignalR Hub** broadcasts real-time updates to clients
5. **Background Purge Service** cleans old data every hour
6. **Database** persists readings and metadata

### Key Components
- **InMemoryDataService**: Thread-safe concurrent collections for high-performance data storage
- **SensorSimulationService**: Background service generating realistic sensor data
- **AnomalyDetectionService**: Statistical analysis using Z-score for anomaly detection
- **DataPurgeService**: Automated cleanup of old data
- **SensorHub**: SignalR hub for real-time communication

## 📈 Performance Features

- **Concurrent Data Structures**: Thread-safe collections for high-throughput data
- **Batch Processing**: Efficient database operations
- **Memory Management**: Automatic cleanup of old data
- **Real-Time Updates**: WebSocket communication for instant updates
- **Statistical Analysis**: Pre-calculated statistics for fast queries

## 🚨 Monitoring and Alerts

The system includes comprehensive monitoring:
- Real-time performance metrics
- Anomaly detection with configurable thresholds
- Alert severity levels (Info, Warning, Error, Critical)
- Historical alert tracking
- Sensor health monitoring

## 🔒 Security Considerations

- CORS enabled for development
- Input validation on all endpoints
- SQL injection protection via Entity Framework
- Secure SignalR connections

## 🧪 Testing

The system includes:
- Unit tests for core services
- Integration tests for API endpoints
- Performance tests for high-load scenarios
- Real-time communication tests

## 📝 License

This project is licensed under the MIT License.

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests if applicable
5. Submit a pull request

## 📞 Support

For questions or issues, please create an issue in the repository or contact the development team.

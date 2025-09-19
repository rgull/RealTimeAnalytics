# Real-Time Sensor Analytics - Backend

## Overview

A robust ASP.NET Core backend system for real-time sensor data collection, processing, and alert management. This system provides comprehensive APIs for sensor management, real-time data streaming via SignalR, and intelligent alert generation based on configurable thresholds.

## Features

### Core Functionality

- **Real-time Sensor Data Collection**: Continuous simulation and processing of sensor readings
- **SignalR Integration**: Real-time data streaming to connected clients
- **Intelligent Alert System**: Automated alert generation based on sensor thresholds
- **Database Management**: Entity Framework Core with SQL Server integration
- **Background Services**: Continuous sensor simulation and data processing

### Sensor Management

- **Multi-sensor Support**: Temperature, Humidity, Pressure, Light, Motion, and Sound sensors
- **Configurable Thresholds**: Warning, Critical, and Minimum threshold settings per sensor
- **Alert Configuration**: Enable/disable alerts per sensor
- **Real-time Statistics**: Live sensor performance metrics

### Alert System

- **Threshold-based Alerts**: Automatic alerts when sensor values exceed configured limits
- **Severity Levels**: Info, Warning, Error, and Critical alert classifications
- **Alert Management**: Create, resolve, and track alert status
- **Real-time Notifications**: Instant alert delivery via SignalR
- **Alert History**: Comprehensive alert logging and retrieval

### API Endpoints

- **Sensor Management**: CRUD operations for sensor configuration
- **Data Retrieval**: Historical sensor readings and statistics
- **Alert Management**: Alert creation, resolution, and history
- **Real-time Status**: System health and connection monitoring

## Technology Stack

- **Framework**: ASP.NET Core 8.0
- **Database**: SQL Server with Entity Framework Core
- **Real-time Communication**: SignalR
- **Background Processing**: Hosted Services
- **Logging**: Built-in ASP.NET Core logging
- **Configuration**: JSON-based configuration management

## Architecture

### Service Layer

- **SensorSimulationService**: Background service for continuous sensor data generation
- **AlertService**: Intelligent alert processing and notification management
- **InMemoryDataService**: High-performance in-memory data caching
- **DatabaseSeeder**: Automated database initialization and sample data

### Data Models

- **Sensor**: Core sensor configuration with threshold settings
- **SensorReading**: Individual sensor data points with metadata
- **Alert**: Alert records with severity and resolution tracking
- **SensorStatistic**: Aggregated sensor performance metrics

### Real-time Features

- **SignalR Hub**: Centralized real-time communication hub
- **Client Grouping**: Sensor-specific subscription management
- **Automatic Reconnection**: Robust connection handling
- **Data Broadcasting**: Efficient multi-client data distribution

## Configuration

### Database Connection

Configure SQL Server connection string in `appsettings.json`:

- Development and production database settings
- Connection pooling and timeout configuration
- Migration and seeding options

### Sensor Simulation

- **Simulation Rate**: Configurable readings per second
- **Batch Processing**: Optimized data generation and storage
- **Update Intervals**: Customizable data refresh rates
- **Alert Probability**: Configurable alert trigger frequency

### SignalR Settings

- **Transport Configuration**: WebSocket and fallback options
- **Connection Management**: Automatic reconnection policies
- **Client Grouping**: Sensor-specific subscription handling
- **Message Broadcasting**: Efficient multi-client communication

## Database Schema

### Core Tables

- **Sensors**: Sensor configuration and threshold settings
- **SensorReadings**: Historical sensor data points
- **Alerts**: Alert records and resolution tracking
- **SensorStatistics**: Aggregated performance metrics

### Relationships

- One-to-many relationships between sensors and readings
- One-to-many relationships between sensors and alerts
- Foreign key constraints for data integrity
- Indexed columns for optimal query performance

## Performance Features

### Data Processing

- **Batch Operations**: Efficient bulk data processing
- **In-memory Caching**: High-performance data access
- **Background Processing**: Non-blocking data generation
- **Optimized Queries**: Efficient database operations

### Real-time Communication

- **Connection Pooling**: Optimized SignalR connections
- **Message Batching**: Efficient data broadcasting
- **Client Management**: Automatic connection handling
- **Error Recovery**: Robust error handling and reconnection

## Security Features

### API Security

- **CORS Configuration**: Cross-origin request handling
- **Input Validation**: Comprehensive data validation
- **Error Handling**: Secure error message management
- **Logging**: Comprehensive audit trail

### Data Protection

- **SQL Injection Prevention**: Parameterized queries
- **Data Validation**: Input sanitization and validation
- **Error Logging**: Secure error handling
- **Connection Security**: Encrypted database connections

## Monitoring and Logging

### System Monitoring

- **Health Checks**: Application and database health monitoring
- **Performance Metrics**: Real-time system performance tracking
- **Connection Status**: SignalR connection monitoring
- **Alert Statistics**: Comprehensive alert analytics

### Logging

- **Structured Logging**: JSON-formatted log entries
- **Log Levels**: Configurable logging verbosity
- **Performance Tracking**: Request and operation timing
- **Error Tracking**: Comprehensive error logging and analysis

## Deployment

### Prerequisites

- .NET 8.0 Runtime
- SQL Server 2019 or later
- Windows Server or Linux environment
- IIS or Kestrel web server

### Configuration

- Database connection string configuration
- SignalR transport settings
- Logging configuration
- Performance tuning parameters

### Production Considerations

- **Database Optimization**: Indexing and query optimization
- **Connection Pooling**: Database connection management
- **Caching Strategy**: In-memory and distributed caching
- **Monitoring**: Application performance monitoring

## Maintenance

### Database Maintenance

- **Data Purging**: Automated old data cleanup
- **Index Optimization**: Regular index maintenance
- **Backup Strategy**: Automated database backups
- **Migration Management**: Schema update procedures

### System Maintenance

- **Log Rotation**: Automated log file management
- **Performance Monitoring**: Continuous system monitoring
- **Alert Management**: Alert cleanup and resolution
- **Update Procedures**: Application update processes

## Troubleshooting

### Common Issues

- **Database Connection**: Connection string and network issues
- **SignalR Connectivity**: WebSocket and transport problems
- **Performance Issues**: Database and memory optimization
- **Alert Generation**: Threshold and configuration problems

### Diagnostic Tools

- **Log Analysis**: Comprehensive log file analysis
- **Performance Counters**: System performance monitoring
- **Database Profiling**: Query performance analysis
- **Network Diagnostics**: Connection and communication testing

## Support

For technical support and documentation:

- Review application logs for error details
- Check database connectivity and configuration
- Verify SignalR connection settings
- Monitor system performance metrics

## Version Information

- **Framework**: ASP.NET Core 8.0
- **Database**: Entity Framework Core 8.0
- **SignalR**: ASP.NET Core SignalR
- **Target Platform**: .NET 8.0

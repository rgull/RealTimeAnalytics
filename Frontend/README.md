# Real-Time Sensor Analytics - Frontend

## Overview

A modern React-based dashboard for real-time sensor data visualization and alert management. Built with Material-UI components and SignalR integration.

## Features

### Dashboard Components

- Real-time data visualization with interactive charts
- Comprehensive alert monitoring and resolution
- Detailed performance metrics and analytics
- Real-time system status and connectivity monitoring
- Responsive design for desktop and mobile devices

### Data Visualization

- Live line charts for sensor data trends
- Pie charts for sensor type distribution
- Bar charts for alert severity analysis
- Interactive graphs with zoom and pan capabilities
- Historical data with configurable time ranges

### Alert System

- Real-time notifications via SignalR
- Comprehensive alert log with filtering
- Visual alert classification and prioritization
- Manual alert resolution and status tracking
- Custom alert creation and testing

## Technology Stack

- **Framework**: React 18 with TypeScript
- **UI Library**: Material-UI (MUI) v5
- **Charts**: MUI X Charts
- **Real-time Communication**: SignalR client
- **HTTP Client**: Axios
- **Build Tool**: Vite
- **Styling**: Material-UI theming

## Architecture

### Component Structure

- **Dashboard**: Main application container
- **SensorCard**: Individual sensor display
- **AlertsPanel**: Alert management interface
- **StatisticsPanel**: Performance metrics
- **ConnectionStatus**: Connectivity monitoring

### State Management

- React Hooks for local state
- SignalR Context for global real-time data
- API Service for centralized communication
- In-memory data caching

### Real-time Features

- Automatic SignalR connection and reconnection
- Real-time sensor data updates
- Instant alert delivery and display
- Automatic connection status tracking

## User Interface

### Dashboard Layout

- Header with application title and connection status
- Statistics cards showing key metrics
- Main content area with charts and visualization
- Sidebar with alert management
- Sensor grid with individual status cards

### Navigation

- Tabbed interface for organized content
- Filter controls for data filtering
- Pagination for efficient data browsing
- Responsive mobile navigation

### Visual Design

- Consistent Material-UI components
- Color-coded alert severity indicators
- Interactive hover effects
- Responsive layout for all screen sizes

## Data Management

### Real-time Data Flow

- Automatic SignalR data streaming
- Efficient data transformation and caching
- React state management for UI updates
- Optimized data storage and cleanup

### API Integration

- RESTful API communication
- Comprehensive error handling
- Input validation and sanitization
- Efficient data caching and retrieval

## Configuration

### Development Setup

- Node.js 18 or later
- npm or yarn package manager
- Vite development server
- Automatic code reloading

### Build Configuration

- TypeScript strict type checking
- Vite optimized bundling
- ESLint code quality enforcement
- Optimized production builds

## Performance

### Optimization Features

- Dynamic component loading
- Bundle size monitoring
- Efficient data and asset caching
- On-demand resource loading

### Monitoring

- Real-time performance tracking
- Comprehensive error logging
- Usage analytics and behavior tracking
- Application health monitoring

## Security

### Data Protection

- Client-side input validation
- Cross-site scripting protection
- Cross-site request forgery prevention
- HTTPS and secure WebSocket connections

## Development

### Code Quality

- TypeScript strict type checking
- ESLint code quality enforcement
- Prettier code formatting

### Testing

- Component and function testing
- API and SignalR testing
- End-to-end user flow testing
- Load and performance testing

## Troubleshooting

### Common Issues

- SignalR connection problems
- Backend API communication errors
- Performance and rendering issues
- Cross-browser compatibility

### Debug Tools

- Browser developer tools
- React DevTools
- Network monitoring

## Version Information

- React: 18.2.0
- TypeScript: 5.2.2
- Material-UI: 5.15.0
- SignalR: 8.0.0
- Vite: 5.0.8

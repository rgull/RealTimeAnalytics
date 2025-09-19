# Real-Time Sensor Dashboard Frontend

A modern React frontend built with Vite, MUI X charts, and SignalR for real-time sensor data visualization.

## 🚀 Features

### Real-Time Dashboard
- **Live Charts**: Real-time line charts using MUI X charts
- **Sensor Cards**: Individual sensor monitoring with trend indicators
- **Statistics Panel**: Aggregated statistics (min, max, average, standard deviation)
- **Alert System**: Real-time alert notifications with severity levels

### Performance & Scalability
- **SignalR Integration**: WebSocket connection for real-time updates
- **Optimized Rendering**: React.memo and useMemo for performance
- **Efficient Data Management**: Automatic data cleanup and pagination
- **Responsive Design**: Mobile-first design with Material-UI

### Charts & Visualizations
- **Line Charts**: Live sensor data trends
- **Pie Charts**: Sensor type distribution
- **Bar Charts**: Alert severity distribution
- **Data Grids**: Tabular data display with sorting and filtering

## 🛠️ Tech Stack

- **React 18** with TypeScript
- **Vite** for fast development and building
- **Material-UI (MUI)** for components and theming
- **MUI X Charts** for advanced data visualization
- **SignalR** for real-time communication
- **Axios** for API communication

## 📦 Installation

1. **Navigate to Frontend directory:**
   ```bash
   cd Frontend
   ```

2. **Install dependencies:**
   ```bash
   npm install
   ```

3. **Start development server:**
   ```bash
   npm run dev
   ```

4. **Build for production:**
   ```bash
   npm run build
   ```

## 🔧 Configuration

### Backend Integration
The frontend is configured to connect to the .NET backend:

- **API Base URL**: `http://localhost:5025/api`
- **SignalR Hub**: `http://localhost:5025/sensorHub`
- **Development Port**: `3000`

### Environment Variables
Create a `.env` file for custom configuration:

```env
VITE_API_BASE_URL=http://localhost:5025/api
VITE_SIGNALR_HUB_URL=http://localhost:5025/sensorHub
VITE_APP_TITLE=Real-Time Sensor Dashboard
```

## 📊 Components

### Dashboard
Main dashboard component with:
- Real-time sensor data visualization
- Statistics overview
- Alert management
- Connection status

### SensorCard
Individual sensor monitoring with:
- Current value display
- Trend indicators
- Statistics summary
- Status indicators

### StatisticsPanel
Aggregated statistics display:
- Min/Max/Average values
- Standard deviation
- Stability indicators
- Data quality metrics

### AlertsPanel
Alert management system:
- Real-time alert notifications
- Severity-based filtering
- Alert resolution
- Historical alert viewing

## 🔄 Real-Time Features

### SignalR Integration
- **Automatic Reconnection**: Handles connection drops gracefully
- **Real-Time Updates**: Live sensor data and alerts
- **Connection Status**: Visual connection indicator
- **Error Handling**: Robust error handling and recovery

### Data Management
- **Efficient Updates**: Only re-renders when necessary
- **Memory Management**: Automatic cleanup of old data
- **Performance Optimization**: Debounced updates and memoization

## 🎨 UI/UX Features

### Material-UI Theming
- **Consistent Design**: Material Design principles
- **Dark/Light Mode**: Theme switching support
- **Responsive Layout**: Mobile-first design
- **Accessibility**: WCAG compliance

### Real-Time Indicators
- **Connection Status**: Visual connection indicator
- **Data Rate**: Live data throughput display
- **Trend Indicators**: Visual trend arrows
- **Severity Colors**: Color-coded alerts and status

## 🚀 Performance Optimizations

### React Optimizations
- **React.memo**: Prevents unnecessary re-renders
- **useMemo**: Memoizes expensive calculations
- **useCallback**: Memoizes event handlers
- **Lazy Loading**: Code splitting for better performance

### Data Optimizations
- **Pagination**: Efficient data loading
- **Debouncing**: Reduces API calls
- **Caching**: Intelligent data caching
- **Cleanup**: Automatic memory management

## 🔧 Development

### Available Scripts
- `npm run dev` - Start development server
- `npm run build` - Build for production
- `npm run preview` - Preview production build
- `npm run lint` - Run ESLint

### Code Structure
```
src/
├── components/          # React components
│   ├── Dashboard.tsx   # Main dashboard
│   ├── SensorCard.tsx  # Individual sensor card
│   ├── StatisticsPanel.tsx # Statistics display
│   ├── AlertsPanel.tsx # Alert management
│   └── ConnectionStatus.tsx # Connection indicator
├── services/           # API and SignalR services
│   ├── api.ts         # Backend API client
│   └── SignalRService.tsx # Real-time communication
├── App.tsx            # Main app component
├── main.tsx           # App entry point
└── index.css          # Global styles
```

## 🌐 Browser Support

- **Chrome** 90+
- **Firefox** 88+
- **Safari** 14+
- **Edge** 90+

## 📱 Mobile Support

- **Responsive Design**: Mobile-first approach
- **Touch Gestures**: Swipe and tap support
- **Performance**: Optimized for mobile devices
- **Offline Support**: Graceful degradation

## 🔒 Security

- **CORS Configuration**: Proper cross-origin setup
- **Input Validation**: Client-side validation
- **Error Handling**: Secure error messages
- **HTTPS Support**: Secure communication

## 🧪 Testing

### Manual Testing
1. **Real-Time Updates**: Verify live data updates
2. **Connection Handling**: Test connection drops
3. **Performance**: Monitor under high data load
4. **Responsiveness**: Test on different screen sizes

### Performance Testing
- **Load Testing**: High data volume scenarios
- **Memory Usage**: Monitor memory consumption
- **Rendering Performance**: Measure render times
- **Network Efficiency**: Optimize data transfer

## 🚀 Deployment

### Production Build
```bash
npm run build
```

### Environment Configuration
Update `vite.config.ts` for production URLs:

```typescript
export default defineConfig({
  // ... other config
  server: {
    proxy: {
      '/api': {
        target: 'https://your-api-domain.com',
        changeOrigin: true,
      },
    }
  }
})
```

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests if applicable
5. Submit a pull request

## 📄 License

This project is licensed under the MIT License.


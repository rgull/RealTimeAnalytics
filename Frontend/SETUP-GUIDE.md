# Frontend Setup Guide

## 🚀 Quick Start

### Prerequisites
- **Node.js 18+** (Required for Vite compatibility)
- **npm** (comes with Node.js)

### Installation Steps

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

4. **Access the dashboard:**
   - Open `http://localhost:3000` in your browser

## 🔧 Troubleshooting

### Node.js Version Issues

If you encounter the `crypto$2.getRandomValues is not a function` error:

1. **Check Node.js version:**
   ```bash
   node --version
   ```

2. **Update to Node.js 18+ if needed:**
   - Download from: https://nodejs.org/
   - Or use a version manager like nvm

3. **Clear npm cache:**
   ```bash
   npm cache clean --force
   ```

4. **Delete node_modules and reinstall:**
   ```bash
   rm -rf node_modules package-lock.json
   npm install
   ```

### API Connection Issues

If you see 307 redirect errors in the browser:

1. **Make sure backend is running:**
   ```bash
   # In the main project directory
   dotnet run
   ```

2. **Check backend is accessible:**
   ```bash
   # Test API directly
   curl http://localhost:5025/api/sensors
   ```

3. **Verify CORS configuration:**
   - Backend should allow `http://localhost:3000`
   - Check browser console for CORS errors

### Port Conflicts

If port 3000 is already in use:

1. **Find what's using the port:**
   ```bash
   netstat -ano | findstr :3000
   ```

2. **Kill the process:**
   ```bash
   taskkill /PID <process_id> /F
   ```

3. **Or use a different port:**
   ```bash
   npm run dev -- --port 3001
   ```

## 🎯 Development Workflow

### Making Changes
1. **Edit components** in `src/components/`
2. **Update API calls** in `src/services/api.ts`
3. **Modify SignalR** in `src/services/SignalRService.tsx`
4. **Hot reload** will automatically update the browser

### Building for Production
```bash
npm run build
```

### Preview Production Build
```bash
npm run preview
```

## 📁 Project Structure

```
Frontend/
├── src/
│   ├── components/          # React components
│   │   ├── Dashboard.tsx   # Main dashboard
│   │   ├── SensorCard.tsx  # Individual sensor card
│   │   ├── StatisticsPanel.tsx # Statistics display
│   │   ├── AlertsPanel.tsx # Alert management
│   │   └── ConnectionStatus.tsx # Connection indicator
│   ├── services/           # API and SignalR services
│   │   ├── api.ts         # Backend API client
│   │   └── SignalRService.tsx # Real-time communication
│   ├── App.tsx            # Main app component
│   ├── main.tsx           # App entry point
│   └── index.css          # Global styles
├── package.json           # Dependencies and scripts
├── vite.config.ts         # Vite configuration
├── tsconfig.json          # TypeScript configuration
└── index.html             # HTML template
```

## 🔗 API Integration

### Backend Connection
- **API Base URL**: `http://localhost:5025/api`
- **SignalR Hub**: `http://localhost:5025/sensorHub`
- **CORS**: Configured for `http://localhost:3000`

### Available Endpoints
- `GET /api/sensors` - Get all sensors
- `GET /api/sensorreadings` - Get sensor readings
- `GET /api/alerts` - Get alerts
- `GET /api/sensors/{id}/statistics` - Get sensor statistics

## 🎨 UI Components

### MUI X Charts
- **LineChart**: Real-time sensor data
- **PieChart**: Sensor type distribution
- **BarChart**: Alert severity distribution

### Material-UI Components
- **Cards**: Sensor information display
- **Tables**: Statistics and data grids
- **Alerts**: Notification system
- **Chips**: Status indicators

## 🚀 Performance Features

### Real-Time Updates
- **SignalR WebSocket** connection
- **Automatic reconnection** on connection loss
- **Live data streaming** from backend

### Optimization
- **React.memo** for component optimization
- **useMemo** for expensive calculations
- **useCallback** for event handlers
- **Automatic cleanup** of old data

## 🐛 Common Issues

### 1. Node.js Version Error
**Error**: `crypto$2.getRandomValues is not a function`
**Solution**: Update to Node.js 18+

### 2. API Connection Failed
**Error**: 307 Temporary Redirect
**Solution**: Ensure backend is running and HTTPS redirection is disabled in development

### 3. CORS Error
**Error**: Access blocked by CORS policy
**Solution**: Check backend CORS configuration allows `http://localhost:3000`

### 4. SignalR Connection Failed
**Error**: SignalR connection error
**Solution**: Ensure backend is running and SignalR hub is accessible

## 📞 Support

If you encounter issues:

1. **Check the browser console** for error messages
2. **Verify backend is running** on port 5025
3. **Check Node.js version** is 18+
4. **Clear npm cache** and reinstall dependencies
5. **Restart both frontend and backend** servers

## 🎉 Success Indicators

When everything is working correctly, you should see:

- ✅ **Dashboard loads** at `http://localhost:3000`
- ✅ **Real-time charts** updating with data
- ✅ **Sensor cards** showing live values
- ✅ **Connection status** shows "Connected"
- ✅ **No console errors** in browser dev tools
- ✅ **API calls succeed** in Network tab

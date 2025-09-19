import React, { useState, useEffect, useCallback } from 'react';
import {
  Box,
  Grid,
  Paper,
  Typography,
  Card,
  CardContent,
  Chip,
  Alert as MuiAlert,
  Snackbar,
  CircularProgress,
  Fade,
} from '@mui/material';
import { LineChart } from '@mui/x-charts/LineChart';
import { PieChart } from '@mui/x-charts/PieChart';
import { BarChart } from '@mui/x-charts/BarChart';
import { DataGrid, GridColDef } from '@mui/x-data-grid';
import { useSignalR } from '../services/SignalRService';
import { ApiService, Sensor, SensorReading, Alert, SensorStatistics } from '../services/api';
import SensorCard from './SensorCard';
import StatisticsPanel from './StatisticsPanel';
import AlertsPanel from './AlertsPanel';
import ConnectionStatus from './ConnectionStatus';

const Dashboard: React.FC = () => {
  const { isConnected, newReadings, newAlerts, clearNewReadings, clearNewAlerts } = useSignalR();
  
  const [sensors, setSensors] = useState<Sensor[]>([]);
  const [readings, setReadings] = useState<SensorReading[]>([]);
  const [alerts, setAlerts] = useState<Alert[]>([]);
  const [statistics, setStatistics] = useState<Map<number, SensorStatistics>>(new Map());
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [showSnackbar, setShowSnackbar] = useState(false);
  const [snackbarMessage, setSnackbarMessage] = useState('');

  // Load initial data
  useEffect(() => {
    const loadInitialData = async () => {
      try {
        setLoading(true);
        const [sensorsData, readingsData, alertsData] = await Promise.all([
          ApiService.getSensors(),
          ApiService.getSensorReadings(1000),
          ApiService.getAlerts(1, 20),
        ]);

        setSensors(sensorsData);
        setReadings(readingsData);
        setAlerts(alertsData);

        // Load statistics for each sensor
        const statsPromises = sensorsData.map(async (sensor) => {
          try {
            const stats = await ApiService.getSensorStatistics(sensor.id);
            return { sensorId: sensor.id, stats };
          } catch (error) {
            console.warn(`Failed to load statistics for sensor ${sensor.id}:`, error);
            return null;
          }
        });

        const statsResults = await Promise.all(statsPromises);
        const statsMap = new Map<number, SensorStatistics>();
        statsResults.forEach((result) => {
          if (result) {
            statsMap.set(result.sensorId, result.stats);
          }
        });
        setStatistics(statsMap);

      } catch (error) {
        console.error('Error loading initial data:', error);
        setError('Failed to load dashboard data');
      } finally {
        setLoading(false);
      }
    };

    loadInitialData();
  }, []);

  // Handle new readings from SignalR
  useEffect(() => {
    if (newReadings.length > 0) {
      setReadings(prev => {
        const updated = [...prev, ...newReadings];
        return updated.slice(-1000); // Keep last 1000 readings
      });
      clearNewReadings();
    }
  }, [newReadings, clearNewReadings]);

  // Handle new alerts from SignalR
  useEffect(() => {
    if (newAlerts.length > 0) {
      setAlerts(prev => [newAlerts[0], ...prev].slice(0, 50)); // Keep last 50 alerts
      setSnackbarMessage(`New ${newAlerts[0].severity} alert: ${newAlerts[0].message}`);
      setShowSnackbar(true);
      clearNewAlerts();
    }
  }, [newAlerts, clearNewAlerts]);

  // Prepare chart data
  const chartData = React.useMemo(() => {
    const sensorData = new Map<number, { data: number[], timestamps: string[] }>();
    
    readings.forEach(reading => {
      if (!sensorData.has(reading.sensorId)) {
        sensorData.set(reading.sensorId, { data: [], timestamps: [] });
      }
      const sensor = sensorData.get(reading.sensorId)!;
      sensor.data.push(reading.value);
      sensor.timestamps.push(reading.timestamp);
    });

    return sensorData;
  }, [readings]);

  // Prepare pie chart data for sensor types
  const sensorTypeData = React.useMemo(() => {
    const typeCount = new Map<string, number>();
    sensors.forEach(sensor => {
      typeCount.set(sensor.type, (typeCount.get(sensor.type) || 0) + 1);
    });
    
    return Array.from(typeCount.entries()).map(([type, count], index) => ({
      id: index,
      value: count,
      label: type,
    }));
  }, [sensors]);

  // Prepare alert severity data
  const alertSeverityData = React.useMemo(() => {
    const severityCount = new Map<string, number>();
    alerts.forEach(alert => {
      severityCount.set(alert.severity, (severityCount.get(alert.severity) || 0) + 1);
    });
    
    return Array.from(severityCount.entries()).map(([severity, count], index) => ({
      id: index,
      value: count,
      label: severity,
    }));
  }, [alerts]);

  const handleSnackbarClose = () => {
    setShowSnackbar(false);
  };

  if (loading) {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" minHeight="100vh">
        <CircularProgress size={60} />
        <Typography variant="h6" sx={{ ml: 2 }}>
          Loading Dashboard...
        </Typography>
      </Box>
    );
  }

  if (error) {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" minHeight="100vh">
        <MuiAlert severity="error" sx={{ maxWidth: 400 }}>
          {error}
        </MuiAlert>
      </Box>
    );
  }

  return (
    <Box sx={{ flexGrow: 1, p: 3 }}>
      {/* Header */}
      <Box sx={{ mb: 3 }}>
        <Typography variant="h4" component="h1" gutterBottom>
          Real-Time Sensor Dashboard
        </Typography>
        <ConnectionStatus isConnected={isConnected} />
      </Box>

      {/* Statistics Cards */}
      <Grid container spacing={3} sx={{ mb: 3 }}>
        <Grid item xs={12} sm={6} md={3}>
          <Card>
            <CardContent>
              <Typography color="textSecondary" gutterBottom>
                Total Sensors
              </Typography>
              <Typography variant="h4">
                {sensors.length}
              </Typography>
            </CardContent>
          </Card>
        </Grid>
        <Grid item xs={12} sm={6} md={3}>
          <Card>
            <CardContent>
              <Typography color="textSecondary" gutterBottom>
                Total Readings
              </Typography>
              <Typography variant="h4">
                {readings.length.toLocaleString()}
              </Typography>
            </CardContent>
          </Card>
        </Grid>
        <Grid item xs={12} sm={6} md={3}>
          <Card>
            <CardContent>
              <Typography color="textSecondary" gutterBottom>
                Active Alerts
              </Typography>
              <Typography variant="h4" color="error">
                {alerts.filter(alert => !alert.isResolved).length}
              </Typography>
            </CardContent>
          </Card>
        </Grid>
        <Grid item xs={12} sm={6} md={3}>
          <Card>
            <CardContent>
              <Typography color="textSecondary" gutterBottom>
                Data Rate
              </Typography>
              <Typography variant="h4" color="primary">
                {newReadings.length}/sec
              </Typography>
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      {/* Main Content */}
      <Grid container spacing={3}>
        {/* Real-time Charts */}
        <Grid item xs={12} lg={8}>
          <Paper sx={{ p: 3, mb: 3 }}>
            <Typography variant="h6" gutterBottom>
              Live Sensor Data
            </Typography>
            <Box sx={{ height: 400 }}>
              {chartData.size > 0 ? (
                <LineChart
                  series={Array.from(chartData.entries()).map(([sensorId, data]) => {
                    const sensor = sensors.find(s => s.id === sensorId);
                    return {
                      data: data.data.slice(-50), // Show last 50 points
                      label: sensor?.name || `Sensor ${sensorId}`,
                      color: `hsl(${(sensorId * 137.5) % 360}, 70%, 50%)`,
                    };
                  })}
                  height={350}
                />
              ) : (
                <Box display="flex" justifyContent="center" alignItems="center" height="100%">
                  <Typography color="textSecondary">
                    No data available
                  </Typography>
                </Box>
              )}
            </Box>
          </Paper>

          {/* Sensor Statistics */}
          <StatisticsPanel statistics={statistics} sensors={sensors} />
        </Grid>

        {/* Sidebar */}
        <Grid item xs={12} lg={4}>
          {/* Sensor Types Chart */}
          <Paper sx={{ p: 3, mb: 3 }}>
            <Typography variant="h6" gutterBottom>
              Sensor Types
            </Typography>
            <Box sx={{ height: 300 }}>
              {sensorTypeData.length > 0 ? (
                <PieChart
                  series={[{
                    data: sensorTypeData,
                    innerRadius: 30,
                    outerRadius: 100,
                  }]}
                  height={300}
                />
              ) : (
                <Box display="flex" justifyContent="center" alignItems="center" height="100%">
                  <Typography color="textSecondary">
                    No data available
                  </Typography>
                </Box>
              )}
            </Box>
          </Paper>

          {/* Alert Severity Chart */}
          <Paper sx={{ p: 3, mb: 3 }}>
            <Typography variant="h6" gutterBottom>
              Alert Severity
            </Typography>
            <Box sx={{ height: 300 }}>
              {alertSeverityData.length > 0 ? (
                <BarChart
                  series={[{
                    data: alertSeverityData.map(item => item.value),
                  }]}
                  xAxis={[{
                    data: alertSeverityData.map(item => item.label),
                    scaleType: 'band',
                  }]}
                  height={300}
                />
              ) : (
                <Box display="flex" justifyContent="center" alignItems="center" height="100%">
                  <Typography color="textSecondary">
                    No alerts available
                  </Typography>
                </Box>
              )}
            </Box>
          </Paper>

          {/* Alerts Panel */}
          <AlertsPanel alerts={alerts} />
        </Grid>
      </Grid>

      {/* Individual Sensor Cards */}
      <Box sx={{ mt: 3 }}>
        <Typography variant="h6" gutterBottom>
          Sensor Details
        </Typography>
        <Grid container spacing={2}>
          {sensors.map((sensor) => (
            <Grid item xs={12} sm={6} md={4} key={sensor.id}>
              <SensorCard
                sensor={sensor}
                readings={readings.filter(r => r.sensorId === sensor.id).slice(-10)}
                statistics={statistics.get(sensor.id)}
              />
            </Grid>
          ))}
        </Grid>
      </Box>

      {/* Snackbar for new alerts */}
      <Snackbar
        open={showSnackbar}
        autoHideDuration={6000}
        onClose={handleSnackbarClose}
        anchorOrigin={{ vertical: 'top', horizontal: 'right' }}
      >
        <MuiAlert onClose={handleSnackbarClose} severity="warning" sx={{ width: '100%' }}>
          {snackbarMessage}
        </MuiAlert>
      </Snackbar>
    </Box>
  );
};

export default Dashboard;


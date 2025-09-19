import React from 'react';
import {
  Paper,
  Typography,
  Box,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Chip,
} from '@mui/material';
import { Sensor, SensorStatistics } from '../services/api';

interface StatisticsPanelProps {
  statistics: Map<number, SensorStatistics>;
  sensors: Sensor[];
}

const StatisticsPanel: React.FC<StatisticsPanelProps> = ({ statistics, sensors }) => {
  const getSeverityColor = (severity: 'low' | 'medium' | 'high') => {
    switch (severity) {
      case 'low': return 'success';
      case 'medium': return 'warning';
      case 'high': return 'error';
      default: return 'default';
    }
  };

  const getSeverityLevel = (stats: SensorStatistics) => {
    const range = stats.max - stats.min;
    const deviation = stats.standardDeviation;
    
    if (deviation > range * 0.2) return 'high';
    if (deviation > range * 0.1) return 'medium';
    return 'low';
  };

  return (
    <Paper sx={{ p: 3 }}>
      <Typography variant="h6" gutterBottom>
        Sensor Statistics
      </Typography>
      
      <TableContainer>
        <Table size="small">
          <TableHead>
            <TableRow>
              <TableCell>Sensor</TableCell>
              <TableCell align="right">Min</TableCell>
              <TableCell align="right">Max</TableCell>
              <TableCell align="right">Avg</TableCell>
              <TableCell align="right">Std Dev</TableCell>
              <TableCell align="center">Stability</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {sensors.map((sensor) => {
              const stats = statistics.get(sensor.id);
              if (!stats) return null;
              
              const severity = getSeverityLevel(stats);
              
              return (
                <TableRow key={sensor.id}>
                  <TableCell>
                    <Box>
                      <Typography variant="body2" noWrap>
                        {sensor.name}
                      </Typography>
                      <Typography variant="caption" color="textSecondary">
                        {sensor.type}
                      </Typography>
                    </Box>
                  </TableCell>
                  <TableCell align="right">
                    {stats.min.toFixed(2)}
                  </TableCell>
                  <TableCell align="right">
                    {stats.max.toFixed(2)}
                  </TableCell>
                  <TableCell align="right">
                    {stats.average.toFixed(2)}
                  </TableCell>
                  <TableCell align="right">
                    {stats.standardDeviation.toFixed(2)}
                  </TableCell>
                  <TableCell align="center">
                    <Chip
                      label={severity}
                      size="small"
                      color={getSeverityColor(severity)}
                      variant="outlined"
                    />
                  </TableCell>
                </TableRow>
              );
            })}
          </TableBody>
        </Table>
      </TableContainer>
    </Paper>
  );
};

export default StatisticsPanel;


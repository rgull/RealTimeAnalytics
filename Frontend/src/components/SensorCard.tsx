import React from "react";
import {
  Card,
  CardContent,
  Typography,
  Box,
  Chip,
  LinearProgress,
  Grid,
} from "@mui/material";
import { TrendingUp, TrendingDown, TrendingFlat } from "@mui/icons-material";
import { Sensor, SensorReading, SensorStatistics } from "../services/api";

interface SensorCardProps {
  sensor: Sensor;
  readings: SensorReading[];
  statistics?: SensorStatistics;
}

const SensorCard: React.FC<SensorCardProps> = ({
  sensor,
  readings,
  statistics,
}) => {
  const getTrendIcon = (current: number, previous: number) => {
    if (current > previous) return <TrendingUp color="success" />;
    if (current < previous) return <TrendingDown color="error" />;
    return <TrendingFlat color="action" />;
  };

  const currentValue =
    readings.length > 0 ? readings[readings.length - 1]?.value : 0;
  const previousValue =
    readings.length > 1 ? readings[readings.length - 2]?.value : currentValue;

  const getSeverityColor = (value: number) => {
    if (!statistics) return "default";

    const { min, max, average } = statistics;
    const range = max - min;
    const deviation = Math.abs(value - average);

    if (deviation > range * 0.3) return "error";
    if (deviation > range * 0.15) return "warning";
    return "success";
  };

  return (
    <Card sx={{ height: "100%", display: "flex", flexDirection: "column" }}>
      <CardContent sx={{ flexGrow: 1 }}>
        <Box
          sx={{
            display: "flex",
            justifyContent: "space-between",
            alignItems: "flex-start",
            mb: 2,
          }}
        >
          <Typography variant="h6" component="h3" noWrap>
            {sensor.name}
          </Typography>
          <Chip
            label={sensor.type}
            size="small"
            color="primary"
            variant="outlined"
          />
        </Box>

        <Typography variant="body2" color="textSecondary" sx={{ mb: 2 }}>
          {sensor.location || "Unknown Location"}
        </Typography>

        {/* Current Value */}
        <Box sx={{ mb: 2 }}>
          <Box sx={{ display: "flex", alignItems: "center", gap: 1, mb: 1 }}>
            <Typography variant="h4" component="span">
              {currentValue.toFixed(2)}
            </Typography>
            <Typography variant="body2" color="textSecondary">
              {sensor.unit || ""}
            </Typography>
            {getTrendIcon(currentValue, previousValue)}
          </Box>

          <LinearProgress
            variant="determinate"
            value={
              statistics
                ? ((currentValue - statistics.min) /
                    (statistics.max - statistics.min)) *
                  100
                : 50
            }
            color={getSeverityColor(currentValue) as any}
            sx={{ height: 8, borderRadius: 4 }}
          />
        </Box>

        {/* Statistics */}
        {statistics && (
          <Grid container spacing={1}>
            <Grid item xs={6}>
              <Typography variant="body2" color="textSecondary">
                Min: {statistics.min.toFixed(2)}
              </Typography>
            </Grid>
            <Grid item xs={6}>
              <Typography variant="body2" color="textSecondary">
                Max: {statistics.max.toFixed(2)}
              </Typography>
            </Grid>
            <Grid item xs={6}>
              <Typography variant="body2" color="textSecondary">
                Avg: {statistics.average.toFixed(2)}
              </Typography>
            </Grid>
            <Grid item xs={6}>
              <Typography variant="body2" color="textSecondary">
                Count: {statistics.count}
              </Typography>
            </Grid>
          </Grid>
        )}

        {/* Status */}
        <Box
          sx={{
            mt: 2,
            display: "flex",
            justifyContent: "space-between",
            alignItems: "center",
          }}
        >
          <Chip
            label={sensor.isActive ? "Active" : "Inactive"}
            size="small"
            color={sensor.isActive ? "success" : "default"}
            variant="outlined"
          />
          <Typography variant="caption" color="textSecondary">
            {readings.length} readings
          </Typography>
        </Box>
      </CardContent>
    </Card>
  );
};

export default SensorCard;

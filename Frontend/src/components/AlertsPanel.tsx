import React, { useState } from 'react';
import {
  Paper,
  Typography,
  Box,
  List,
  ListItem,
  ListItemText,
  ListItemIcon,
  Chip,
  IconButton,
  Collapse,
  Button,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
} from '@mui/material';
import {
  Warning,
  Error,
  Info,
  //Critical,
  ExpandMore,
  ExpandLess,
  Visibility,
} from '@mui/icons-material';
import { Alert } from '../services/api';
import { ApiService } from '../services/api';

interface AlertsPanelProps {
  alerts: Alert[];
}

const AlertsPanel: React.FC<AlertsPanelProps> = ({ alerts }) => {
  const [expanded, setExpanded] = useState(false);
  const [selectedAlert, setSelectedAlert] = useState<Alert | null>(null);
  const [dialogOpen, setDialogOpen] = useState(false);

  const getSeverityIcon = (severity: string) => {
    switch (severity.toLowerCase()) {
      case 'critical':
        return <Warning color="warning" />;
      case 'error':
        return <Error color="error" />;
      case 'warning':
        return <Warning color="warning" />;
      case 'info':
        return <Info color="info" />;
      default:
        return <Info color="action" />;
    }
  };

  const getSeverityColor = (severity: string) => {
    switch (severity.toLowerCase()) {
      case 'critical':
        return 'error';
      case 'error':
        return 'error';
      case 'warning':
        return 'warning';
      case 'info':
        return 'info';
      default:
        return 'default';
    }
  };

  const formatTimestamp = (timestamp: string) => {
    return new Date(timestamp).toLocaleString();
  };

  const handleViewAlert = (alert: Alert) => {
    setSelectedAlert(alert);
    setDialogOpen(true);
  };

  const handleResolveAlert = async (alertId: number) => {
    try {
      await ApiService.resolveAlert(alertId);
      // The parent component should refresh the alerts
    } catch (error) {
      console.error('Failed to resolve alert:', error);
    }
  };

  const recentAlerts = alerts.slice(0, 5);
  const unresolvedCount = alerts.filter(alert => !alert.isResolved).length;

  return (
    <Paper sx={{ p: 3 }}>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
        <Typography variant="h6">
          Recent Alerts
        </Typography>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          {unresolvedCount > 0 && (
            <Chip
              label={`${unresolvedCount} unresolved`}
              color="error"
              size="small"
            />
          )}
          <IconButton
            onClick={() => setExpanded(!expanded)}
            size="small"
          >
            {expanded ? <ExpandLess /> : <ExpandMore />}
          </IconButton>
        </Box>
      </Box>

      <Collapse in={expanded || recentAlerts.length <= 3}>
        <List dense>
          {recentAlerts.map((alert) => (
            <ListItem
              key={alert.id}
              sx={{
                borderLeft: `4px solid ${
                  alert.severity === 'Critical' ? 'error.main' :
                  alert.severity === 'Error' ? 'error.main' :
                  alert.severity === 'Warning' ? 'warning.main' :
                  'info.main'
                }`,
                mb: 1,
                bgcolor: 'background.paper',
                borderRadius: 1,
              }}
            >
              <ListItemIcon>
                {getSeverityIcon(alert.severity)}
              </ListItemIcon>
              <ListItemText
                primary={
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 0.5 }}>
                    <Typography variant="body2" noWrap sx={{ flexGrow: 1 }}>
                      {alert.message}
                    </Typography>
                    <Chip
                      label={alert.severity}
                      size="small"
                      color={getSeverityColor(alert.severity)}
                      variant="outlined"
                    />
                  </Box>
                }
                secondary={
                  <Box>
                    <Typography variant="caption" color="textSecondary">
                      {formatTimestamp(alert.createdAt)}
                    </Typography>
                    {alert.actualValue && (
                      <Typography variant="caption" color="textSecondary" sx={{ ml: 1 }}>
                        Value: {alert.actualValue.toFixed(2)}
                      </Typography>
                    )}
                  </Box>
                }
              />
              <IconButton
                size="small"
                onClick={() => handleViewAlert(alert)}
              >
                <Visibility />
              </IconButton>
            </ListItem>
          ))}
          
          {recentAlerts.length === 0 && (
            <ListItem>
              <ListItemText
                primary="No alerts"
                secondary="All systems operating normally"
              />
            </ListItem>
          )}
        </List>
      </Collapse>

      {/* Alert Detail Dialog */}
      <Dialog
        open={dialogOpen}
        onClose={() => setDialogOpen(false)}
        maxWidth="sm"
        fullWidth
      >
        <DialogTitle>
          Alert Details
        </DialogTitle>
        <DialogContent>
          {selectedAlert && (
            <Box>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 2 }}>
                {getSeverityIcon(selectedAlert.severity)}
                <Chip
                  label={selectedAlert.severity}
                  color={getSeverityColor(selectedAlert.severity)}
                  variant="outlined"
                />
                <Chip
                  label={selectedAlert.isResolved ? 'Resolved' : 'Unresolved'}
                  color={selectedAlert.isResolved ? 'success' : 'error'}
                  size="small"
                />
              </Box>
              
              <Typography variant="body1" sx={{ mb: 2 }}>
                {selectedAlert.message}
              </Typography>
              
              <Box sx={{ display: 'flex', gap: 2, mb: 2 }}>
                <Box>
                  <Typography variant="body2" color="textSecondary">
                    Created
                  </Typography>
                  <Typography variant="body2">
                    {formatTimestamp(selectedAlert.createdAt)}
                  </Typography>
                </Box>
                
                {selectedAlert.resolvedAt && (
                  <Box>
                    <Typography variant="body2" color="textSecondary">
                      Resolved
                    </Typography>
                    <Typography variant="body2">
                      {formatTimestamp(selectedAlert.resolvedAt)}
                    </Typography>
                  </Box>
                )}
              </Box>
              
              {selectedAlert.actualValue && (
                <Box sx={{ mb: 2 }}>
                  <Typography variant="body2" color="textSecondary">
                    Actual Value
                  </Typography>
                  <Typography variant="h6">
                    {selectedAlert.actualValue.toFixed(2)}
                  </Typography>
                </Box>
              )}
              
              {selectedAlert.thresholdValue && (
                <Box>
                  <Typography variant="body2" color="textSecondary">
                    Threshold Value
                  </Typography>
                  <Typography variant="body2">
                    {selectedAlert.thresholdValue.toFixed(2)}
                  </Typography>
                </Box>
              )}
            </Box>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDialogOpen(false)}>
            Close
          </Button>
          {selectedAlert && !selectedAlert.isResolved && (
            <Button
              onClick={() => {
                handleResolveAlert(selectedAlert.id);
                setDialogOpen(false);
              }}
              color="primary"
              variant="contained"
            >
              Resolve Alert
            </Button>
          )}
        </DialogActions>
      </Dialog>
    </Paper>
  );
};

export default AlertsPanel;


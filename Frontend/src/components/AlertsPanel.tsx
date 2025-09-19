import React, { useState, useEffect } from "react";
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
  Tabs,
  Tab,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Pagination,
  CircularProgress,
} from "@mui/material";
import {
  Warning,
  Error,
  Info,
  ExpandMore,
  ExpandLess,
  Visibility,
  Refresh,
  Add,
} from "@mui/icons-material";
import { Alert } from "../services/api";
import { ApiService } from "../services/api";

interface AlertsPanelProps {
  alerts: Alert[];
  onRefresh?: () => void;
}

interface TabPanelProps {
  children?: React.ReactNode;
  index: number;
  value: number;
}

function TabPanel(props: TabPanelProps) {
  const { children, value, index, ...other } = props;

  return (
    <div
      role="tabpanel"
      hidden={value !== index}
      id={`alert-tabpanel-${index}`}
      aria-labelledby={`alert-tab-${index}`}
      {...other}
    >
      {value === index && <Box sx={{ p: 3 }}>{children}</Box>}
    </div>
  );
}

const AlertsPanel: React.FC<AlertsPanelProps> = ({ alerts, onRefresh }) => {
  const [expanded, setExpanded] = useState(false);
  const [selectedAlert, setSelectedAlert] = useState<Alert | null>(null);
  const [dialogOpen, setDialogOpen] = useState(false);
  const [tabValue, setTabValue] = useState(0);
  const [alertHistory, setAlertHistory] = useState<Alert[]>([]);
  const [loading, setLoading] = useState(false);
  const [severityFilter, setSeverityFilter] = useState<string>("");
  const [resolvedFilter, setResolvedFilter] = useState<string>("");
  const [currentPage, setCurrentPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);

  // Load alert history when component mounts or filters change
  useEffect(() => {
    loadAlertHistory();
  }, [currentPage, severityFilter, resolvedFilter]);

  const loadAlertHistory = async () => {
    setLoading(true);
    try {
      const isResolved =
        resolvedFilter === "resolved"
          ? true
          : resolvedFilter === "unresolved"
          ? false
          : undefined;
      const alerts = await ApiService.getAlerts(
        currentPage,
        20,
        severityFilter || undefined,
        isResolved
      );
      setAlertHistory(alerts);
      // Calculate total pages (assuming 20 items per page)
      setTotalPages(Math.ceil(alerts.length / 20));
    } catch (error) {
      console.error("Failed to load alert history:", error);
    } finally {
      setLoading(false);
    }
  };

  const handleTabChange = (event: React.SyntheticEvent, newValue: number) => {
    setTabValue(newValue);
  };

  const handleCreateCustomAlert = async () => {
    try {
      // Create a sample custom alert
      await ApiService.createAlert({
        sensorId: 1,
        message: "Custom alert created manually",
        severity: "Info",
        thresholdValue: 50,
        actualValue: 55,
      });
      if (onRefresh) {
        onRefresh();
      }
      loadAlertHistory();
    } catch (error) {
      console.error("Failed to create custom alert:", error);
    }
  };

  const getSeverityIcon = (severity: string) => {
    switch (severity.toLowerCase()) {
      case "critical":
        return <Error color="error" />;
      case "error":
        return <Error color="error" />;
      case "warning":
        return <Warning color="warning" />;
      case "info":
        return <Info color="info" />;
      default:
        return <Info color="action" />;
    }
  };

  const getSeverityColor = (severity: string) => {
    switch (severity.toLowerCase()) {
      case "critical":
        return "error";
      case "error":
        return "error";
      case "warning":
        return "warning";
      case "info":
        return "info";
      default:
        return "default";
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
      console.error("Failed to resolve alert:", error);
    }
  };

  const recentAlerts = alerts.slice(0, 5);
  const unresolvedCount = alerts.filter((alert) => !alert.isResolved).length;

  return (
    <Paper sx={{ p: 3 }}>
      <Box
        sx={{
          display: "flex",
          justifyContent: "space-between",
          alignItems: "center",
          mb: 2,
        }}
      >
        <Typography variant="h6">Alert Management</Typography>
        <Box sx={{ display: "flex", alignItems: "center", gap: 1 }}>
          {unresolvedCount > 0 && (
            <Chip
              label={`${unresolvedCount} unresolved`}
              color="error"
              size="small"
            />
          )}
          <IconButton
            onClick={loadAlertHistory}
            size="small"
            disabled={loading}
          >
            <Refresh />
          </IconButton>
          <IconButton onClick={handleCreateCustomAlert} size="small">
            <Add />
          </IconButton>
        </Box>
      </Box>

      <Box sx={{ borderBottom: 1, borderColor: "divider" }}>
        <Tabs
          value={tabValue}
          onChange={handleTabChange}
          aria-label="alert tabs"
        >
          <Tab label="Recent Alerts" />
          <Tab label="Alert History" />
        </Tabs>
      </Box>

      <TabPanel value={tabValue} index={0}>
        <Box
          sx={{
            display: "flex",
            justifyContent: "space-between",
            alignItems: "center",
            mb: 2,
          }}
        >
          <Typography variant="subtitle1">Real-time Alerts</Typography>
          <IconButton onClick={() => setExpanded(!expanded)} size="small">
            {expanded ? <ExpandLess /> : <ExpandMore />}
          </IconButton>
        </Box>

        <Collapse in={expanded || recentAlerts.length <= 3}>
          <List dense>
            {recentAlerts.map((alert) => (
              <ListItem
                key={alert.id}
                sx={{
                  borderLeft: `4px solid ${
                    alert.severity === "Critical"
                      ? "error.main"
                      : alert.severity === "Error"
                      ? "error.main"
                      : alert.severity === "Warning"
                      ? "warning.main"
                      : "info.main"
                  }`,
                  mb: 1,
                  bgcolor: "background.paper",
                  borderRadius: 1,
                }}
              >
                <ListItemIcon>{getSeverityIcon(alert.severity)}</ListItemIcon>
                <ListItemText
                  primary={
                    <Box
                      sx={{
                        display: "flex",
                        alignItems: "center",
                        gap: 1,
                        mb: 0.5,
                      }}
                    >
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
                        <Typography
                          variant="caption"
                          color="textSecondary"
                          sx={{ ml: 1 }}
                        >
                          Value: {alert.actualValue.toFixed(2)}
                        </Typography>
                      )}
                    </Box>
                  }
                />
                <IconButton size="small" onClick={() => handleViewAlert(alert)}>
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
      </TabPanel>

      <TabPanel value={tabValue} index={1}>
        <Box sx={{ mb: 2, display: "flex", gap: 2, alignItems: "center" }}>
          <FormControl size="small" sx={{ minWidth: 120 }}>
            <InputLabel>Severity</InputLabel>
            <Select
              value={severityFilter}
              label="Severity"
              onChange={(e) => setSeverityFilter(e.target.value)}
            >
              <MenuItem value="">All</MenuItem>
              <MenuItem value="Critical">Critical</MenuItem>
              <MenuItem value="Error">Error</MenuItem>
              <MenuItem value="Warning">Warning</MenuItem>
              <MenuItem value="Info">Info</MenuItem>
            </Select>
          </FormControl>

          <FormControl size="small" sx={{ minWidth: 120 }}>
            <InputLabel>Status</InputLabel>
            <Select
              value={resolvedFilter}
              label="Status"
              onChange={(e) => setResolvedFilter(e.target.value)}
            >
              <MenuItem value="">All</MenuItem>
              <MenuItem value="unresolved">Unresolved</MenuItem>
              <MenuItem value="resolved">Resolved</MenuItem>
            </Select>
          </FormControl>
        </Box>

        {loading ? (
          <Box sx={{ display: "flex", justifyContent: "center", p: 3 }}>
            <CircularProgress />
          </Box>
        ) : (
          <>
            <List dense>
              {alertHistory.map((alert) => (
                <ListItem
                  key={alert.id}
                  sx={{
                    borderLeft: `4px solid ${
                      alert.severity === "Critical"
                        ? "error.main"
                        : alert.severity === "Error"
                        ? "error.main"
                        : alert.severity === "Warning"
                        ? "warning.main"
                        : "info.main"
                    }`,
                    mb: 1,
                    bgcolor: "background.paper",
                    borderRadius: 1,
                    opacity: alert.isResolved ? 0.7 : 1,
                  }}
                >
                  <ListItemIcon>{getSeverityIcon(alert.severity)}</ListItemIcon>
                  <ListItemText
                    primary={
                      <Box
                        sx={{
                          display: "flex",
                          alignItems: "center",
                          gap: 1,
                          mb: 0.5,
                        }}
                      >
                        <Typography variant="body2" noWrap sx={{ flexGrow: 1 }}>
                          {alert.message}
                        </Typography>
                        <Chip
                          label={alert.severity}
                          size="small"
                          color={getSeverityColor(alert.severity)}
                          variant="outlined"
                        />
                        <Chip
                          label={alert.isResolved ? "Resolved" : "Active"}
                          size="small"
                          color={alert.isResolved ? "success" : "error"}
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
                          <Typography
                            variant="caption"
                            color="textSecondary"
                            sx={{ ml: 1 }}
                          >
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

              {alertHistory.length === 0 && (
                <ListItem>
                  <ListItemText
                    primary="No alerts found"
                    secondary="No alerts match the current filters"
                  />
                </ListItem>
              )}
            </List>

            {totalPages > 1 && (
              <Box sx={{ display: "flex", justifyContent: "center", mt: 2 }}>
                <Pagination
                  count={totalPages}
                  page={currentPage}
                  onChange={(event, page) => setCurrentPage(page)}
                  color="primary"
                />
              </Box>
            )}
          </>
        )}
      </TabPanel>

      {/* Alert Detail Dialog */}
      <Dialog
        open={dialogOpen}
        onClose={() => setDialogOpen(false)}
        maxWidth="sm"
        fullWidth
      >
        <DialogTitle>Alert Details</DialogTitle>
        <DialogContent>
          {selectedAlert && (
            <Box>
              <Box
                sx={{ display: "flex", alignItems: "center", gap: 1, mb: 2 }}
              >
                {getSeverityIcon(selectedAlert.severity)}
                <Chip
                  label={selectedAlert.severity}
                  color={getSeverityColor(selectedAlert.severity)}
                  variant="outlined"
                />
                <Chip
                  label={selectedAlert.isResolved ? "Resolved" : "Unresolved"}
                  color={selectedAlert.isResolved ? "success" : "error"}
                  size="small"
                />
              </Box>

              <Typography variant="body1" sx={{ mb: 2 }}>
                {selectedAlert.message}
              </Typography>

              <Box sx={{ display: "flex", gap: 2, mb: 2 }}>
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
          <Button onClick={() => setDialogOpen(false)}>Close</Button>
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

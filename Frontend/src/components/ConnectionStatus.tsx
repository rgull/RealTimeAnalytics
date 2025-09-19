import React from 'react';
import { Box, Chip, Typography } from '@mui/material';
import { Wifi, WifiOff } from '@mui/icons-material';

interface ConnectionStatusProps {
  isConnected: boolean;
}

const ConnectionStatus: React.FC<ConnectionStatusProps> = ({ isConnected }) => {
  return (
    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
      <Chip
        icon={isConnected ? <Wifi /> : <WifiOff />}
        label={isConnected ? 'Connected' : 'Disconnected'}
        color={isConnected ? 'success' : 'error'}
        variant="outlined"
        size="small"
      />
      <Typography variant="body2" color="textSecondary">
        {isConnected ? 'Real-time updates active' : 'Connection lost - attempting to reconnect...'}
      </Typography>
    </Box>
  );
};

export default ConnectionStatus;


import React, { useEffect, useState } from 'react';
import { useSignalR } from '../services/SignalRService';
import { SensorReading } from '../services/api';

interface RealTimeGraphProps {
  sensorId?: number;
  maxDataPoints?: number;
}

export const RealTimeGraph: React.FC<RealTimeGraphProps> = ({ 
  sensorId, 
  maxDataPoints = 50 
}) => {
  const { newReadings, isConnected, startSimulation, stopSimulation } = useSignalR();
  const [graphData, setGraphData] = useState<{ x: Date; y: number; sensorName: string }[]>([]);
  const [isSimulationRunning, setIsSimulationRunning] = useState(false);

  // Filter readings by sensor ID if specified
  const filteredReadings = sensorId 
    ? newReadings.filter(reading => reading.sensorId === sensorId)
    : newReadings;

  // Update graph data when new readings arrive
  useEffect(() => {
    if (filteredReadings.length > 0) {
      const latestReadings = filteredReadings.slice(-maxDataPoints);
      const newGraphData = latestReadings.map(reading => ({
        x: new Date(reading.timestamp),
        y: reading.value,
        sensorName: `Sensor ${reading.sensorId}`
      }));
      
      setGraphData(newGraphData);
    }
  }, [filteredReadings, maxDataPoints]);

  const handleStartSimulation = async () => {
    await startSimulation();
    setIsSimulationRunning(true);
  };

  const handleStopSimulation = async () => {
    await stopSimulation();
    setIsSimulationRunning(false);
  };

  const getLatestValue = () => {
    if (graphData.length === 0) return 'No data';
    const latest = graphData[graphData.length - 1];
    return `${latest.y.toFixed(2)} (${latest.sensorName})`;
  };

  return (
    <div className="real-time-graph">
      <div className="graph-header">
        <h3>Real-Time Sensor Data</h3>
        <div className="controls">
          <div className="status">
            <span className={`status-indicator ${isConnected ? 'connected' : 'disconnected'}`}>
              {isConnected ? 'Connected' : 'Disconnected'}
            </span>
            <span className="data-count">
              {graphData.length} data points
            </span>
          </div>
          <div className="simulation-controls">
            <button 
              onClick={handleStartSimulation}
              disabled={!isConnected || isSimulationRunning}
              className="btn btn-start"
            >
              Start Simulation
            </button>
            <button 
              onClick={handleStopSimulation}
              disabled={!isConnected || !isSimulationRunning}
              className="btn btn-stop"
            >
              Stop Simulation
            </button>
          </div>
        </div>
      </div>
      
      <div className="graph-content">
        <div className="latest-value">
          <strong>Latest Value:</strong> {getLatestValue()}
        </div>
        
        <div className="graph-container">
          {graphData.length === 0 ? (
            <div className="no-data">
              {isConnected ? 'Waiting for data...' : 'Not connected'}
            </div>
          ) : (
            <div className="data-points">
              {graphData.slice(-10).map((point, index) => (
                <div key={index} className="data-point">
                  <span className="time">
                    {point.x.toLocaleTimeString()}
                  </span>
                  <span className="value">
                    {point.y.toFixed(2)}
                  </span>
                  <span className="sensor">
                    {point.sensorName}
                  </span>
                </div>
              ))}
            </div>
          )}
        </div>
      </div>
      
      <style jsx>{`
        .real-time-graph {
          border: 1px solid #ddd;
          border-radius: 8px;
          padding: 20px;
          margin: 20px 0;
          background: white;
          box-shadow: 0 2px 4px rgba(0,0,0,0.1);
        }
        
        .graph-header {
          display: flex;
          justify-content: space-between;
          align-items: center;
          margin-bottom: 20px;
          flex-wrap: wrap;
          gap: 10px;
        }
        
        .controls {
          display: flex;
          align-items: center;
          gap: 20px;
          flex-wrap: wrap;
        }
        
        .status {
          display: flex;
          align-items: center;
          gap: 10px;
        }
        
        .status-indicator {
          padding: 4px 8px;
          border-radius: 4px;
          font-size: 12px;
          font-weight: bold;
        }
        
        .status-indicator.connected {
          background: #d4edda;
          color: #155724;
        }
        
        .status-indicator.disconnected {
          background: #f8d7da;
          color: #721c24;
        }
        
        .data-count {
          font-size: 14px;
          color: #666;
        }
        
        .simulation-controls {
          display: flex;
          gap: 10px;
        }
        
        .btn {
          padding: 8px 16px;
          border: none;
          border-radius: 4px;
          cursor: pointer;
          font-size: 14px;
          transition: background-color 0.2s;
        }
        
        .btn:disabled {
          opacity: 0.5;
          cursor: not-allowed;
        }
        
        .btn-start {
          background: #28a745;
          color: white;
        }
        
        .btn-start:hover:not(:disabled) {
          background: #218838;
        }
        
        .btn-stop {
          background: #dc3545;
          color: white;
        }
        
        .btn-stop:hover:not(:disabled) {
          background: #c82333;
        }
        
        .graph-content {
          margin-top: 20px;
        }
        
        .latest-value {
          font-size: 16px;
          margin-bottom: 15px;
          padding: 10px;
          background: #f8f9fa;
          border-radius: 4px;
        }
        
        .graph-container {
          min-height: 200px;
          border: 1px solid #e9ecef;
          border-radius: 4px;
          padding: 15px;
        }
        
        .no-data {
          display: flex;
          align-items: center;
          justify-content: center;
          height: 150px;
          color: #666;
          font-style: italic;
        }
        
        .data-points {
          display: flex;
          flex-direction: column;
          gap: 8px;
        }
        
        .data-point {
          display: flex;
          justify-content: space-between;
          align-items: center;
          padding: 8px 12px;
          background: #f8f9fa;
          border-radius: 4px;
          border-left: 4px solid #007bff;
        }
        
        .time {
          font-size: 12px;
          color: #666;
          min-width: 80px;
        }
        
        .value {
          font-weight: bold;
          font-size: 16px;
          color: #007bff;
          min-width: 60px;
          text-align: center;
        }
        
        .sensor {
          font-size: 12px;
          color: #666;
          min-width: 100px;
          text-align: right;
        }
      `}</style>
    </div>
  );
};

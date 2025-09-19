import React, { useEffect, useState } from 'react';
import { useSignalR } from '../services/SignalRService';

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

  const getLatestValue = () => {
    if (graphData.length === 0) return 'No data';
    const latest = graphData[graphData.length - 1];
    return `${latest.y.toFixed(2)} (${latest.sensorName})`;
  };

  return (
    <div className="border border-gray-300 rounded-lg p-5 my-5 bg-white shadow-md">
      <div className="flex justify-between items-center mb-5 flex-wrap gap-2.5">
        <h3 className="text-xl font-semibold">Real-Time Sensor Data</h3>
        <div className="flex items-center gap-5 flex-wrap">
          <div className="flex items-center gap-2.5">
            <span className={`px-2 py-1 rounded text-xs font-bold ${
              isConnected 
                ? 'bg-green-100 text-green-800' 
                : 'bg-red-100 text-red-800'
            }`}>
              {isConnected ? 'Connected' : 'Disconnected'}
            </span>
            <span className="text-sm text-gray-600">
              {graphData.length} data points
            </span>
          </div>
          <div className="flex gap-2.5">
          </div>
        </div>
      </div>
      
      <div className="mt-5">
        <div className="text-base mb-4 p-2.5 bg-gray-50 rounded">
          <strong>Latest Value:</strong> {getLatestValue()}
        </div>
        
        <div className="min-h-[200px] border border-gray-200 rounded p-4">
          {graphData.length === 0 ? (
            <div className="flex items-center justify-center h-[150px] text-gray-600 italic">
              {isConnected ? 'Waiting for data...' : 'Not connected'}
            </div>
          ) : (
            <div className="flex flex-col gap-2">
              {graphData.slice(-10).map((point, index) => (
                <div key={index} className="flex justify-between items-center py-2 px-3 bg-gray-50 rounded border-l-4 border-blue-500">
                  <span className="text-xs text-gray-600 min-w-[80px]">
                    {point.x.toLocaleTimeString()}
                  </span>
                  <span className="font-bold text-base text-blue-600 min-w-[60px] text-center">
                    {point.y.toFixed(2)}
                  </span>
                  <span className="text-xs text-gray-600 min-w-[100px] text-right">
                    {point.sensorName}
                  </span>
                </div>
              ))}
            </div>
          )}
        </div>
      </div>
    </div>
  );
};

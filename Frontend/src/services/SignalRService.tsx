import React, {
  createContext,
  useContext,
  useEffect,
  useState,
  ReactNode,
} from "react";
import * as signalR from "@microsoft/signalr";
import { SensorReading, Alert } from "./api";

interface SignalRContextType {
  connection: signalR.HubConnection | null;
  isConnected: boolean;
  connectionId: string | null;
  newReadings: SensorReading[];
  newAlerts: Alert[];
  subscribeToSensor: (sensorId: number) => Promise<void>;
  unsubscribeFromSensor: (sensorId: number) => Promise<void>;
  startSimulation: () => Promise<void>;
  stopSimulation: () => Promise<void>;
  clearNewReadings: () => void;
  clearNewAlerts: () => void;
}

const SignalRContext = createContext<SignalRContextType | undefined>(undefined);

interface SignalRProviderProps {
  children: ReactNode;
}

export const SignalRProvider: React.FC<SignalRProviderProps> = ({
  children,
}) => {
  const [connection, setConnection] = useState<signalR.HubConnection | null>(
    null
  );
  const [isConnected, setIsConnected] = useState(false);
  const [connectionId, setConnectionId] = useState<string | null>(null);
  const [newReadings, setNewReadings] = useState<SensorReading[]>([]);
  const [newAlerts, setNewAlerts] = useState<Alert[]>([]);

  useEffect(() => {
    const startConnection = async () => {
      try {
        const hubConnection = new signalR.HubConnectionBuilder()
          .withUrl("http://localhost:5025/sensorHub", {
            skipNegotiation: true,
            transport: signalR.HttpTransportType.WebSockets,
          })
          .withAutomaticReconnect({
            nextRetryDelayInMilliseconds: (retryContext) => {
              if (retryContext.previousRetryCount < 5) {
                return Math.min(
                  1000 * Math.pow(2, retryContext.previousRetryCount),
                  30000
                );
              }
              return 30000;
            },
          })
          .configureLogging(signalR.LogLevel.Information)
          .build();

        // Set up event handlers
        hubConnection.on("NewReadings", (readings: SensorReading[]) => {
          setNewReadings((prev) => [...prev, ...readings].slice(-200)); // Keep last 200 readings for continuous flow
        });

        hubConnection.on("NewReading", (reading: SensorReading) => {
          setNewReadings((prev) => [...prev, reading].slice(-200)); // Keep last 200 readings
        });

        hubConnection.on("NewAlert", (alert: Alert) => {
          setNewAlerts((prev) => [alert, ...prev].slice(0, 50)); // Keep last 50 alerts
        });

        hubConnection.on("SimulationStarted", (message: string) => {
          console.log("Simulation started:", message);
        });

        hubConnection.on("SimulationStopped", (message: string) => {
          console.log("Simulation stopped:", message);
        });

        hubConnection.onclose((error) => {
          console.log("SignalR connection closed:", error);
          setIsConnected(false);
          setConnectionId(null);
        });

        hubConnection.onreconnecting((error) => {
          console.log("SignalR reconnecting:", error);
          setIsConnected(false);
        });

        hubConnection.onreconnected((connectionId) => {
          console.log("SignalR reconnected:", connectionId);
          setIsConnected(true);
          setConnectionId(connectionId as any);
        });

        // Start the connection
        await hubConnection.start();
        console.log("SignalR connection started");

        setConnection(hubConnection);
        setIsConnected(true);
        setConnectionId(hubConnection.connectionId);

        // Auto-start the sensor simulation
        try {
          await hubConnection.invoke("StartSimulation");
          console.log("Auto-started sensor simulation");
        } catch (error) {
          console.warn("Could not auto-start simulation:", error);
        }
      } catch (error) {
        console.error("Error starting SignalR connection:", error);
        setIsConnected(false);
      }
    };

    startConnection();

    // Cleanup on unmount
    return () => {
      if (connection) {
        connection.stop();
      }
    };
  }, []);

  const subscribeToSensor = async (sensorId: number) => {
    if (connection && isConnected) {
      try {
        await connection.invoke("SubscribeToSensor", sensorId);
        console.log(`Subscribed to sensor ${sensorId}`);
      } catch (error) {
        console.error(`Error subscribing to sensor ${sensorId}:`, error);
      }
    }
  };

  const unsubscribeFromSensor = async (sensorId: number) => {
    if (connection && isConnected) {
      try {
        await connection.invoke("UnsubscribeFromSensor", sensorId);
        console.log(`Unsubscribed from sensor ${sensorId}`);
      } catch (error) {
        console.error(`Error unsubscribing from sensor ${sensorId}:`, error);
      }
    }
  };

  const clearNewReadings = () => {
    setNewReadings([]);
  };

  const clearNewAlerts = () => {
    setNewAlerts([]);
  };

  const startSimulation = async () => {
    if (connection && isConnected) {
      try {
        await connection.invoke("StartSimulation");
        console.log("Started sensor simulation");
      } catch (error) {
        console.error("Error starting simulation:", error);
      }
    }
  };

  const stopSimulation = async () => {
    if (connection && isConnected) {
      try {
        await connection.invoke("StopSimulation");
        console.log("Stopped sensor simulation");
      } catch (error) {
        console.error("Error stopping simulation:", error);
      }
    }
  };

  const value: SignalRContextType = {
    connection,
    isConnected,
    connectionId,
    newReadings,
    newAlerts,
    subscribeToSensor,
    unsubscribeFromSensor,
    startSimulation,
    stopSimulation,
    clearNewReadings,
    clearNewAlerts,
  };

  return (
    <SignalRContext.Provider value={value}>{children}</SignalRContext.Provider>
  );
};

export const useSignalR = (): SignalRContextType => {
  const context = useContext(SignalRContext);
  if (context === undefined) {
    throw new Error("useSignalR must be used within a SignalRProvider");
  }
  return context;
};

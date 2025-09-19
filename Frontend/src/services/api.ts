import axios from 'axios';

const API_BASE_URL = 'http://localhost:5025/api';

const apiClient = axios.create({
  baseURL: API_BASE_URL,
  timeout: 50000,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Request interceptor
apiClient.interceptors.request.use(
  (config) => {
    console.log(`API Request: ${config.method?.toUpperCase()} ${config.url}`);
    return config;
  },
  (error) => {
    console.error('API Request Error:', error);
    return Promise.reject(error);
  }
);

// Response interceptor
apiClient.interceptors.response.use(
  (response) => {
    console.log(`API Response: ${response.status} ${response.config.url}`);
    return response;
  },
  (error) => {
    console.error('API Response Error:', error.response?.data || error.message);
    return Promise.reject(error);
  }
);

export interface Sensor {
  id: number;
  name: string;
  type: string;
  location?: string;
  description?: string;
  unit?: string;
  isActive: boolean;
  createdAt: string;
  lastReadingAt?: string;
}

export interface SensorReading {
  id: number;
  sensorId: number;
  value: number;
  unit?: string;
  timestamp: string;
  metadata?: string;
}

export interface Alert {
  id: number;
  sensorId: number;
  message: string;
  severity: 'Info' | 'Warning' | 'Error' | 'Critical';
  thresholdValue?: number;
  actualValue?: number;
  createdAt: string;
  isResolved: boolean;
  resolvedAt?: string;
}

export interface SensorStatistics {
  sensorId: number;
  count: number;
  min: number;
  max: number;
  average: number;
  standardDeviation: number;
  lastReading: SensorReading;
  timeRange: {
    start: string;
    end: string;
  };
}

export interface AlertStatistics {
  total: number;
  resolved: number;
  unresolved: number;
  recent24Hours: number;
  bySeverity: Array<{
    severity: string;
    count: number;
  }>;
}

// API Service Class
export class ApiService {
  // Sensors
  static async getSensors(): Promise<Sensor[]> {
    const response = await apiClient.get('/sensors');
    return response.data;
  }

  static async getSensor(id: number): Promise<Sensor> {
    const response = await apiClient.get(`/sensors/${id}`);
    return response.data;
  }

  static async createSensor(sensor: Omit<Sensor, 'id' | 'createdAt'>): Promise<Sensor> {
    const response = await apiClient.post('/sensors', sensor);
    return response.data;
  }

  static async updateSensor(id: number, sensor: Partial<Sensor>): Promise<void> {
    await apiClient.put(`/sensors/${id}`, sensor);
  }

  static async deleteSensor(id: number): Promise<void> {
    await apiClient.delete(`/sensors/${id}`);
  }

  // Sensor Readings
  static async getSensorReadings(count: number = 1000): Promise<SensorReading[]> {
    const response = await apiClient.get(`/sensorreadings?count=${count}`);
    return response.data;
  }

  static async getReadingsBySensor(sensorId: number, count: number = 100): Promise<SensorReading[]> {
    const response = await apiClient.get(`/sensorreadings/by-sensor/${sensorId}?count=${count}`);
    return response.data;
  }

  static async getReadingsByTimeRange(startTime: string, endTime: string): Promise<SensorReading[]> {
    const response = await apiClient.get(`/sensorreadings/by-time-range?startTime=${startTime}&endTime=${endTime}`);
    return response.data;
  }

  static async getReadingCount(): Promise<{ count: number }> {
    const response = await apiClient.get('/sensorreadings/count');
    return response.data;
  }

  static async createSensorReading(reading: Omit<SensorReading, 'id' | 'timestamp'>): Promise<SensorReading> {
    const response = await apiClient.post('/sensorreadings', reading);
    return response.data;
  }

  // Sensor Statistics
  static async getSensorStatistics(sensorId: number): Promise<SensorStatistics> {
    const response = await apiClient.get(`/sensors/${sensorId}/statistics`);
    return response.data;
  }

  // Alerts
  static async getAlerts(
    page: number = 1,
    pageSize: number = 50,
    severity?: string,
    isResolved?: boolean
  ): Promise<Alert[]> {
    const params = new URLSearchParams({
      page: page.toString(),
      pageSize: pageSize.toString(),
    });
    
    if (severity) params.append('severity', severity);
    if (isResolved !== undefined) params.append('isResolved', isResolved.toString());

    const response = await apiClient.get(`/alerts?${params.toString()}`);
    return response.data;
  }

  static async getAlert(id: number): Promise<Alert> {
    const response = await apiClient.get(`/alerts/${id}`);
    return response.data;
  }

  static async resolveAlert(id: number): Promise<void> {
    await apiClient.put(`/alerts/${id}/resolve`);
  }

  static async deleteAlert(id: number): Promise<void> {
    await apiClient.delete(`/alerts/${id}`);
  }

  static async getAlertStatistics(): Promise<AlertStatistics> {
    const response = await apiClient.get('/alerts/statistics');
    return response.data;
  }
}

export default ApiService;


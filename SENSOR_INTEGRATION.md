# Sensor Integration Documentation

## Overview
This document describes the DHT22 sensor integration for the Student Housing Management System.

## Backend Components

### 1. SensorReading Model
Location: `HouseApp.API/Models/SensorReading.cs`
- Stores temperature and humidity readings
- Linked to a house
- Includes timestamp and device identifier

### 2. SensorsController
Location: `HouseApp.API/Controllers/SensorsController.cs`

**Endpoints:**
- `POST /api/sensors/readings` - Submit sensor data (requires authentication)
- `GET /api/sensors/house/{houseId}/latest` - Get latest reading for a house
- `GET /api/sensors/house/{houseId}/history` - Get last 24 hours of readings

**SignalR Integration:**
- Broadcasts sensor readings to house members via `ReceiveSensorReading` event

### 3. ArduinoSensorService (Optional)
Location: `HouseApp.API/Services/ArduinoSensorService.cs`

Background service that reads sensor data directly from an Arduino connected via serial port.

**Configuration (appsettings.json):**
```json
"Arduino": {
  "Enabled": false,
  "PortName": "COM3",
  "BaudRate": 115200,
  "HouseId": 1,
  "DeviceId": "ARDUINO_DHT22_01"
}
```

Set `Enabled` to `true` to activate the service.

## Frontend Components

### 1. SensorService
Location: `HouseApp/Services/SensorService.cs`
- Listens for real-time sensor updates via SignalR
- Integrated with ChatService

### 2. HomeViewModel
Location: `HouseApp/ViewModels/HomeViewModel.cs`
- Displays temperature and humidity
- Loads latest sensor data on initialization
- Updates in real-time when new readings arrive

### 3. HomePage UI
Location: `HouseApp/Views/HomePage.xaml`
- Purple floating card in bottom-right corner
- Shows temperature (°C) and humidity (%)
- Uses emoji indicators (🌡️ and 💧)

## Arduino Code

```cpp
#include <DHT.h>

#define DHTPIN 4
#define DHTTYPE DHT22
DHT dht(DHTPIN, DHTTYPE);

void setup() {
  Serial.begin(115200);
  delay(2000);
  dht.begin();
}

void loop() {
  delay(2500);

  float h = dht.readHumidity();
  float t = dht.readTemperature();
  if (isnan(h) || isnan(t)) return;

  Serial.print("{\"tempC\":");
  Serial.print(t, 1);
  Serial.print(",\"humidity\":");
  Serial.print(h, 1);
  Serial.println("}");
}
```

## Testing

### Manual API Testing
```bash
# Post a sensor reading
curl -X POST http://localhost:5199/api/sensors/readings \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -d '{
    "houseId": 1,
    "tempC": 22.5,
    "humidity": 45.0,
    "deviceId": "ARDUINO_DHT22_01"
  }'

# Get latest reading
curl -X GET http://localhost:5199/api/sensors/house/1/latest \
  -H "Authorization: Bearer YOUR_TOKEN"

# Get 24-hour history
curl -X GET http://localhost:5199/api/sensors/house/1/history \
  -H "Authorization: Bearer YOUR_TOKEN"
```

### Using the Arduino Service
1. Connect DHT22 sensor to Arduino (data pin 4)
2. Upload the Arduino code
3. Connect Arduino to computer via USB
4. Update `appsettings.json` with correct port name
5. Set `Arduino:Enabled` to `true`
6. Start the API - sensor data will be automatically read and stored

## Bug Fixes Implemented

### 1. Tenant Removal Feature
- Added tenant list display to HouseManagementPage.xaml
- Added "Remove" button for each tenant
- Added confirmation dialog before removal
- Backend endpoint already existed and works correctly

### 2. Landlord Logout Fix
- Added proper exception handling to logout flow
- Disconnects SignalR before clearing session
- Ensures proper navigation to login page
- Falls back gracefully on errors

## Testing Checklist

- [x] Backend API compiles successfully
- [x] MAUI app compiles successfully
- [x] Sensor POST endpoint works
- [x] Sensor GET latest endpoint works
- [x] Sensor GET history endpoint works
- [ ] SignalR broadcasts sensor updates (requires running app)
- [ ] Sensor display appears on HomePage (requires running app)
- [ ] Tenant removal button appears and works (requires running app)
- [ ] Logout works without exceptions (requires running app)

using Microsoft.AspNetCore.SignalR.Client;
using HouseApp.DTOs;

namespace HouseApp.Services;

public class SensorService
{
    private HubConnection? _hubConnection;

    public event Action<SensorReadingDto>? SensorDataReceived;

    public void Initialize(HubConnection connection)
    {
        _hubConnection = connection;
        _hubConnection.On<SensorReadingDto>("ReceiveSensorReading", OnSensorDataReceived);
        System.Diagnostics.Debug.WriteLine("SensorService initialized and listening for sensor readings");
    }

    private void OnSensorDataReceived(SensorReadingDto data)
    {
        System.Diagnostics.Debug.WriteLine($"Sensor reading received: Temp={data.TempC}°C, Humidity={data.Humidity}%");
        SensorDataReceived?.Invoke(data);
    }
}

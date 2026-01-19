using System.IO.Ports;
using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using HouseApp.API.Data;
using HouseApp.API.Models;
using HouseApp.API.Hubs;
using HouseApp.API.DTOs;

namespace HouseApp.API.Services;

public class ArduinoSensorService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ArduinoSensorService> _logger;
    private readonly IConfiguration _configuration;
    private SerialPort? _serialPort;

    public ArduinoSensorService(
        IServiceProvider serviceProvider,
        ILogger<ArduinoSensorService> logger,
        IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Get configuration
        var portName = _configuration["Arduino:PortName"] ?? "COM3"; // Default to COM3 on Windows
        var baudRate = _configuration.GetValue<int>("Arduino:BaudRate", 115200);
        var houseId = _configuration.GetValue<int>("Arduino:HouseId", 1);
        var deviceId = _configuration["Arduino:DeviceId"] ?? "ARDUINO_DHT22_01";
        var enabled = _configuration.GetValue<bool>("Arduino:Enabled", false);

        if (!enabled)
        {
            _logger.LogInformation("Arduino sensor service is disabled. Set Arduino:Enabled to true in appsettings.json to enable.");
            return;
        }

        try
        {
            _serialPort = new SerialPort(portName, baudRate);
            _serialPort.Open();
            _logger.LogInformation($"Arduino sensor service started on port {portName}");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var json = _serialPort.ReadLine();
                    _logger.LogDebug($"Received data: {json}");

                    var data = JsonSerializer.Deserialize<SensorData>(json);
                    if (data == null || float.IsNaN(data.TempC) || float.IsNaN(data.Humidity))
                    {
                        continue;
                    }

                    using var scope = _serviceProvider.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<ChatHub>>();

                    var reading = new SensorReading
                    {
                        HouseId = houseId,
                        Temperature = (decimal)data.TempC,
                        Humidity = (decimal)data.Humidity,
                        Timestamp = DateTime.UtcNow,
                        DeviceId = deviceId
                    };

                    context.SensorReadings.Add(reading);
                    await context.SaveChangesAsync(stoppingToken);

                    _logger.LogInformation($"Saved sensor reading: {data.TempC}°C, {data.Humidity}%");

                    // Broadcast to house group via SignalR
                    await hubContext.Clients.Group($"House_{houseId}")
                        .SendAsync("ReceiveSensorReading", new SensorReadingDto
                        {
                            Id = reading.Id,
                            HouseId = reading.HouseId,
                            TempC = reading.Temperature,
                            Humidity = reading.Humidity,
                            Timestamp = reading.Timestamp,
                            DeviceId = reading.DeviceId
                        }, stoppingToken);
                }
                catch (TimeoutException)
                {
                    // No data available, continue
                    await Task.Delay(100, stoppingToken);
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning($"Failed to parse sensor data: {ex.Message}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing sensor data");
                    await Task.Delay(1000, stoppingToken);
                }
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogError(ex, $"Cannot access serial port {portName}. Make sure the port is not in use and you have permissions.");
        }
        catch (IOException ex)
        {
            _logger.LogError(ex, $"Serial port {portName} not found or cannot be opened.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Arduino sensor service error");
        }
        finally
        {
            _serialPort?.Close();
            _serialPort?.Dispose();
        }
    }

    public override void Dispose()
    {
        _serialPort?.Close();
        _serialPort?.Dispose();
        base.Dispose();
    }
}

class SensorData
{
    public float TempC { get; set; }
    public float Humidity { get; set; }
}

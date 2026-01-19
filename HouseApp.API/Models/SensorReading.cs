namespace HouseApp.API.Models;

public class SensorReading
{
    public int Id { get; set; }
    public int HouseId { get; set; }
    public decimal Temperature { get; set; } // Celsius
    public decimal Humidity { get; set; } // Percentage
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? DeviceId { get; set; }
    
    public House House { get; set; } = null!;
}

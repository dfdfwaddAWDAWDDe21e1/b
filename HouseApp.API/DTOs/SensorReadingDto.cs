namespace HouseApp.API.DTOs;

public class SensorReadingDto
{
    public int? Id { get; set; }
    public int HouseId { get; set; }
    public decimal TempC { get; set; }
    public decimal Humidity { get; set; }
    public DateTime Timestamp { get; set; }
    public string? DeviceId { get; set; }
}

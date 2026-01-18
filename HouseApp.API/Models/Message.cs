namespace HouseApp.API.Models;

public class Message
{
    public int Id { get; set; }
    public int HouseId { get; set; }
    public int SenderId { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public string MessageText { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public bool IsRead { get; set; } = false;

    public House House { get; set; } = null!;
}

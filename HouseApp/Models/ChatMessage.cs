namespace HouseApp.Models;

public class ChatMessage
{
    public int Id { get; set; }
    public int HouseId { get; set; }
    public int SenderId { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public string MessageText { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public bool IsRead { get; set; }
    public bool IsCurrentUser { get; set; }
}
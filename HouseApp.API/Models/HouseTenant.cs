namespace HouseApp.API.Models;

public class HouseTenant
{
    public int Id { get; set; }
    public int HouseId { get; set; }
    public int StudentId { get; set; }
    public DateTime JoinedDate { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;

    public House House { get; set; } = null!;
    public User Student { get; set; } = null!;
}

namespace HouseApp.Models;

public class HouseTenant
{
    public int Id { get; set; }
    public int HouseId { get; set; }
    public int StudentId { get; set; }
    public DateTime JoinedDate { get; set; }
    public bool IsActive { get; set; }
    public string StudentName { get; set; } = string.Empty;
}

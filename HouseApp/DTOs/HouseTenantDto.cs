namespace HouseApp.DTOs;

public class HouseTenantDto
{
    public int HouseId { get; set; }
    public string HouseName { get; set; } = string.Empty;
    public int StudentId { get; set; }
    public DateTime JoinedDate { get; set; }
}

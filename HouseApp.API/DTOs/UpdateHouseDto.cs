namespace HouseApp.API.DTOs;

public class UpdateHouseDto
{
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public decimal MonthlyRent { get; set; }
    public decimal UtilitiesCost { get; set; }
    public decimal WaterBillCost { get; set; }
    public int MaxOccupants { get; set; }
    public string? Password { get; set; }
}

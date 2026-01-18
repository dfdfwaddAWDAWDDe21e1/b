namespace HouseApp.API.DTOs;

public class HouseDto
{
    public int? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public int? LandlordId { get; set; }
    public decimal MonthlyRent { get; set; }
    public decimal UtilitiesCost { get; set; }
    public decimal WaterBillCost { get; set; }
    public int MaxOccupants { get; set; }
    public int? CurrentOccupants { get; set; }
    public DateTime? CreatedDate { get; set; }
    public string? HouseCode { get; set; }
    public string? Password { get; set; }
}

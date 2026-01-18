namespace HouseApp.Models;

public class House
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public int LandlordId { get; set; }
    public decimal MonthlyRent { get; set; }
    public decimal UtilitiesCost { get; set; }
    public decimal WaterBillCost { get; set; }
    public DateTime CreatedDate { get; set; }
    public int MaxOccupants { get; set; }
    public int CurrentOccupants { get; set; }
}

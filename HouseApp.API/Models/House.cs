namespace HouseApp.API.Models;

public class House
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public int LandlordId { get; set; }
    public decimal MonthlyRent { get; set; }
    public decimal UtilitiesCost { get; set; }
    public decimal WaterBillCost { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public int MaxOccupants { get; set; }
    public string? Password { get; set; }
    public string? HouseCode { get; set; }

    public User Landlord { get; set; } = null!;
    public ICollection<HouseTenant> HouseTenants { get; set; } = new List<HouseTenant>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public ICollection<Message> Messages { get; set; } = new List<Message>();
}

namespace HouseApp.API.Models;

public enum UserType
{
    Student,
    Landlord
}

public class User
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserType UserType { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public int Age { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public ICollection<House> Houses { get; set; } = new List<House>();
    public ICollection<HouseTenant> HouseTenants { get; set; } = new List<HouseTenant>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}

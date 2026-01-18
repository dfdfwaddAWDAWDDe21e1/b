namespace HouseApp.API.Models;

public enum PaymentType
{
    Rent,
    Utilities,
    Water
}

public enum PaymentStatus
{
    Pending,
    Completed,
    Failed
}

public class Payment
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public int HouseId { get; set; }
    public decimal Amount { get; set; }
    public PaymentType PaymentType { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public string? StripePaymentIntentId { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? PaidDate { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public User Student { get; set; } = null!;
    public House House { get; set; } = null!;
}

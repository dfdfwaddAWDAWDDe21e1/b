using HouseApp.API.Models;

namespace HouseApp.API.DTOs;

public class PaymentDto
{
    public int? Id { get; set; }
    public int StudentId { get; set; }
    public int HouseId { get; set; }
    public decimal Amount { get; set; }
    public PaymentType PaymentType { get; set; }
    public PaymentStatus Status { get; set; }
    public string? StripePaymentIntentId { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? PaidDate { get; set; }
    public DateTime? CreatedDate { get; set; }
}

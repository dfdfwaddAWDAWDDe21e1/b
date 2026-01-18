using HouseApp.API.Models;

namespace HouseApp.API.DTOs;

public class CreatePaymentIntentDto
{
    public int StudentId { get; set; }
    public int HouseId { get; set; }
    public decimal Amount { get; set; }
    public PaymentType PaymentType { get; set; }
    public DateTime DueDate { get; set; }
}

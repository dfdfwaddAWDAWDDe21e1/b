using HouseApp.API.DTOs;

namespace HouseApp.API.Services;

public interface IPaymentService
{
    Task<string> CreatePaymentIntentAsync(CreatePaymentIntentDto dto);
    Task<bool> ConfirmPaymentAsync(int paymentId, string paymentIntentId);
}

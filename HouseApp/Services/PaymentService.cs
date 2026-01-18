using HouseApp.Models;

namespace HouseApp.Services;

public class PaymentService
{
    private readonly ApiService _apiService;

    public PaymentService(ApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task<List<Payment>> GetStudentPaymentsAsync(int studentId)
    {
        var result = await _apiService.GetAsync<List<Payment>>($"/api/payments/student/{studentId}");
        return result ?? new List<Payment>();
    }

    public async Task<List<Payment>> GetHousePaymentsAsync(int houseId)
    {
        var result = await _apiService.GetAsync<List<Payment>>($"/api/payments/house/{houseId}");
        return result ?? new List<Payment>();
    }

    public async Task<string?> CreatePaymentIntentAsync(int studentId, int houseId, decimal amount, PaymentType paymentType)
    {
        var dto = new DTOs.CreatePaymentIntentDto
        {
            StudentId = studentId,
            HouseId = houseId,
            Amount = amount,
            PaymentType = paymentType,
            DueDate = DateTime.UtcNow.AddDays(30)
        };

        var response = await _apiService.PostAsync<DTOs.CreatePaymentIntentDto, Dictionary<string, string>>("/api/payments/create-intent", dto);
        return response?.GetValueOrDefault("clientSecret");
    }

    public async Task<bool> ConfirmPaymentAsync(int paymentId, string paymentIntentId)
    {
        return await _apiService.PostAsync($"/api/payments/{paymentId}/confirm", new { paymentIntentId });
    }
}

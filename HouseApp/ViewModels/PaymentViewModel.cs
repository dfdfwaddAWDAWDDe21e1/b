using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HouseApp.Models;
using HouseApp.Services;

namespace HouseApp.ViewModels;

public partial class PaymentViewModel : ObservableObject, IQueryAttributable
{
    private readonly PaymentService _paymentService;
    private readonly AuthService _authService;

    [ObservableProperty]
    private int paymentId;

    [ObservableProperty]
    private Payment? payment;

    [ObservableProperty]
    private string cardNumber = string.Empty;

    [ObservableProperty]
    private string expiryDate = string.Empty;

    [ObservableProperty]
    private string cvv = string.Empty;

    [ObservableProperty]
    private bool isProcessing;

    public PaymentViewModel(PaymentService paymentService, AuthService authService)
    {
        _paymentService = paymentService;
        _authService = authService;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.ContainsKey("paymentId"))
        {
            PaymentId = int.Parse(query["paymentId"].ToString()!);
            _ = LoadPaymentAsync();
        }
    }

    private async Task LoadPaymentAsync()
    {
        try
        {
            var studentId = await _authService.GetCurrentUserIdAsync();
            var payments = await _paymentService.GetStudentPaymentsAsync(studentId);
            Payment = payments.FirstOrDefault(p => p.Id == PaymentId);
        }
        catch (Exception ex)
        {
            await Application.Current!.MainPage!.DisplayAlert("Error", $"Failed to load payment: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    private async Task ProcessPaymentAsync()
    {
        if (Payment == null) return;

        if (string.IsNullOrWhiteSpace(CardNumber) || string.IsNullOrWhiteSpace(ExpiryDate) || string.IsNullOrWhiteSpace(Cvv))
        {
            await Application.Current!.MainPage!.DisplayAlert("Error", "Please fill all card details", "OK");
            return;
        }

        IsProcessing = true;

        try
        {
            var studentId = await _authService.GetCurrentUserIdAsync();
            var clientSecret = await _paymentService.CreatePaymentIntentAsync(
                studentId, Payment.HouseId, Payment.Amount, Payment.PaymentType);

            if (!string.IsNullOrEmpty(clientSecret))
            {
                await Task.Delay(2000);

                var success = await _paymentService.ConfirmPaymentAsync(Payment.Id, "simulated_intent_id");

                if (success)
                {
                    await Application.Current!.MainPage!.DisplayAlert("Success", "Payment processed successfully!", "OK");
                    await Shell.Current.GoToAsync("..");
                }
                else
                {
                    await Application.Current!.MainPage!.DisplayAlert("Error", "Payment failed", "OK");
                }
            }
            else
            {
                await Application.Current!.MainPage!.DisplayAlert("Error", "Failed to initialize payment", "OK");
            }
        }
        catch (Exception ex)
        {
            await Application.Current!.MainPage!.DisplayAlert("Error", $"Payment error: {ex.Message}", "OK");
        }
        finally
        {
            IsProcessing = false;
        }
    }
}

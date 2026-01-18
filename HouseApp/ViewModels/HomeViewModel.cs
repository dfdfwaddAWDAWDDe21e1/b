using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HouseApp.Models;
using HouseApp.Services;
using System.Collections.ObjectModel;

namespace HouseApp.ViewModels;

public partial class HomeViewModel : ObservableObject
{
    private readonly AuthService _authService;
    private readonly HouseService _houseService;
    private readonly PaymentService _paymentService;

    [ObservableProperty]
    private string dayName = DateTime.Now.ToString("dddd");

    [ObservableProperty]
    private string fullDate = DateTime.Now.ToString("d MMMM yyyy");

    [ObservableProperty]
    private House? currentHouse;

    [ObservableProperty]
    private ObservableCollection<Payment> payments = new();

    [ObservableProperty]
    private Payment? nextPayment;

    // Notify UI when NextPayment changes
    partial void OnNextPaymentChanged(Payment? value)
    {
        OnPropertyChanged(nameof(HasNextPayment));
        OnPropertyChanged(nameof(HasNoNextPayment));
        OnPropertyChanged(nameof(NextPaymentAmount));
        OnPropertyChanged(nameof(NextPaymentStatus));
        OnPropertyChanged(nameof(NextPaymentDueDate));
    }

    [ObservableProperty]
    private decimal totalDue;

    [ObservableProperty]
    private bool isLoading;

    // Helper properties for UI bindings
    public bool HasNextPayment => NextPayment != null;
    public bool HasNoNextPayment => NextPayment == null;
    public string NextPaymentAmount => NextPayment != null ? $"€{NextPayment.Amount:N2}" : "€0.00";
    public string NextPaymentStatus => NextPayment?.Status.ToString() ?? "No Payment";
    public string NextPaymentDueDate => NextPayment != null ? $"Due: {NextPayment.DueDate:MMM dd}" : "No due date";

    public HomeViewModel(AuthService authService, HouseService houseService, PaymentService paymentService)
    {
        _authService = authService;
        _houseService = houseService;
        _paymentService = paymentService;
    }

    public async Task InitializeAsync()
    {
        await LoadDataAsync();
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        IsLoading = true;

        try
        {
            var studentId = await _authService.GetCurrentUserIdAsync();
            
            CurrentHouse = await _houseService.GetStudentHouseAsync(studentId);
            
            var paymentsList = await _paymentService.GetStudentPaymentsAsync(studentId);
            Payments.Clear();
            foreach (var payment in paymentsList.Where(p => p.Status == PaymentStatus.Pending))
            {
                Payments.Add(payment);
            }

            NextPayment = Payments.OrderBy(p => p.DueDate).FirstOrDefault();
            TotalDue = Payments.Sum(p => p.Amount);
        }
        catch (Exception ex)
        {
            await Application.Current!.MainPage!.DisplayAlert("Error", $"Failed to load data: {ex.Message}", "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task NavigateToPayment()
    {
        if (NextPayment != null)
        {
            await Shell.Current.GoToAsync($"payment?paymentId={NextPayment.Id}");
        }
    }
}

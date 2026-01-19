using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HouseApp.DTOs;
using HouseApp.Models;
using HouseApp.Services;
using System.Collections.ObjectModel;

namespace HouseApp.ViewModels;

public partial class HomeViewModel : ObservableObject, IDisposable
{
    private readonly AuthService _authService;
    private readonly HouseService _houseService;
    private readonly PaymentService _paymentService;
    private readonly SensorService _sensorService;
    private readonly ApiService _apiService;
    private bool _disposed;

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

    // Sensor properties
    [ObservableProperty]
    private decimal temperature;

    [ObservableProperty]
    private decimal humidity;

    // Notify UI when NextPayment changes
    partial void OnNextPaymentChanged(Payment? value)
    {
        OnPropertyChanged(nameof(HasNextPayment));
        OnPropertyChanged(nameof(HasNoNextPayment));
        OnPropertyChanged(nameof(NextPaymentAmount));
        OnPropertyChanged(nameof(NextPaymentStatus));
        OnPropertyChanged(nameof(NextPaymentDueDate));
        OnPropertyChanged(nameof(DaysUntilDue));
        OnPropertyChanged(nameof(IsOverdue));
        OnPropertyChanged(nameof(DaysUntilDueText));
        OnPropertyChanged(nameof(StatusColor));
        OnPropertyChanged(nameof(StatusText));
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
    
    // New computed properties for days until due
    public int DaysUntilDue => NextPayment != null 
        ? (NextPayment.DueDate.Date - DateTime.Now.Date).Days 
        : 0;

    public bool IsOverdue => NextPayment != null 
        && NextPayment.DueDate.Date < DateTime.Now.Date 
        && NextPayment.Status == PaymentStatus.Pending;

    public string DaysUntilDueText => NextPayment == null 
        ? "" 
        : IsOverdue 
            ? $"Overdue by {Math.Abs(DaysUntilDue)} days" 
            : DaysUntilDue == 0 
                ? "Due today" 
                : $"Due in {DaysUntilDue} days";

    public Color StatusColor => NextPayment == null 
        ? Colors.Gray 
        : NextPayment.Status == PaymentStatus.Completed 
            ? Color.FromArgb("#10B981") // Green
            : IsOverdue 
                ? Color.FromArgb("#EF4444") // Red
                : Color.FromArgb("#F59E0B"); // Orange/Yellow

    public string StatusText => NextPayment == null 
        ? "No Payment" 
        : NextPayment.Status == PaymentStatus.Completed 
            ? "Paid" 
            : IsOverdue 
                ? "Overdue" 
                : "Pending";

    public HomeViewModel(AuthService authService, HouseService houseService, PaymentService paymentService, SensorService sensorService, ApiService apiService)
    {
        _authService = authService;
        _houseService = houseService;
        _paymentService = paymentService;
        _sensorService = sensorService;
        _apiService = apiService;
    }

    public async Task InitializeAsync()
    {
        await LoadDataAsync();
        await LoadLatestSensorData();
        
        _sensorService.SensorDataReceived += OnSensorDataReceived;
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

    private async Task LoadLatestSensorData()
    {
        try
        {
            if (CurrentHouse == null || CurrentHouse.Id == 0)
            {
                System.Diagnostics.Debug.WriteLine("No current house, skipping sensor data load");
                return;
            }

            var reading = await _apiService.GetAsync<SensorReadingDto>(
                $"/api/sensors/house/{CurrentHouse.Id}/latest");

            if (reading != null)
            {
                Temperature = reading.TempC;
                Humidity = reading.Humidity;
                System.Diagnostics.Debug.WriteLine($"Loaded sensor data: Temp={Temperature}°C, Humidity={Humidity}%");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load sensor data: {ex.Message}");
            // Don't show error to user - sensor data is optional
        }
    }

    private void OnSensorDataReceived(SensorReadingDto data)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            Temperature = data.TempC;
            Humidity = data.Humidity;
            System.Diagnostics.Debug.WriteLine($"Sensor data updated: Temp={Temperature}°C, Humidity={Humidity}%");
        });
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _sensorService.SensorDataReceived -= OnSensorDataReceived;
            _disposed = true;
        }
    }
}

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HouseApp.Models;
using HouseApp.Services;
using HouseApp.DTOs;
using System.Collections.ObjectModel;

namespace HouseApp.ViewModels;

public partial class LandlordDashboardViewModel : ObservableObject
{
    private readonly ApiService _apiService;

    [ObservableProperty]
    private ObservableCollection<House> houses = new();

    [ObservableProperty]
    private decimal totalMonthlyIncome;

    [ObservableProperty]
    private int totalProperties;

    [ObservableProperty]
    private int totalTenants;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private bool isRefreshing;

    [ObservableProperty]
    private bool isEmpty;

    public LandlordDashboardViewModel(ApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task InitializeAsync()
    {
        await LoadHousesAsync();
    }

    [RelayCommand]
    private async Task LoadHousesAsync()
    {
        try
        {
            IsLoading = true;
            IsRefreshing = true;

            var userId = await SecureStorage.GetAsync(Constants.UserIdKey);
            if (string.IsNullOrEmpty(userId))
            {
                await Application.Current!.MainPage!.DisplayAlert("Error", "User not logged in", "OK");
                return;
            }

            System.Diagnostics.Debug.WriteLine($"Loading houses for landlord ID: {userId}");

            // Get landlord's houses from the API
            var houseDtoList = await _apiService.GetAsync<List<HouseDto>>("/api/houses/my-houses");

            if (houseDtoList != null && houseDtoList.Any())
            {
                System.Diagnostics.Debug.WriteLine($"Loaded {houseDtoList.Count} houses");
                
                Houses.Clear();
                foreach (var houseDto in houseDtoList)
                {
                    var house = new House
                    {
                        Id = houseDto.Id ?? 0,
                        Name = houseDto.Name,
                        Address = houseDto.Address,
                        LandlordId = houseDto.LandlordId ?? 0,
                        MonthlyRent = houseDto.MonthlyRent,
                        UtilitiesCost = houseDto.UtilitiesCost,
                        WaterBillCost = houseDto.WaterBillCost,
                        MaxOccupants = houseDto.MaxOccupants,
                        CurrentOccupants = houseDto.CurrentOccupants ?? 0,
                        CreatedDate = houseDto.CreatedDate ?? DateTime.UtcNow
                    };
                    
                    System.Diagnostics.Debug.WriteLine($"House: {house.Name} - {house.Address} - Occupants: {house.CurrentOccupants}");
                    Houses.Add(house);
                }

                IsEmpty = false;
                CalculateStats();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("No houses found");
                Houses.Clear();
                IsEmpty = true;
                TotalProperties = 0;
                TotalTenants = 0;
                TotalMonthlyIncome = 0;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading houses: {ex.Message}");
            await Application.Current!.MainPage!.DisplayAlert("Error", $"Failed to load houses: {ex.Message}", "OK");
            Houses.Clear();
            IsEmpty = true;
        }
        finally
        {
            IsLoading = false;
            IsRefreshing = false;
        }
    }

    private void CalculateStats()
    {
        TotalProperties = Houses.Count;
        TotalTenants = Houses.Sum(h => h.CurrentOccupants);
        TotalMonthlyIncome = Houses.Sum(h => h.MonthlyRent * h.CurrentOccupants);
    }

    [RelayCommand]
    private async Task NavigateToHouseManagement(House house)
    {
        if (house == null)
        {
            await Application.Current!.MainPage!.DisplayAlert("Error", "Invalid house selected", "OK");
            return;
        }

        try
        {
            // Use proper navigation with parameters
            await Shell.Current.GoToAsync($"housemanagement?HouseId={house.Id}");
        }
        catch (Exception ex)
        {
            await Application.Current!.MainPage!.DisplayAlert("Error", $"Failed to open house management: {ex.Message}", "OK");
            System.Diagnostics.Debug.WriteLine($"Navigation error: {ex}");
        }
    }

    [RelayCommand]
    private async Task AddHouseAsync()
    {
        await Shell.Current.GoToAsync("housemanagement");
    }

    [RelayCommand]
    private async Task Refresh()
    {
        await LoadHousesAsync();
    }
}

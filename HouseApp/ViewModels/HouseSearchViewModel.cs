using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HouseApp.Services;
using HouseApp.Models;
using System.Collections.ObjectModel;

namespace HouseApp.ViewModels;

public partial class HouseSearchViewModel : ObservableObject
{
    private readonly ApiService _apiService;

    [ObservableProperty]
    private string houseCode = string.Empty;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private bool isEmpty;

    [ObservableProperty]
    private ObservableCollection<HouseWithPasswordModel> availableHouses = new();

    public HouseSearchViewModel(ApiService apiService)
    {
        _apiService = apiService;
        _ = LoadAvailableHousesAsync();
    }

    private async Task LoadAvailableHousesAsync()
    {
        try
        {
            IsLoading = true;
            var houses = await _apiService.GetAsync<List<House>>("/api/houses/available");
            
            if (houses != null && houses.Any())
            {
                AvailableHouses = new ObservableCollection<HouseWithPasswordModel>(
                    houses.Select(h => new HouseWithPasswordModel
                    {
                        Id = h.Id,
                        Name = h.Name,
                        Address = h.Address,
                        MonthlyRent = h.MonthlyRent,
                        UtilitiesCost = h.UtilitiesCost,
                        WaterBillCost = h.WaterBillCost,
                        MaxOccupants = h.MaxOccupants,
                        CurrentOccupants = h.CurrentOccupants,
                        AvailableSpots = h.MaxOccupants - h.CurrentOccupants,
                        Password = string.Empty
                    }));
                IsEmpty = false;
            }
            else
            {
                IsEmpty = true;
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Failed to load houses: {ex.Message}", "OK");
            IsEmpty = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task JoinHouse()
    {
        if (string.IsNullOrWhiteSpace(HouseCode))
        {
            await Shell.Current.DisplayAlert("Error", "Please enter a house code", "OK");
            return;
        }

        try
        {
            IsLoading = true;

            var userId = int.Parse(await SecureStorage.GetAsync(Constants.UserIdKey) ?? "0");
            var response = await _apiService.PostAsync<object, object>($"/api/houses/join", new
            {
                StudentId = userId,
                HouseCode = HouseCode.Trim()
            });

            if (response != null)
            {
                await Shell.Current.DisplayAlert("Success", "You've joined the house!", "OK");
                await Shell.Current.GoToAsync("///tabs/home");
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Failed to join house: {ex.Message}", "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task JoinSpecificHouse(HouseWithPasswordModel house)
    {
        if (string.IsNullOrWhiteSpace(house.Password))
        {
            await Shell.Current.DisplayAlert("Error", "Please enter the house password", "OK");
            return;
        }

        try
        {
            IsLoading = true;

            var userId = int.Parse(await SecureStorage.GetAsync(Constants.UserIdKey) ?? "0");
            var response = await _apiService.PostAsync<object, object>($"/api/houses/{house.Id}/join", new
            {
                StudentId = userId,
                Password = house.Password
            });

            if (response != null)
            {
                await Shell.Current.DisplayAlert("Success", $"You've joined {house.Name}!", "OK");
                await Shell.Current.GoToAsync("///tabs/home");
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Incorrect password or house is full", "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }
}

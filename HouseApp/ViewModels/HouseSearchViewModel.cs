using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HouseApp.Services;
using HouseApp.Models;

namespace HouseApp.ViewModels;

public partial class HouseSearchViewModel : ObservableObject
{
    private readonly ApiService _apiService;

    [ObservableProperty]
    private string houseCode = string.Empty;

    [ObservableProperty]
    private bool isLoading;

    public HouseSearchViewModel(ApiService apiService)
    {
        _apiService = apiService;
    }

    [RelayCommand]
    private async Task JoinHouse()
    {
        if (string.IsNullOrWhiteSpace(HouseCode))
        {
            await Shell.Current.DisplayAlert("Error", "Please enter a house code", "OK");
            return;
        }

        // Trim and validate code format (6 characters alphanumeric)
        var trimmedCode = HouseCode.Trim().ToUpper();
        if (trimmedCode.Length != 6)
        {
            await Shell.Current.DisplayAlert("Error", "House code must be exactly 6 characters", "OK");
            return;
        }

        try
        {
            IsLoading = true;

            var userId = int.Parse(await SecureStorage.GetAsync(Constants.UserIdKey) ?? "0");
            var response = await _apiService.PostAsync<object, object>($"/api/houses/join", new
            {
                StudentId = userId,
                HouseCode = trimmedCode
            });

            if (response != null)
            {
                await Shell.Current.DisplayAlert("Success", "You've joined the house!", "OK");
                await Shell.Current.GoToAsync("///tabs/home");
            }
        }
        catch (Exception ex)
        {
            // Extract meaningful error message from exception
            var message = ex.Message;
            if (message.Contains("Invalid house code"))
            {
                await Shell.Current.DisplayAlert("Error", "Invalid house code. Please check and try again.", "OK");
            }
            else if (message.Contains("House is full"))
            {
                await Shell.Current.DisplayAlert("Error", "This house is already full. Please contact your landlord.", "OK");
            }
            else if (message.Contains("already in a house"))
            {
                await Shell.Current.DisplayAlert("Error", "You are already in a house. Leave your current house first.", "OK");
            }
            else
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to join house: {message}", "OK");
            }
        }
        finally
        {
            IsLoading = false;
        }
    }
}

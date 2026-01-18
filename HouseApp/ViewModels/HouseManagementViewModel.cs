using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HouseApp.DTOs;
using HouseApp.Models;
using HouseApp.Services;
using System.Collections.ObjectModel;

namespace HouseApp.ViewModels;

public partial class HouseManagementViewModel : ObservableObject
{
    private readonly ApiService _apiService;

    [ObservableProperty]
    private int houseId;

    [ObservableProperty]
    private string houseName = string.Empty;

    [ObservableProperty]
    private string address = string.Empty;

    [ObservableProperty]
    private decimal monthlyRent;

    [ObservableProperty]
    private decimal utilitiesCost;

    [ObservableProperty]
    private decimal waterBillCost;

    [ObservableProperty]
    private int maxOccupants = 1;

    [ObservableProperty]
    private string? housePassword;

    [ObservableProperty]
    private string? houseCode;

    [ObservableProperty]
    private ObservableCollection<TenantDto> tenants = new();

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private bool isEditMode;

    [ObservableProperty]
    private string studentEmail = string.Empty;

    public HouseManagementViewModel(ApiService apiService)
    {
        _apiService = apiService;
    }

    [RelayCommand]
    private async Task LoadHouseDetails()
    {
        if (HouseId <= 0) return;

        try
        {
            IsLoading = true;

            var house = await _apiService.GetAsync<HouseDto>($"/api/houses/{HouseId}");
            
            if (house != null)
            {
                HouseName = house.Name;
                Address = house.Address;
                MonthlyRent = house.MonthlyRent;
                UtilitiesCost = house.UtilitiesCost;
                WaterBillCost = house.WaterBillCost;
                MaxOccupants = house.MaxOccupants;
                HousePassword = house.Password;
                HouseCode = house.HouseCode;
            }

            await LoadTenants();
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Failed to load house: {ex.Message}", "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task LoadTenants()
    {
        if (HouseId <= 0) return;

        try
        {
            var tenantsList = await _apiService.GetAsync<List<TenantDto>>($"/api/houses/{HouseId}/tenants");
            
            Tenants.Clear();
            if (tenantsList != null)
            {
                foreach (var tenant in tenantsList)
                {
                    Tenants.Add(tenant);
                }
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Failed to load tenants: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    private async Task SaveHouse()
    {
        if (string.IsNullOrWhiteSpace(HouseName) || string.IsNullOrWhiteSpace(Address))
        {
            await Shell.Current.DisplayAlert("Error", "Please fill all required fields", "OK");
            return;
        }

        // Validate positive numbers
        if (MonthlyRent < 0)
        {
            await Shell.Current.DisplayAlert("Error", "Monthly rent must be a positive number", "OK");
            return;
        }
        if (UtilitiesCost < 0)
        {
            await Shell.Current.DisplayAlert("Error", "Utilities cost must be a positive number", "OK");
            return;
        }
        if (WaterBillCost < 0)
        {
            await Shell.Current.DisplayAlert("Error", "Water bill cost must be a positive number", "OK");
            return;
        }
        if (MaxOccupants < 1)
        {
            await Shell.Current.DisplayAlert("Error", "Maximum occupants must be at least 1", "OK");
            return;
        }

        try
        {
            IsLoading = true;

            var updateDto = new UpdateHouseDto
            {
                Name = HouseName,
                Address = Address,
                MonthlyRent = MonthlyRent,
                UtilitiesCost = UtilitiesCost,
                WaterBillCost = WaterBillCost,
                MaxOccupants = MaxOccupants,
                Password = HousePassword
            };

            if (HouseId > 0)
            {
                // Update existing house
                await _apiService.PutAsync($"/api/houses/{HouseId}", updateDto);
                await Shell.Current.DisplayAlert("Success", "House updated successfully", "OK");
                IsEditMode = false; // Exit edit mode after successful save
            }
            else
            {
                // Create new house - use HouseDto for creation
                var houseDto = new HouseDto
                {
                    Name = HouseName,
                    Address = Address,
                    MonthlyRent = MonthlyRent,
                    UtilitiesCost = UtilitiesCost,
                    WaterBillCost = WaterBillCost,
                    MaxOccupants = MaxOccupants,
                    Password = HousePassword
                };
                
                var createdHouse = await _apiService.PostAsync<HouseDto, HouseDto>("/api/houses", houseDto);
                if (createdHouse != null && createdHouse.Id.HasValue)
                {
                    HouseId = createdHouse.Id.Value;
                    HouseCode = createdHouse.HouseCode;
                    await Shell.Current.DisplayAlert("Success", "House created successfully", "OK");
                    await Shell.Current.GoToAsync("///tabs/dashboard");
                }
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Failed to save house: {ex.Message}", "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task DeleteHouse()
    {
        if (HouseId <= 0) return;

        var confirm = await Shell.Current.DisplayAlert(
            "Confirm Delete", 
            "Are you sure you want to delete this house?", 
            "Yes", 
            "No");

        if (!confirm) return;

        try
        {
            IsLoading = true;

            var success = await _apiService.DeleteAsync($"/api/houses/{HouseId}");

            if (success)
            {
                await Shell.Current.DisplayAlert("Success", "House deleted successfully", "OK");
                await Shell.Current.GoToAsync("///tabs/dashboard");
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Failed to delete house: {ex.Message}", "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task CopyHouseCode()
    {
        if (!string.IsNullOrEmpty(HouseCode))
        {
            await Clipboard.SetTextAsync(HouseCode);
            await Shell.Current.DisplayAlert("Copied", "House code copied to clipboard", "OK");
        }
    }

    [RelayCommand]
    private async Task AddTenant()
    {
        if (string.IsNullOrWhiteSpace(StudentEmail))
        {
            await Shell.Current.DisplayAlert("Error", "Please enter student email", "OK");
            return;
        }

        try
        {
            IsLoading = true;

            // Find student by email
            var student = await _apiService.GetAsync<UserDto>($"/api/auth/users/search?email={StudentEmail}");

            if (student == null || student.UserType != UserType.Student)
            {
                await Shell.Current.DisplayAlert("Error", "Student not found", "OK");
                return;
            }

            // Add tenant
            var success = await _apiService.PostAsync($"/api/houses/{HouseId}/tenants/{student.Id}", (object?)null);

            if (success)
            {
                await Shell.Current.DisplayAlert("Success", $"Added {student.FirstName} {student.LastName} to house", "OK");
                StudentEmail = string.Empty;
                await LoadTenants();
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Failed to add tenant: {ex.Message}", "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task RemoveTenant(int tenantId)
    {
        try
        {
            IsLoading = true;

            var success = await _apiService.DeleteAsync($"/api/houses/{HouseId}/tenants/{tenantId}");

            if (success)
            {
                await Shell.Current.DisplayAlert("Success", "Tenant removed from house", "OK");
                await LoadTenants();
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Failed to remove tenant: {ex.Message}", "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void ToggleEditMode()
    {
        IsEditMode = !IsEditMode;
    }
}
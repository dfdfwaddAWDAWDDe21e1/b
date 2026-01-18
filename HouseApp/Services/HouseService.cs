using HouseApp.Models;
using HouseApp.DTOs;

namespace HouseApp.Services;

public class HouseService
{
    private readonly ApiService _apiService;

    public HouseService(ApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task<List<House>> GetLandlordHousesAsync(int landlordId)
    {
        var result = await _apiService.GetAsync<List<House>>($"/api/houses/landlord/{landlordId}");
        return result ?? new List<House>();
    }

    public async Task<House?> GetHouseByIdAsync(int houseId)
    {
        return await _apiService.GetAsync<House>($"/api/houses/{houseId}");
    }

    public async Task<House?> GetStudentHouseAsync(int studentId)
    {
        var houseTenantDto = await _apiService.GetAsync<HouseTenantDto>($"/api/houses/student/{studentId}/house");
        
        if (houseTenantDto != null)
        {
            // Convert the DTO to a House object for backward compatibility
            return new House
            {
                Id = houseTenantDto.HouseId,
                Name = houseTenantDto.HouseName
            };
        }
        
        return null;
    }

    public async Task<List<HouseTenant>> GetHouseTenantsAsync(int houseId)
    {
        var result = await _apiService.GetAsync<List<HouseTenant>>($"/api/houses/{houseId}/tenants");
        return result ?? new List<HouseTenant>();
    }

    public async Task<bool> CreateHouseAsync(House house)
    {
        var result = await _apiService.PostAsync<House, House>("/api/houses", house);
        return result != null;
    }

    public async Task<bool> UpdateHouseAsync(int houseId, House house)
    {
        var result = await _apiService.PutAsync<House, House>($"/api/houses/{houseId}", house);
        return result != null;
    }

    public async Task<bool> DeleteHouseAsync(int houseId)
    {
        return await _apiService.DeleteAsync($"/api/houses/{houseId}");
    }

    public async Task<bool> AddTenantAsync(int houseId, int studentId)
    {
        return await _apiService.PostAsync($"/api/houses/{houseId}/tenants/{studentId}", new { });
    }

    public async Task<bool> RemoveTenantAsync(int houseId, int studentId)
    {
        return await _apiService.DeleteAsync($"/api/houses/{houseId}/tenants/{studentId}");
    }
}

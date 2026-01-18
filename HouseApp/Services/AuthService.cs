using HouseApp.DTOs;
using HouseApp.Models;

namespace HouseApp.Services;

public class AuthService
{
    private readonly ApiService _apiService;

    public AuthService(ApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task<(bool Success, string Message, LoginResponseDto? Data)> RegisterAsync(RegisterDto registerDto)
    {
        try
        {
            var response = await _apiService.PostAsync<RegisterDto, LoginResponseDto>("/api/auth/register", registerDto);
            
            if (response != null)
            {
                await SaveUserDataAsync(response);
                return (true, "Registration successful", response);
            }
            
            return (false, "Registration failed", null);
        }
        catch (Exception ex)
        {
            return (false, ex.Message, null);
        }
    }

    public async Task<(bool Success, string Message, LoginResponseDto? Data)> LoginAsync(LoginDto loginDto)
    {
        try
        {
            var response = await _apiService.PostAsync<LoginDto, LoginResponseDto>("/api/auth/login", loginDto);
            
            if (response != null)
            {
                await SaveUserDataAsync(response);
                return (true, "Login successful", response);
            }
            
            return (false, "Invalid credentials", null);
        }
        catch (Exception ex)
        {
            return (false, ex.Message, null);
        }
    }

    private async Task SaveUserDataAsync(LoginResponseDto userData)
    {
        await SecureStorage.SetAsync(Constants.TokenKey, userData.Token);
        await SecureStorage.SetAsync(Constants.UserIdKey, userData.UserId.ToString());
        await SecureStorage.SetAsync(Constants.UserTypeKey, ((int)userData.UserType).ToString());
        await SecureStorage.SetAsync(Constants.UserEmailKey, userData.Email);
        await SecureStorage.SetAsync(Constants.UserFirstNameKey, userData.FirstName);
        await SecureStorage.SetAsync(Constants.UserLastNameKey, userData.LastName);
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        var token = await SecureStorage.GetAsync(Constants.TokenKey);
        return !string.IsNullOrEmpty(token);
    }

    public async Task<UserType> GetUserTypeAsync()
    {
        var userTypeStr = await SecureStorage.GetAsync(Constants.UserTypeKey);
        if (int.TryParse(userTypeStr, out int userType))
        {
            return (UserType)userType;
        }
        return UserType.Student;
    }

    public async Task<int> GetCurrentUserIdAsync()
    {
        var userIdStr = await SecureStorage.GetAsync(Constants.UserIdKey);
        if (int.TryParse(userIdStr, out int userId))
        {
            return userId;
        }
        return 0;
    }

    public async Task LogoutAsync()
    {
        SecureStorage.RemoveAll();
        await Task.CompletedTask;
    }
}

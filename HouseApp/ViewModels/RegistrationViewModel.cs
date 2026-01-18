using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HouseApp.DTOs;
using HouseApp.Models;
using HouseApp.Services;

namespace HouseApp.ViewModels;

public partial class RegistrationViewModel : ObservableObject
{
    private readonly AuthService _authService;
    private readonly UserSession _userSession;
    private readonly ApiService _apiService;

    [ObservableProperty]
    private string email = string.Empty;

    [ObservableProperty]
    private string password = string.Empty;

    [ObservableProperty]
    private string firstName = string.Empty;

    [ObservableProperty]
    private string lastName = string.Empty;

    [ObservableProperty]
    private string phoneNumber = string.Empty;

    [ObservableProperty]
    private DateTime dateOfBirth = DateTime.Now.AddYears(-20);

    public DateTime MaximumDateOfBirth => DateTime.Today;

    [ObservableProperty]
    private UserType selectedUserType = UserType.Student;

    private bool _isStudent = true; // Default to Student
    public bool IsStudent
    {
        get => _isStudent;
        set
        {
            if (SetProperty(ref _isStudent, value))
            {
                if (value) IsLandlord = false;
            }
        }
    }

    private bool _isLandlord;
    public bool IsLandlord
    {
        get => _isLandlord;
        set
        {
            if (SetProperty(ref _isLandlord, value))
            {
                if (value) IsStudent = false;
            }
        }
    }

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private bool isLoginMode = true;

    public RegistrationViewModel(AuthService authService, UserSession userSession, ApiService apiService)
    {
        _authService = authService;
        _userSession = userSession;
        _apiService = apiService;
    }

    private async Task<bool> CheckIfStudentInHouse(int studentId)
    {
        try
        {
            var houseTenant = await _apiService.GetAsync<HouseTenantDto>($"/api/houses/student/{studentId}/house");
            return houseTenant != null;
        }
        catch
        {
            return false;
        }
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            await Application.Current!.MainPage!.DisplayAlert("Error", "Please fill all fields", "OK");
            return;
        }

        IsLoading = true;

        try
        {
            var loginDto = new LoginDto { Email = Email, Password = Password };
            var result = await _authService.LoginAsync(loginDto);

            if (result.Success && result.Data != null)
            {
                _userSession.UserId = result.Data.UserId;
                _userSession.Email = result.Data.Email;
                _userSession.UserType = result.Data.UserType;
                _userSession.FirstName = result.Data.FirstName;
                _userSession.LastName = result.Data.LastName;

                // Store user type in SecureStorage
                await SecureStorage.SetAsync(Constants.UserTypeKey, result.Data.UserType.ToString());

                System.Diagnostics.Debug.WriteLine($"Login - User Type: {result.Data.UserType}");
                System.Diagnostics.Debug.WriteLine($"Login - User ID: {result.Data.UserId}");

                try
                {
                    // Switch to the appropriate shell for the user type
                    var app = Application.Current as App;
                    if (app != null)
                    {
                        app.SetShellForUserType(result.Data.UserType);

                        // Navigate based on user type
                        if (result.Data.UserType == UserType.Landlord)
                        {
                            System.Diagnostics.Debug.WriteLine("Navigating landlord to: ///tabs/dashboard");
                            await Shell.Current.GoToAsync("///tabs/dashboard");
                        }
                        else // Student
                        {
                            // Check if student is in a house
                            var isInHouse = await CheckIfStudentInHouse(result.Data.UserId);
                            System.Diagnostics.Debug.WriteLine($"Student is in house: {isInHouse}");
                            
                            if (isInHouse)
                            {
                                System.Diagnostics.Debug.WriteLine("Navigating student to: ///tabs/home");
                                await Shell.Current.GoToAsync("///tabs/home");
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine("Navigating student to: //housesearch");
                                await Shell.Current.GoToAsync("//housesearch");
                            }
                        }
                    }
                    else
                    {
                        await Application.Current!.MainPage!.DisplayAlert("Error", "Navigation setup failed", "OK");
                    }
                }
                catch (Exception navEx)
                {
                    System.Diagnostics.Debug.WriteLine($"Navigation error: {navEx.Message}");
                    System.Diagnostics.Debug.WriteLine($"Navigation stack trace: {navEx.StackTrace}");
                    await Application.Current!.MainPage!.DisplayAlert("Error", $"Navigation failed: {navEx.Message}", "OK");
                }
            }
            else
            {
                await Application.Current!.MainPage!.DisplayAlert("Error", result.Message, "OK");
            }
        }
        catch (Exception ex)
        {
            await Application.Current!.MainPage!.DisplayAlert("Error", $"Login failed: {ex.Message}", "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task RegisterAsync()
    {
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password) ||
            string.IsNullOrWhiteSpace(FirstName) || string.IsNullOrWhiteSpace(LastName) ||
            string.IsNullOrWhiteSpace(PhoneNumber))
        {
            await Application.Current!.MainPage!.DisplayAlert("Error", "Please fill all fields", "OK");
            return;
        }

        IsLoading = true;

        try
        {
            var registerDto = new RegisterDto
            {
                Email = Email,
                Password = Password,
                FirstName = FirstName,
                LastName = LastName,
                PhoneNumber = PhoneNumber,
                DateOfBirth = DateOfBirth,
                UserType = IsLandlord ? UserType.Landlord : UserType.Student // Use selected type
            };

            var result = await _authService.RegisterAsync(registerDto);

            if (result.Success && result.Data != null)
            {
                _userSession.UserId = result.Data.UserId;
                _userSession.Email = result.Data.Email;
                _userSession.UserType = result.Data.UserType;
                _userSession.FirstName = result.Data.FirstName;
                _userSession.LastName = result.Data.LastName;

                // Store user type in SecureStorage
                await SecureStorage.SetAsync(Constants.UserTypeKey, result.Data.UserType.ToString());

                System.Diagnostics.Debug.WriteLine($"Register - User Type: {result.Data.UserType}");
                System.Diagnostics.Debug.WriteLine($"Register - User ID: {result.Data.UserId}");

                try
                {
                    // Switch to the appropriate shell for the user type
                    var app = Application.Current as App;
                    if (app != null)
                    {
                        app.SetShellForUserType(result.Data.UserType);

                        // Navigate based on user type
                        if (result.Data.UserType == UserType.Landlord)
                        {
                            System.Diagnostics.Debug.WriteLine("Navigating landlord to: ///tabs/dashboard");
                            await Shell.Current.GoToAsync("///tabs/dashboard");
                        }
                        else // Student - always go to house search for new accounts
                        {
                            System.Diagnostics.Debug.WriteLine("Navigating new student to: //housesearch");
                            await Shell.Current.GoToAsync("//housesearch");
                        }
                    }
                    else
                    {
                        await Application.Current!.MainPage!.DisplayAlert("Error", "Navigation setup failed", "OK");
                    }
                }
                catch (Exception navEx)
                {
                    System.Diagnostics.Debug.WriteLine($"Navigation error: {navEx.Message}");
                    System.Diagnostics.Debug.WriteLine($"Navigation stack trace: {navEx.StackTrace}");
                    await Application.Current!.MainPage!.DisplayAlert("Error", $"Navigation failed: {navEx.Message}", "OK");
                }
            }
            else
            {
                await Application.Current!.MainPage!.DisplayAlert("Error", result.Message, "OK");
            }
        }
        catch (Exception ex)
        {
            await Application.Current!.MainPage!.DisplayAlert("Error", $"Registration failed: {ex.Message}", "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void ToggleMode()
    {
        IsLoginMode = !IsLoginMode;
    }
}

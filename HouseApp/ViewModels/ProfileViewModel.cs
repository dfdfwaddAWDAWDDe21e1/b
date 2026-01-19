using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HouseApp.Services;

namespace HouseApp.ViewModels;

public partial class ProfileViewModel : ObservableObject
{
    private readonly AuthService _authService;
    private readonly UserSession _userSession;
    private readonly IServiceProvider _serviceProvider;

    [ObservableProperty]
    private string firstName = string.Empty;

    [ObservableProperty]
    private string lastName = string.Empty;

    [ObservableProperty]
    private string email = string.Empty;

    [ObservableProperty]
    private string profilePictureUrl = string.Empty;

    [ObservableProperty]
    private bool isLoading;

    public ProfileViewModel(AuthService authService, UserSession userSession, IServiceProvider serviceProvider)
    {
        _authService = authService;
        _userSession = userSession;
        _serviceProvider = serviceProvider;
        LoadUserData();
    }

    private void LoadUserData()
    {
        FirstName = _userSession.FirstName;
        LastName = _userSession.LastName;
        Email = _userSession.Email;
    }

    [RelayCommand]
    private async Task PickProfilePictureAsync()
    {
        try
        {
            var result = await MediaPicker.PickPhotoAsync(new MediaPickerOptions
            {
                Title = "Pick a profile picture"
            });

            if (result != null)
            {
                ProfilePictureUrl = result.FullPath;
                await Application.Current!.MainPage!.DisplayAlert("Info", "Profile picture updated (UI only)", "OK");
            }
        }
        catch (Exception ex)
        {
            await Application.Current!.MainPage!.DisplayAlert("Error", $"Failed to pick image: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    private async Task LogoutAsync()
    {
        var confirm = await Application.Current!.MainPage!.DisplayAlert(
            "Confirm", "Are you sure you want to logout?", "Yes", "No");

        if (!confirm) return;

        try
        {
            // Get ChatService to disconnect SignalR
            var chatService = _serviceProvider.GetService<ChatService>();
            if (chatService != null)
            {
                try
                {
                    await chatService.DisconnectAsync();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error disconnecting chat: {ex.Message}");
                }
            }

            // Clear session and secure storage
            _userSession.Clear();
            await _authService.LogoutAsync();
            
            // Navigate to login page using AppShell
            var appShell = _serviceProvider.GetRequiredService<AppShell>();
            Application.Current.MainPage = appShell;
            
            // Navigate to login route
            await Shell.Current.GoToAsync("//login");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Logout error: {ex.Message}");
            
            // Force navigate to login even if error occurs
            try
            {
                var appShell = _serviceProvider.GetRequiredService<AppShell>();
                Application.Current.MainPage = appShell;
                await Shell.Current.GoToAsync("//login");
            }
            catch (Exception navEx)
            {
                System.Diagnostics.Debug.WriteLine($"Navigation error: {navEx.Message}");
                await Application.Current!.MainPage!.DisplayAlert("Error", "Please restart the application", "OK");
            }
        }
    }
}

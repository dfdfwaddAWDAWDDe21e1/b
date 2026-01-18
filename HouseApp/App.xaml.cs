using HouseApp.Models;

namespace HouseApp;

public partial class App : Application
{
    private readonly AppShell _shell;
    private readonly IServiceProvider _serviceProvider;

    public App(AppShell shell, IServiceProvider serviceProvider)
    {
        InitializeComponent();  
        _shell = shell;
        _serviceProvider = serviceProvider;
    }

    protected override Window CreateWindow(IActivationState? activationState)
        => new Window(_shell);

    public void SetShellForUserType(UserType userType)
    {
        try
        {
            Shell? newShell = userType == UserType.Landlord
                ? _serviceProvider.GetRequiredService<LandlordShell>()
                : _serviceProvider.GetRequiredService<StudentShell>();

            if (newShell != null)
            {
                MainPage = newShell;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Failed to create shell: newShell is null");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error setting shell for user type {userType}: {ex.Message}");
            // Fall back to keeping current shell if switching fails
        }
    }
}

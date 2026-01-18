using HouseApp.Views;

namespace HouseApp;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        
        // Register the house management route
        Routing.RegisterRoute("housemanagement", typeof(Views.HouseManagementPage));
    }
}

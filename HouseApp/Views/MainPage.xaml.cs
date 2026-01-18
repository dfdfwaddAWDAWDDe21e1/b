namespace HouseApp.Views;

public partial class MainPage : ContentPage
{
    public MainPage(ViewModels.MainViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}

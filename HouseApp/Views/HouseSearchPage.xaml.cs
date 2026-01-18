using HouseApp.ViewModels;

namespace HouseApp.Views;

public partial class HouseSearchPage : ContentPage
{
    public HouseSearchPage(HouseSearchViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    private async void OnBackButtonClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("///tabs/home");
    }
}

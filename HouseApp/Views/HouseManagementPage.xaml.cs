using HouseApp.ViewModels;

namespace HouseApp.Views;

[QueryProperty(nameof(HouseId), "HouseId")]
public partial class HouseManagementPage : ContentPage
{
    private readonly HouseManagementViewModel _viewModel;
    private int _houseId;

    public int HouseId
    {
        get => _houseId;
        set
        {
            _houseId = value;
            if (_viewModel != null)
            {
                _viewModel.HouseId = value;
                _viewModel.IsEditMode = value > 0;
            }
        }
    }

    public HouseManagementPage(HouseManagementViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        if (_viewModel.IsEditMode && _viewModel.HouseId > 0)
        {
            try
            {
                await _viewModel.LoadHouseDetailsCommand.ExecuteAsync(null);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading house details: {ex.Message}");
                await DisplayAlert("Error", "Failed to load house details", "OK");
            }
        }
    }

    private async void OnBackButtonClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("///tabs/dashboard");
    }
}

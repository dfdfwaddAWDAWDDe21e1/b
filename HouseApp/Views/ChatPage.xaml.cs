using HouseApp.ViewModels;

namespace HouseApp.Views;

public partial class ChatPage : ContentPage
{
    private readonly ChatViewModel _viewModel;

    public ChatPage(ChatViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        // Reload messages when page appears
        if (_viewModel.LoadPreviousMessagesCommand?.CanExecute(null) == true)
        {
            await _viewModel.LoadPreviousMessagesCommand.ExecuteAsync(null);
        }

        // Scroll to bottom after messages load
        await Task.Delay(300);
        await ScrollToBottom();
    }

    private async Task ScrollToBottom()
    {
        try
        {
            if (_viewModel.Messages.Any())
            {
                await MessagesScrollView.ScrollToAsync(0, MessagesScrollView.ContentSize.Height, false);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error scrolling: {ex.Message}");
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
    }

    private async void OnBackButtonClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("///tabs/home");
    }
}
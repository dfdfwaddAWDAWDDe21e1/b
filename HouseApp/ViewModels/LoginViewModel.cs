using System.Windows.Input;
using HouseApp.Services;

namespace HouseApp.ViewModels;

public class LoginViewModel : BindableObject
{
    private readonly UserSession _session;

    private string _name = "";
    public string Name
    {
        get => _name;
        set
        {
            if (_name == value) return;
            _name = value;
            OnPropertyChanged();
        }
    }

    public ICommand LoginCommand { get; }

    public LoginViewModel(UserSession session)
    {
        _session = session;

        LoginCommand = new Command(async () =>
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                await Application.Current!.MainPage!
                    .DisplayAlert("Error", "Please enter your name", "OK");
                return;
            }

            _session.UserName = Name.Trim();
            await Shell.Current.GoToAsync("//tabs/home");

        });
    }
}

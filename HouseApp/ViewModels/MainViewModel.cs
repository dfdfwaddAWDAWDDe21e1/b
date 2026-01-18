using HouseApp.Services;

namespace HouseApp.ViewModels;

public class MainViewModel : BindableObject
{
    private readonly UserSession _session;

    public string WelcomeText =>
        $"Welcome, {_session.UserName}!";

    public MainViewModel(UserSession session)
    {
        _session = session;
    }
}

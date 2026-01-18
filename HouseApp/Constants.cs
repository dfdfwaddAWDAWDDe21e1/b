namespace HouseApp;

public static class Constants
{
#if ANDROID
    // For Android Emulator, 10.0.2.2 maps to host machine's localhost
    public const string ApiBaseUrl = "https://10.0.2.2:5001";
#else
    // For iOS simulator, Windows, etc.
    public const string ApiBaseUrl = "https://localhost:5001";
#endif

    public const string TokenKey = "auth_token";
    public const string UserIdKey = "user_id";
    public const string UserTypeKey = "user_type";
    public const string UserEmailKey = "user_email";
    public const string UserFirstNameKey = "user_firstname";
    public const string UserLastNameKey = "user_lastname";
}

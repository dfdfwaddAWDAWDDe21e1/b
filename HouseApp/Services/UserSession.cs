using HouseApp.Models;

namespace HouseApp.Services;

public class UserSession
{
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public UserType UserType { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? UserName { get; set; }
    public bool IsLoggedIn => UserId > 0;
    
    public void Clear()
    {
        UserId = 0;
        Email = string.Empty;
        FirstName = string.Empty;
        LastName = string.Empty;
        UserName = null;
    }
}

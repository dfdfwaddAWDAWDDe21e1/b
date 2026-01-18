using HouseApp.API.DTOs;

namespace HouseApp.API.Services;

public interface IAuthService
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string passwordHash);
    string GenerateJwtToken(int userId, string email, string userType);
}

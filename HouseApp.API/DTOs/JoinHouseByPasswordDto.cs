namespace HouseApp.API.DTOs;

public class JoinHouseByPasswordDto
{
    public int StudentId { get; set; }
    public string Password { get; set; } = string.Empty;
}

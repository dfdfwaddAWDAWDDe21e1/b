namespace HouseApp.API.DTOs;

public class JoinHouseByCodeDto
{
    public int StudentId { get; set; }
    public string HouseCode { get; set; } = string.Empty;
}

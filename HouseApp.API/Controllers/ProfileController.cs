using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HouseApp.API.Data;
using HouseApp.API.Models;

namespace HouseApp.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ProfileController : ControllerBase
{
    private readonly AppDbContext _context;

    public ProfileController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("{userId}")]
    public async Task<ActionResult<object>> GetProfile(int userId)
    {
        var user = await _context.Users.FindAsync(userId);

        if (user == null)
        {
            return NotFound();
        }

        return Ok(new
        {
            user.Id,
            user.Email,
            user.UserType,
            user.FirstName,
            user.LastName,
            user.Age,
            user.ProfilePictureUrl,
            user.DateOfBirth,
            user.PhoneNumber
        });
    }

    [HttpPut("{userId}")]
    public async Task<IActionResult> UpdateProfile(int userId, [FromBody] UpdateProfileDto dto)
    {
        var user = await _context.Users.FindAsync(userId);

        if (user == null)
        {
            return NotFound();
        }

        if (!string.IsNullOrEmpty(dto.FirstName))
            user.FirstName = dto.FirstName;

        if (!string.IsNullOrEmpty(dto.LastName))
            user.LastName = dto.LastName;

        if (!string.IsNullOrEmpty(dto.PhoneNumber))
            user.PhoneNumber = dto.PhoneNumber;

        if (!string.IsNullOrEmpty(dto.ProfilePictureUrl))
            user.ProfilePictureUrl = dto.ProfilePictureUrl;

        if (dto.DateOfBirth.HasValue)
        {
            user.DateOfBirth = dto.DateOfBirth.Value;
            var age = DateTime.UtcNow.Year - dto.DateOfBirth.Value.Year;
            if (dto.DateOfBirth.Value > DateTime.UtcNow.AddYears(-age)) age--;
            user.Age = age;
        }

        await _context.SaveChangesAsync();

        return NoContent();
    }

    public class UpdateProfileDto
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public DateTime? DateOfBirth { get; set; }
    }
}

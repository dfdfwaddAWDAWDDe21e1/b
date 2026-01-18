using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using HouseApp.API.Data;
using HouseApp.API.DTOs;
using HouseApp.API.Models;
using HouseApp.API.Services;

namespace HouseApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IAuthService _authService;

    public AuthController(AppDbContext context, IAuthService authService)
    {
        _context = context;
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<LoginResponseDto>> Register(RegisterDto dto)
    {
        // Server-side validation
        
        // First Name validation
        if (string.IsNullOrWhiteSpace(dto.FirstName) || dto.FirstName.Length < 2)
        {
            return BadRequest(new { message = "First name must be at least 2 characters" });
        }
        if (dto.FirstName.Any(char.IsDigit))
        {
            return BadRequest(new { message = "First name cannot contain numbers" });
        }
        if (!dto.FirstName.All(c => char.IsLetter(c) || char.IsWhiteSpace(c)))
        {
            return BadRequest(new { message = "First name must contain only letters" });
        }

        // Last Name validation
        if (string.IsNullOrWhiteSpace(dto.LastName) || dto.LastName.Length < 2)
        {
            return BadRequest(new { message = "Last name must be at least 2 characters" });
        }
        if (dto.LastName.Any(char.IsDigit))
        {
            return BadRequest(new { message = "Last name cannot contain numbers" });
        }
        if (!dto.LastName.All(c => char.IsLetter(c) || char.IsWhiteSpace(c)))
        {
            return BadRequest(new { message = "Last name must contain only letters" });
        }

        // Email validation
        if (string.IsNullOrWhiteSpace(dto.Email) || !dto.Email.Contains('@') || !dto.Email.Contains('.'))
        {
            return BadRequest(new { message = "Please enter a valid email address" });
        }

        // Phone Number validation
        if (string.IsNullOrWhiteSpace(dto.PhoneNumber))
        {
            return BadRequest(new { message = "Phone number is required" });
        }
        var digitsOnly = new string(dto.PhoneNumber.Where(char.IsDigit).ToArray());
        if (digitsOnly.Length < 10)
        {
            return BadRequest(new { message = "Phone number must contain at least 10 digits" });
        }

        // Password validation
        if (string.IsNullOrWhiteSpace(dto.Password) || dto.Password.Length < 8)
        {
            return BadRequest(new { message = "Password must be at least 8 characters" });
        }
        if (!dto.Password.Any(char.IsUpper))
        {
            return BadRequest(new { message = "Password must contain at least one uppercase letter" });
        }
        if (!dto.Password.Any(char.IsLower))
        {
            return BadRequest(new { message = "Password must contain at least one lowercase letter" });
        }
        if (!dto.Password.Any(char.IsDigit))
        {
            return BadRequest(new { message = "Password must contain at least one number" });
        }

        // Age validation (18+)
        var currentAge = DateTime.UtcNow.Year - dto.DateOfBirth.Year;
        if (dto.DateOfBirth > DateTime.UtcNow.AddYears(-currentAge)) currentAge--;
        if (currentAge < 18)
        {
            return BadRequest(new { message = "You must be at least 18 years old to register" });
        }

        // Check if email already exists
        if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
        {
            return BadRequest(new { message = "Email already registered" });
        }

        var age = DateTime.UtcNow.Year - dto.DateOfBirth.Year;
        if (dto.DateOfBirth > DateTime.UtcNow.AddYears(-age)) age--;

        var user = new User
        {
            Email = dto.Email,
            PasswordHash = _authService.HashPassword(dto.Password),
            UserType = dto.UserType,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            DateOfBirth = dto.DateOfBirth,
            Age = age,
            PhoneNumber = dto.PhoneNumber,
            CreatedDate = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var token = _authService.GenerateJwtToken(user.Id, user.Email, user.UserType.ToString());

        return Ok(new LoginResponseDto
        {
            Token = token,
            UserId = user.Id,
            Email = user.Email,
            UserType = user.UserType,
            FirstName = user.FirstName,
            LastName = user.LastName
        });
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDto>> Login(LoginDto dto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);

        if (user == null || !_authService.VerifyPassword(dto.Password, user.PasswordHash))
        {
            return Unauthorized(new { message = "Invalid credentials" });
        }

        var token = _authService.GenerateJwtToken(user.Id, user.Email, user.UserType.ToString());

        return Ok(new LoginResponseDto
        {
            Token = token,
            UserId = user.Id,
            Email = user.Email,
            UserType = user.UserType,
            FirstName = user.FirstName,
            LastName = user.LastName
        });
    }

    [HttpGet("users/search")]
    [Authorize]
    public async Task<ActionResult<UserDto>> SearchUserByEmail([FromQuery] string email)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email);

        if (user == null)
            return NotFound();

        return Ok(new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            UserType = user.UserType
        });
    }
}

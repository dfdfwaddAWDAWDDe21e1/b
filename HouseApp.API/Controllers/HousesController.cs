using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HouseApp.API.Data;
using HouseApp.API.DTOs;
using HouseApp.API.Models;

namespace HouseApp.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class HousesController : ControllerBase
{
    private readonly AppDbContext _context;

    public HousesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<HouseDto>>> GetHouses()
    {
        var houses = await _context.Houses
            .Include(h => h.HouseTenants)
            .Select(h => new HouseDto
            {
                Id = h.Id,
                Name = h.Name,
                Address = h.Address,
                LandlordId = h.LandlordId,
                MonthlyRent = h.MonthlyRent,
                UtilitiesCost = h.UtilitiesCost,
                WaterBillCost = h.WaterBillCost,
                MaxOccupants = h.MaxOccupants,
                CurrentOccupants = h.HouseTenants.Count(ht => ht.IsActive),
                CreatedDate = h.CreatedDate
            })
            .ToListAsync();

        return Ok(houses);
    }

    [HttpGet("my-houses")]
    [Authorize(Roles = "Landlord")]
    public async Task<ActionResult<IEnumerable<HouseDto>>> GetMyHouses()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                       ?? User.FindFirst("sub")?.Value;
        
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            return Unauthorized(new { message = "User ID not found in token" });
        }

        var houses = await _context.Houses
            .Include(h => h.HouseTenants)
            .Where(h => h.LandlordId == userId)
            .Select(h => new HouseDto
            {
                Id = h.Id,
                Name = h.Name,
                Address = h.Address,
                LandlordId = h.LandlordId,
                MonthlyRent = h.MonthlyRent,
                UtilitiesCost = h.UtilitiesCost,
                WaterBillCost = h.WaterBillCost,
                MaxOccupants = h.MaxOccupants,
                CurrentOccupants = h.HouseTenants.Count(ht => ht.IsActive),
                CreatedDate = h.CreatedDate
            })
            .ToListAsync();

        return Ok(houses);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<HouseDto>> GetHouse(int id)
    {
        var house = await _context.Houses
            .Include(h => h.HouseTenants)
            .FirstOrDefaultAsync(h => h.Id == id);

        if (house == null)
        {
            return NotFound();
        }

        var houseDto = new HouseDto
        {
            Id = house.Id,
            Name = house.Name,
            Address = house.Address,
            LandlordId = house.LandlordId,
            MonthlyRent = house.MonthlyRent,
            UtilitiesCost = house.UtilitiesCost,
            WaterBillCost = house.WaterBillCost,
            MaxOccupants = house.MaxOccupants,
            CurrentOccupants = house.HouseTenants.Count(ht => ht.IsActive),
            CreatedDate = house.CreatedDate,
            HouseCode = house.HouseCode,
            Password = house.Password
        };

        return Ok(houseDto);
    }

    [HttpPost]
    [Authorize(Roles = "Landlord")]
    public async Task<ActionResult<HouseDto>> CreateHouse(HouseDto dto)
    {
        var house = new House
        {
            Name = dto.Name,
            Address = dto.Address,
            LandlordId = dto.LandlordId ?? 0,
            MonthlyRent = dto.MonthlyRent,
            UtilitiesCost = dto.UtilitiesCost,
            WaterBillCost = dto.WaterBillCost,
            MaxOccupants = dto.MaxOccupants,
            Password = dto.Password,
            HouseCode = GenerateHouseCode(),
            CreatedDate = DateTime.UtcNow
        };

        _context.Houses.Add(house);
        await _context.SaveChangesAsync();

        dto.Id = house.Id;
        dto.CreatedDate = house.CreatedDate;
        dto.CurrentOccupants = 0;
        dto.HouseCode = house.HouseCode;

        return CreatedAtAction(nameof(GetHouse), new { id = house.Id }, dto);
    }

    private string GenerateHouseCode()
    {
        // Generate a unique 6-character code using cryptographically secure random
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        string code;
        
        do
        {
            var result = new char[6];
            byte randomByte;
            
            for (int i = 0; i < 6; i++)
            {
                do
                {
                    randomByte = (byte)System.Security.Cryptography.RandomNumberGenerator.GetInt32(256);
                }
                while (randomByte >= 256 - (256 % chars.Length)); // Rejection sampling to avoid bias
                
                result[i] = chars[randomByte % chars.Length];
            }
            
            code = new string(result);
        }
        while (_context.Houses.Any(h => h.HouseCode == code)); // Ensure unique
        
        return code;
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Landlord")]
    public async Task<IActionResult> UpdateHouse(int id, UpdateHouseDto dto)
    {
        var house = await _context.Houses.FindAsync(id);
        if (house == null)
        {
            return NotFound();
        }

        house.Name = dto.Name;
        house.Address = dto.Address;
        house.MonthlyRent = dto.MonthlyRent;
        house.UtilitiesCost = dto.UtilitiesCost;
        house.WaterBillCost = dto.WaterBillCost;
        house.MaxOccupants = dto.MaxOccupants;
        house.Password = dto.Password;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Landlord")]
    public async Task<IActionResult> DeleteHouse(int id)
    {
        var house = await _context.Houses.FindAsync(id);
        if (house == null)
        {
            return NotFound();
        }

        _context.Houses.Remove(house);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("{houseId}/tenants/{studentId}")]
    [Authorize(Roles = "Landlord")]
    public async Task<IActionResult> AddTenant(int houseId, int studentId)
    {
        var house = await _context.Houses
            .Include(h => h.HouseTenants)
            .FirstOrDefaultAsync(h => h.Id == houseId);

        if (house == null)
        {
            return NotFound("House not found");
        }

        var activeTenantsCount = house.HouseTenants.Count(ht => ht.IsActive);
        if (activeTenantsCount >= house.MaxOccupants)
        {
            return BadRequest("House is at maximum occupancy");
        }

        var existingTenant = await _context.HouseTenants
            .FirstOrDefaultAsync(ht => ht.HouseId == houseId && ht.StudentId == studentId && ht.IsActive);

        if (existingTenant != null)
        {
            return BadRequest("Student is already a tenant");
        }

        var houseTenant = new HouseTenant
        {
            HouseId = houseId,
            StudentId = studentId,
            JoinedDate = DateTime.UtcNow,
            IsActive = true
        };

        _context.HouseTenants.Add(houseTenant);
        await _context.SaveChangesAsync();

        return Ok();
    }

    [HttpDelete("{houseId}/tenants/{studentId}")]
    [Authorize(Roles = "Landlord")]
    public async Task<IActionResult> RemoveTenant(int houseId, int studentId)
    {
        var houseTenant = await _context.HouseTenants
            .FirstOrDefaultAsync(ht => ht.HouseId == houseId && ht.StudentId == studentId && ht.IsActive);

        if (houseTenant == null)
        {
            return NotFound();
        }

        houseTenant.IsActive = false;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("{houseId}/tenants")]
    public async Task<ActionResult<IEnumerable<TenantDto>>> GetHouseTenants(int houseId)
    {
        var tenants = await _context.HouseTenants
            .Include(ht => ht.Student)
            .Where(ht => ht.HouseId == houseId && ht.IsActive)
            .Select(ht => new TenantDto
            {
                UserId = ht.StudentId,
                Name = $"{ht.Student.FirstName} {ht.Student.LastName}",
                Email = ht.Student.Email,
                JoinedDate = ht.JoinedDate
            })
            .ToListAsync();

        return Ok(tenants);
    }

    [HttpGet("{houseId}/messages")]
    public async Task<ActionResult<IEnumerable<MessageDto>>> GetHouseMessages(int houseId)
    {
        var messages = await _context.Messages
            .Where(m => m.HouseId == houseId)
            .OrderBy(m => m.Timestamp)
            .Select(m => new MessageDto
            {
                Id = m.Id,
                HouseId = m.HouseId,
                SenderId = m.SenderId,
                SenderName = m.SenderName,
                MessageText = m.MessageText,
                Timestamp = m.Timestamp,
                IsRead = m.IsRead
            })
            .ToListAsync();

        return Ok(messages);
    }

    [HttpGet("student/{studentId}/house")]
    public async Task<ActionResult<HouseTenantDto>> GetStudentHouse(int studentId)
    {
        var houseTenant = await _context.HouseTenants
            .Include(ht => ht.House)
            .Where(ht => ht.StudentId == studentId && ht.IsActive)
            .FirstOrDefaultAsync();

        if (houseTenant == null)
        {
            return NotFound(new { message = "Student is not assigned to any house" });
        }

        return Ok(new HouseTenantDto
        {
            HouseId = houseTenant.HouseId,
            HouseName = houseTenant.House.Name,
            StudentId = houseTenant.StudentId,
            JoinedDate = houseTenant.JoinedDate
        });
    }

    [HttpGet("available")]
    public async Task<ActionResult<IEnumerable<HouseDto>>> GetAvailableHouses()
    {
        var houses = await _context.Houses
            .Include(h => h.HouseTenants)
            .Where(h => h.HouseTenants.Count(ht => ht.IsActive) < h.MaxOccupants)
            .Select(h => new HouseDto
            {
                Id = h.Id,
                Name = h.Name,
                Address = h.Address,
                MonthlyRent = h.MonthlyRent,
                UtilitiesCost = h.UtilitiesCost,
                WaterBillCost = h.WaterBillCost,
                MaxOccupants = h.MaxOccupants,
                CurrentOccupants = h.HouseTenants.Count(ht => ht.IsActive),
                HouseCode = h.HouseCode
                // Don't expose password in list
            })
            .ToListAsync();

        return Ok(houses);
    }

    [HttpPost("join")]
    public async Task<IActionResult> JoinHouseByCode([FromBody] JoinHouseByCodeDto dto)
    {
        var house = await _context.Houses
            .Include(h => h.HouseTenants)
            .FirstOrDefaultAsync(h => h.HouseCode == dto.HouseCode);

        if (house == null)
            return NotFound(new { message = "Invalid house code" });

        if (house.HouseTenants.Count(ht => ht.IsActive) >= house.MaxOccupants)
            return BadRequest(new { message = "House is full" });

        var existingTenant = await _context.HouseTenants
            .AnyAsync(ht => ht.StudentId == dto.StudentId && ht.IsActive);

        if (existingTenant)
            return BadRequest(new { message = "You are already in a house" });

        var houseTenant = new HouseTenant
        {
            HouseId = house.Id,
            StudentId = dto.StudentId,
            JoinedDate = DateTime.UtcNow,
            IsActive = true
        };

        _context.HouseTenants.Add(houseTenant);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Successfully joined house" });
    }

    [HttpPost("{houseId}/join")]
    public async Task<IActionResult> JoinHouseByPassword(int houseId, [FromBody] JoinHouseByPasswordDto dto)
    {
        var house = await _context.Houses
            .Include(h => h.HouseTenants)
            .FirstOrDefaultAsync(h => h.Id == houseId);

        if (house == null)
            return NotFound(new { message = "House not found" });

        if (house.Password != dto.Password)
            return BadRequest(new { message = "Incorrect password" });

        if (house.HouseTenants.Count(ht => ht.IsActive) >= house.MaxOccupants)
            return BadRequest(new { message = "House is full" });

        var existingTenant = await _context.HouseTenants
            .AnyAsync(ht => ht.StudentId == dto.StudentId && ht.IsActive);

        if (existingTenant)
            return BadRequest(new { message = "You are already in a house" });

        var houseTenant = new HouseTenant
        {
            HouseId = house.Id,
            StudentId = dto.StudentId,
            JoinedDate = DateTime.UtcNow,
            IsActive = true
        };

        _context.HouseTenants.Add(houseTenant);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Successfully joined house" });
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using HouseApp.API.Data;
using HouseApp.API.DTOs;
using HouseApp.API.Models;
using HouseApp.API.Hubs;

namespace HouseApp.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class SensorsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IHubContext<ChatHub> _hubContext;

    public SensorsController(AppDbContext context, IHubContext<ChatHub> hubContext)
    {
        _context = context;
        _hubContext = hubContext;
    }

    [HttpPost("readings")]
    public async Task<IActionResult> PostReading([FromBody] SensorReadingDto dto)
    {
        var reading = new SensorReading
        {
            HouseId = dto.HouseId,
            Temperature = dto.TempC,
            Humidity = dto.Humidity,
            Timestamp = DateTime.UtcNow,
            DeviceId = dto.DeviceId
        };

        _context.SensorReadings.Add(reading);
        await _context.SaveChangesAsync();

        // Broadcast to house group via SignalR
        await _hubContext.Clients.Group($"House_{dto.HouseId}")
            .SendAsync("ReceiveSensorReading", new SensorReadingDto
            {
                Id = reading.Id,
                HouseId = reading.HouseId,
                TempC = reading.Temperature,
                Humidity = reading.Humidity,
                Timestamp = reading.Timestamp,
                DeviceId = reading.DeviceId
            });

        return Ok();
    }

    [HttpGet("house/{houseId}/latest")]
    public async Task<IActionResult> GetLatest(int houseId)
    {
        var reading = await _context.SensorReadings
            .Where(r => r.HouseId == houseId)
            .OrderByDescending(r => r.Timestamp)
            .FirstOrDefaultAsync();

        if (reading == null)
        {
            return NotFound();
        }

        return Ok(new SensorReadingDto
        {
            Id = reading.Id,
            HouseId = reading.HouseId,
            TempC = reading.Temperature,
            Humidity = reading.Humidity,
            Timestamp = reading.Timestamp,
            DeviceId = reading.DeviceId
        });
    }

    [HttpGet("house/{houseId}/history")]
    public async Task<IActionResult> GetHistory(int houseId)
    {
        var cutoff = DateTime.UtcNow.AddHours(-24);
        var readings = await _context.SensorReadings
            .Where(r => r.HouseId == houseId && r.Timestamp >= cutoff)
            .OrderBy(r => r.Timestamp)
            .Select(r => new SensorReadingDto
            {
                Id = r.Id,
                HouseId = r.HouseId,
                TempC = r.Temperature,
                Humidity = r.Humidity,
                Timestamp = r.Timestamp,
                DeviceId = r.DeviceId
            })
            .ToListAsync();

        return Ok(readings);
    }
}

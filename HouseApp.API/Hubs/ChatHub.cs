using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using HouseApp.API.Data;
using HouseApp.API.Models;
using HouseApp.API.DTOs;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace HouseApp.API.Hubs;

[Authorize] // Require authentication
public class ChatHub : Hub
{
    private readonly AppDbContext _context;
    private readonly ILogger<ChatHub> _logger;

    public ChatHub(AppDbContext context, ILogger<ChatHub> logger)
    {
        _context = context;
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation($"Client connected: {Context.ConnectionId}");
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation($"Client disconnected: {Context.ConnectionId}");
        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinHouseChat(int houseId)
    {
        try
        {
            var userId = GetUserId();
            _logger.LogInformation($"User {userId} joining house {houseId} chat");

            // Verify user is a tenant of this house
            var isTenant = await _context.HouseTenants
                .AnyAsync(ht => ht.HouseId == houseId && ht.StudentId == userId && ht.IsActive);

            if (!isTenant)
            {
                _logger.LogWarning($"User {userId} is not a tenant of house {houseId}");
                throw new HubException("You are not a member of this house");
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, $"House_{houseId}");
            _logger.LogInformation($"User {userId} successfully joined house {houseId} chat group");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error joining house chat: {ex.Message}");
            throw;
        }
    }

    public async Task LeaveHouseChat(int houseId)
    {
        try
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"House_{houseId}");
            _logger.LogInformation($"Connection {Context.ConnectionId} left house {houseId} chat");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error leaving house chat: {ex.Message}");
            throw;
        }
    }

    // Keep old method for backward compatibility
    public async Task JoinHouse(string houseId)
    {
        if (int.TryParse(houseId, out int id))
        {
            await JoinHouseChat(id);
        }
    }

    // Keep old method for backward compatibility
    public async Task LeaveHouse(string houseId)
    {
        if (int.TryParse(houseId, out int id))
        {
            await LeaveHouseChat(id);
        }
    }

    public async Task SendMessage(int houseId, string messageText)
    {
        try
        {
            var userId = GetUserId();
            var userEmail = Context.User?.FindFirst(ClaimTypes.Email)?.Value ?? "Unknown";

            _logger.LogInformation($"User {userId} sending message to house {houseId}");

            // Verify user is a tenant
            var isTenant = await _context.HouseTenants
                .AnyAsync(ht => ht.HouseId == houseId && ht.StudentId == userId && ht.IsActive);

            if (!isTenant)
            {
                _logger.LogWarning($"User {userId} attempted to send message but is not a tenant");
                throw new HubException("You are not a member of this house");
            }

            // Get user name
            var user = await _context.Users.FindAsync(userId);
            var senderName = user != null ? $"{user.FirstName} {user.LastName}" : userEmail;

            // Save message to database
            var message = new Message
            {
                HouseId = houseId,
                SenderId = userId,
                SenderName = senderName,
                MessageText = messageText,
                Timestamp = DateTime.UtcNow,
                IsRead = false
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            // Broadcast to all clients in the house group
            var messageDto = new MessageDto
            {
                Id = message.Id,
                HouseId = message.HouseId,
                SenderId = message.SenderId,
                SenderName = message.SenderName,
                MessageText = message.MessageText,
                Timestamp = message.Timestamp,
                IsRead = message.IsRead
            };

            await Clients.Group($"House_{houseId}").SendAsync("ReceiveMessage", messageDto);
            _logger.LogInformation($"Message {message.Id} sent to house {houseId} group");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error sending message: {ex.Message}");
            throw;
        }
    }

    public async Task MarkMessageAsRead(int messageId)
    {
        try
        {
            var message = await _context.Messages.FindAsync(messageId);
            if (message != null)
            {
                message.IsRead = true;
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Message {messageId} marked as read");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error marking message as read: {ex.Message}");
            throw;
        }
    }

    private int GetUserId()
    {
        var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                       ?? Context.User?.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userIdClaim))
        {
            _logger.LogError("User ID not found in claims");
            throw new HubException("User not authenticated");
        }

        if (!int.TryParse(userIdClaim, out int userId))
        {
            _logger.LogError($"Invalid user ID format in claims: {userIdClaim}");
            throw new HubException("Invalid user ID");
        }

        return userId;
    }
}

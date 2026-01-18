using Microsoft.AspNetCore.SignalR.Client;
using HouseApp.DTOs;
using HouseApp.Models;

namespace HouseApp.Services;

public class ChatService
{
    private HubConnection? _hubConnection;
    private readonly AuthService _authService;
    private bool _isConnecting;

    public event Action<ChatMessage>? MessageReceived;
    public event Action<string>? ConnectionStatusChanged;
    public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;

    public ChatService(AuthService authService)
    {
        _authService = authService;
    }

    public async Task InitializeAsync()
    {
        if (_isConnecting || IsConnected)
        {
            System.Diagnostics.Debug.WriteLine("Already connected or connecting");
            return;
        }

        _isConnecting = true;

        try
        {
            var token = await SecureStorage.GetAsync(Constants.TokenKey);
            if (string.IsNullOrEmpty(token))
            {
                throw new InvalidOperationException("Not authenticated - no token found");
            }

            System.Diagnostics.Debug.WriteLine($"Connecting to: {Constants.ApiBaseUrl}/hubs/chat");
            System.Diagnostics.Debug.WriteLine($"Token: {token.Substring(0, Math.Min(20, token.Length))}...");

            _hubConnection = new HubConnectionBuilder()
                .WithUrl($"{Constants.ApiBaseUrl}/hubs/chat", options =>
                {
                    // CRITICAL: Pass token as query parameter for SignalR
                    options.AccessTokenProvider = () => Task.FromResult<string?>(token);
                    
                    // For development with self-signed certificates
                    options.HttpMessageHandlerFactory = (handler) =>
                    {
                        if (handler is HttpClientHandler clientHandler)
                        {
                            clientHandler.ServerCertificateCustomValidationCallback = 
                                (sender, certificate, chain, sslPolicyErrors) => true;
                        }
                        return handler;
                    };
                })
                .WithAutomaticReconnect(new[] { TimeSpan.Zero, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(10) })
                .Build();

            // Set up event handlers
            _hubConnection.Reconnecting += (error) =>
            {
                System.Diagnostics.Debug.WriteLine($"SignalR Reconnecting: {error?.Message}");
                ConnectionStatusChanged?.Invoke("Reconnecting...");
                return Task.CompletedTask;
            };

            _hubConnection.Reconnected += (connectionId) =>
            {
                System.Diagnostics.Debug.WriteLine($"SignalR Reconnected: {connectionId}");
                ConnectionStatusChanged?.Invoke("Connected");
                return Task.CompletedTask;
            };

            _hubConnection.Closed += (error) =>
            {
                System.Diagnostics.Debug.WriteLine($"SignalR Closed: {error?.Message}");
                ConnectionStatusChanged?.Invoke("Disconnected");
                return Task.CompletedTask;
            };

            // Listen for incoming messages
            _hubConnection.On<MessageDto>("ReceiveMessage", async (message) =>
            {
                System.Diagnostics.Debug.WriteLine($"Message received: {message.MessageText}");
                
                var currentUserId = await _authService.GetCurrentUserIdAsync();
                MessageReceived?.Invoke(new ChatMessage
                {
                    Id = message.Id,
                    HouseId = message.HouseId,
                    SenderId = message.SenderId,
                    SenderName = message.SenderName,
                    MessageText = message.MessageText,
                    Timestamp = message.Timestamp,
                    IsRead = message.IsRead,
                    IsCurrentUser = message.SenderId == currentUserId
                });
            });

            // Start connection
            ConnectionStatusChanged?.Invoke("Connecting...");
            await _hubConnection.StartAsync();
            ConnectionStatusChanged?.Invoke("Connected");

            System.Diagnostics.Debug.WriteLine($"SignalR Connected! State: {_hubConnection.State}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"SignalR Connection Error: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
            ConnectionStatusChanged?.Invoke($"Error: {ex.Message}");
            throw;
        }
        finally
        {
            _isConnecting = false;
        }
    }

    public async Task JoinHouseChat(int houseId)
    {
        if (_hubConnection == null || !IsConnected)
        {
            throw new InvalidOperationException("Not connected to chat server");
        }

        try
        {
            System.Diagnostics.Debug.WriteLine($"Joining house {houseId} chat");
            await _hubConnection.InvokeAsync("JoinHouseChat", houseId);
            System.Diagnostics.Debug.WriteLine($"Successfully joined house {houseId}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error joining house chat: {ex.Message}");
            throw;
        }
    }

    public async Task SendMessageAsync(int houseId, string message)
    {
        if (_hubConnection?.State != HubConnectionState.Connected)
        {
            throw new InvalidOperationException("Not connected to chat server");
        }

        if (string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException("Message cannot be empty");
        }

        try
        {
            System.Diagnostics.Debug.WriteLine($"Sending message to house {houseId}: {message}");
            await _hubConnection.InvokeAsync("SendMessage", houseId, message);
            System.Diagnostics.Debug.WriteLine("Message sent successfully");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error sending message: {ex.Message}");
            throw;
        }
    }

    public async Task DisconnectAsync()
    {
        if (_hubConnection != null)
        {
            await _hubConnection.StopAsync();
            await _hubConnection.DisposeAsync();
            _hubConnection = null;
        }
    }
}

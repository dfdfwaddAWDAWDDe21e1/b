using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HouseApp.Services;
using HouseApp.Models;
using HouseApp.DTOs;
using System.Collections.ObjectModel;

namespace HouseApp.ViewModels;

public partial class ChatViewModel : ObservableObject
{
    private readonly ChatService _chatService;
    private readonly ApiService _apiService;
    private int _currentHouseId;
    private int _currentUserId;

    [ObservableProperty]
    private ObservableCollection<ChatMessageViewModel> messages = new();

    [ObservableProperty]
    private string newMessage = string.Empty;

    [ObservableProperty]
    private string connectionStatus = "Connecting...";

    [ObservableProperty]
    private bool isConnected;

    [ObservableProperty]
    private string houseName = string.Empty;

    [ObservableProperty]
    private bool hasNoMessages = true;

    public ChatViewModel(ChatService chatService, ApiService apiService)
    {
        _chatService = chatService;
        _apiService = apiService;
        
        _chatService.MessageReceived += OnMessageReceived;
        _chatService.ConnectionStatusChanged += HandleConnectionStatusChanged;
        
        _ = InitializeChat();
    }

    private async Task InitializeChat()
    {
        try
        {
            // Get current user ID
            var userIdString = await SecureStorage.GetAsync(Constants.UserIdKey);
            _currentUserId = int.Parse(userIdString ?? "0");

            if (_currentUserId == 0)
            {
                ConnectionStatus = "Not logged in";
                return;
            }

            System.Diagnostics.Debug.WriteLine($"Current User ID: {_currentUserId}");

            // Get user's house
            try
            {
                var houseTenant = await _apiService.GetAsync<HouseTenantDto>($"/api/houses/student/{_currentUserId}/house");
                
                if (houseTenant != null && houseTenant.HouseId > 0)
                {
                    _currentHouseId = houseTenant.HouseId;
                    HouseName = houseTenant.HouseName;

                    System.Diagnostics.Debug.WriteLine($"House ID: {_currentHouseId}, Name: {HouseName}");

                    // Connect to SignalR
                    await _chatService.InitializeAsync();
                    
                    if (_chatService.IsConnected)
                    {
                        await _chatService.JoinHouseChat(_currentHouseId);
                        
                        // Load previous messages from database
                        await LoadPreviousMessages();
                    }
                }
                else
                {
                    ConnectionStatus = "Not assigned to a house";
                    IsConnected = false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting house: {ex.Message}");
                ConnectionStatus = "Not assigned to a house";
                IsConnected = false;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Chat initialization error: {ex.Message}");
            ConnectionStatus = $"Error: {ex.Message}";
            IsConnected = false;
        }
    }

    [RelayCommand]
    private async Task LoadPreviousMessages()
    {
        if (_currentHouseId <= 0) return;

        try
        {
            System.Diagnostics.Debug.WriteLine($"Loading messages for house {_currentHouseId}");

            var messagesList = await _apiService.GetAsync<List<MessageDto>>($"/api/houses/{_currentHouseId}/messages");
            
            System.Diagnostics.Debug.WriteLine($"Loaded {messagesList?.Count ?? 0} messages");

            if (messagesList != null && messagesList.Any())
            {
                Messages.Clear();
                
                foreach (var msg in messagesList.OrderBy(m => m.Timestamp))
                {
                    System.Diagnostics.Debug.WriteLine($"Message: {msg.SenderName}: {msg.MessageText}");
                    
                    Messages.Add(new ChatMessageViewModel
                    {
                        Id = msg.Id,
                        SenderId = msg.SenderId,
                        SenderName = msg.SenderName,
                        MessageText = msg.MessageText,
                        Timestamp = msg.Timestamp,
                        IsFromCurrentUser = msg.SenderId == _currentUserId
                    });
                }

                HasNoMessages = false;
            }
            else
            {
                Messages.Clear();
                HasNoMessages = true;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading messages: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }

    private void OnMessageReceived(ChatMessage message)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            System.Diagnostics.Debug.WriteLine($"Message received: {message.SenderName}: {message.MessageText}");

            // Check if message already exists (avoid duplicates)
            if (!Messages.Any(m => m.Id == message.Id && message.Id > 0))
            {
                Messages.Add(new ChatMessageViewModel
                {
                    Id = message.Id,
                    SenderId = message.SenderId,
                    SenderName = message.SenderName,
                    MessageText = message.MessageText,
                    Timestamp = message.Timestamp,
                    IsFromCurrentUser = message.SenderId == _currentUserId
                });

                HasNoMessages = false;
            }
        });
    }

    private void HandleConnectionStatusChanged(string status)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            ConnectionStatus = status;
            IsConnected = status == "Connected";
            System.Diagnostics.Debug.WriteLine($"Connection status: {status}");
        });
    }

    [RelayCommand]
    private async Task SendMessage()
    {
        if (string.IsNullOrWhiteSpace(NewMessage))
        {
            return;
        }

        if (!_chatService.IsConnected)
        {
            await Shell.Current.DisplayAlert("Error", "Not connected to chat. Please wait...", "OK");
            return;
        }

        if (_currentHouseId <= 0)
        {
            await Shell.Current.DisplayAlert("Error", "You are not assigned to a house yet.", "OK");
            return;
        }

        var messageText = NewMessage;
        
        try
        {
            NewMessage = string.Empty; // Clear input immediately

            System.Diagnostics.Debug.WriteLine($"Sending message: {messageText}");

            await _chatService.SendMessageAsync(_currentHouseId, messageText);
            
            System.Diagnostics.Debug.WriteLine("Message sent successfully");
            
            // Message will be received via SignalR callback
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error sending message: {ex.Message}");
            await Shell.Current.DisplayAlert("Error", $"Failed to send: {ex.Message}", "OK");
            
            // Only restore if user hasn't typed a new message
            if (string.IsNullOrEmpty(NewMessage))
            {
                NewMessage = messageText;
            }
        }
    }
}

// Message ViewModel for UI binding
public partial class ChatMessageViewModel :  ObservableObject
{
    [ObservableProperty]
    private int id;

    [ObservableProperty]
    private int senderId;

    [ObservableProperty]
    private string senderName = string.Empty;

    [ObservableProperty]
    private string messageText = string.Empty;

    [ObservableProperty]
    private DateTime timestamp;

    [ObservableProperty]
    private bool isFromCurrentUser;
}
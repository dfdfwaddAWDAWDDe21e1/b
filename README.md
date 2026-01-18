# HouseApp - House Rental Management System

A comprehensive house rental management application built with ASP.NET Core Web API and .NET MAUI, designed for students and landlords to manage rental properties, payments, and communication.

## Features

### Backend API (HouseApp.API)
- **JWT Authentication**: Secure user authentication with role-based access (Student/Landlord)
- **User Management**: Registration, login, and profile management
- **House Management**: CRUD operations for rental properties
- **Tenant Management**: Add/remove students from houses
- **Payment System**: Stripe integration for rent, utilities, and water bill payments
- **Real-time Chat**: SignalR-based messaging for house members
- **Database**: Entity Framework Core with SQLite

### Frontend App (HouseApp - .NET MAUI)
- **User Registration**: Sign up as Student or Landlord
- **Landlord Dashboard**: Manage multiple properties and tenants
- **House Management**: Edit property details, add/remove tenants, track payments
- **Student Home**: View rent breakdown and payment status
- **Payment Integration**: Process payments through Stripe
- **Real-time Chat**: Communicate with housemates
- **Profile Management**: Update personal information and profile picture

## Architecture

### Backend Technologies
- ASP.NET Core 10.0 Web API
- Entity Framework Core 10.0
- SQLite Database
- JWT Bearer Authentication
- BCrypt.NET for password hashing
- Stripe.NET for payment processing
- SignalR for real-time communication

### Frontend Technologies
- .NET MAUI 10.0
- CommunityToolkit.Mvvm for MVVM pattern
- SignalR Client for real-time messaging
- SecureStorage for token management
- Async/await patterns throughout

## Project Structure

```
OsmanPrudliev/
├── HouseApp.API/               # Backend API
│   ├── Controllers/            # API endpoints
│   ├── Models/                 # Domain entities
│   ├── DTOs/                   # Data transfer objects
│   ├── Services/               # Business logic
│   ├── Data/                   # EF Core context
│   ├── Hubs/                   # SignalR hubs
│   └── Migrations/             # Database migrations
├── HouseApp/                   # MAUI Frontend
│   ├── Models/                 # Data models
│   ├── ViewModels/             # MVVM ViewModels
│   ├── Views/                  # XAML pages
│   ├── Services/               # API and business services
│   ├── Converters/             # XAML converters
│   └── Constants.cs            # Configuration
└── README.md
```

## Getting Started

### Prerequisites
- .NET 10.0 SDK
- Visual Studio 2022 or Visual Studio Code
- Android SDK (for Android deployment)
- Stripe API keys (test mode)

### Backend Setup

1. Navigate to the API project:
```bash
cd HouseApp.API
```

2. Update `appsettings.json` with your Stripe API keys:
```json
{
  "Stripe": {
    "SecretKey": "your_stripe_secret_key",
    "PublishableKey": "your_stripe_publishable_key"
  }
}
```

3. Apply database migrations:
```bash
dotnet ef database update
```

4. Run the API:
```bash
dotnet run
```

The API will start on `https://localhost:5001` by default.

### Frontend Setup

1. Update `HouseApp/Constants.cs` with your API URL:
```csharp
public static class Constants
{
    public const string ApiBaseUrl = "https://localhost:5001"; // Or your API URL
}
```

2. Build and run the MAUI app:
```bash
cd HouseApp
dotnet build
```

## User Roles

### Student
- Register with email and password
- View assigned house information
- See rent breakdown (rent + utilities + water)
- Make payments via Stripe
- Chat with other students in the same house
- Manage profile

### Landlord
- Register with email and password
- Create and manage multiple houses
- Set monthly rent, utilities, and water bill costs
- Add/remove students from houses
- View payment history for all properties
- Manage profile

## API Endpoints

### Authentication
- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - Login and get JWT token

### Houses (Landlord)
- `GET /api/houses/my-houses` - Get landlord's houses
- `POST /api/houses` - Create new house
- `GET /api/houses/{id}` - Get house details
- `PUT /api/houses/{id}` - Update house
- `GET /api/houses/{id}/tenants` - Get house tenants
- `POST /api/houses/{id}/tenants` - Add tenant
- `DELETE /api/houses/{id}/tenants/{studentId}` - Remove tenant

### Payments
- `POST /api/payments/create-intent` - Create Stripe payment intent
- `POST /api/payments/confirm` - Confirm payment
- `GET /api/payments/student/{studentId}` - Get student payments
- `GET /api/payments/house/{houseId}` - Get house payments
- `GET /api/payments/pending` - Get pending payments

### Profile
- `GET /api/profile` - Get current user profile
- `PUT /api/profile` - Update profile
- `POST /api/profile/picture` - Upload profile picture

### Chat (SignalR Hub)
- `SendMessage(houseId, message)` - Send message to house chat
- `JoinHouseChat(houseId)` - Join house chat room
- `LeaveHouseChat(houseId)` - Leave house chat room
- `GetChatHistory(houseId)` - Get chat history

## Database Schema

### Tables
- **Users**: User accounts with authentication
- **Houses**: Rental properties
- **HouseTenants**: Junction table for house-student relationships
- **Payments**: Payment records with Stripe integration
- **Messages**: Chat messages

### Relationships
- One Landlord → Many Houses
- One House → Many Students (through HouseTenants)
- One House → Many Payments
- One House → Many Messages

## Security

- Passwords hashed with BCrypt
- JWT tokens for authentication
- Role-based authorization
- Secure token storage in MAUI (SecureStorage)
- HTTPS communication
- Input validation on all endpoints

## Testing

### Test Credentials
Create users through the registration page:
1. Register as Landlord
2. Create a house
3. Register as Student
4. Landlord adds student to house

### Stripe Test Cards
Use Stripe test card numbers for payments:
- Success: `4242 4242 4242 4242`
- Decline: `4000 0000 0000 0002`
- Any future date for expiry
- Any 3-digit CVC

## Development Notes

- The MAUI project is configured for Android only in this environment
- SQLite database file: `houseapp.db`
- JWT token expiration: 7 days
- SignalR auto-reconnects on connection loss
- All API calls use async/await patterns
- MVVM pattern with CommunityToolkit.Mvvm

## Troubleshooting

### API Connection Issues
1. Ensure the API is running on the correct port
2. Update `Constants.cs` with the correct API URL
3. Check firewall settings
4. Verify HTTPS certificate (may need to trust development certificate)

### Database Issues
1. Delete `houseapp.db` file
2. Run `dotnet ef database update` again

### Build Issues
1. Clean solution: `dotnet clean`
2. Restore packages: `dotnet restore`
3. Rebuild: `dotnet build`

## Future Enhancements

- Push notifications for payments and messages
- Recurring payment schedules
- Maintenance request system
- Document sharing
- Invoice generation
- Analytics dashboard
- Mobile payment methods (Apple Pay, Google Pay)

## License

This project is part of a coding challenge and is provided as-is for educational purposes.

## Support

For issues or questions, please create an issue in the repository.

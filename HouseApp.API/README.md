# HouseApp API

A complete backend API for managing student housing, built with ASP.NET Core 10.0.

## Features

- **User Authentication**: JWT-based authentication with BCrypt password hashing
- **User Management**: Support for Students and Landlords with profile management
- **House Management**: CRUD operations for houses with tenant management
- **Payment Processing**: Stripe integration for rent, utilities, and water bill payments
- **Real-time Chat**: SignalR-based chat functionality per house
- **SQLite Database**: Lightweight database with Entity Framework Core

## Project Structure

```
HouseApp.API/
├── Models/           # Domain entities (User, House, Payment, etc.)
├── DTOs/            # Data Transfer Objects
├── Data/            # EF Core DbContext
├── Services/        # Business logic (Auth, Payments)
├── Controllers/     # API endpoints
├── Hubs/            # SignalR hubs
└── Migrations/      # Database migrations
```

## Technology Stack

- **.NET 10.0**
- **Entity Framework Core 10.0** with SQLite
- **JWT Authentication**
- **BCrypt.Net** for password hashing
- **Stripe.NET** for payment processing
- **SignalR** for real-time communication

## Setup Instructions

### Prerequisites
- .NET 10 SDK
- Stripe account (for payment testing)

### Quick Start (Database Setup)

1. **Navigate to the API folder:**
   ```bash
   cd HouseApp.API
   ```

2. **Install Entity Framework tools (if not already installed):**
   ```bash
   dotnet tool install --global dotnet-ef
   ```

3. **Run database migrations:**
   ```bash
   dotnet ef database update
   ```
   
   This will create the `houseapp.db` SQLite database file in the HouseApp.API folder.

4. **Start the API:**
   ```bash
   dotnet run
   ```
   
   The API will start on `https://localhost:7001` or `http://localhost:5000`

### Configuration

1. Update `appsettings.json` with your credentials:

```json
{
  "JwtSettings": {
    "SecretKey": "YourSecretKey",
    "Issuer": "HouseApp",
    "Audience": "HouseAppClient"
  },
  "Stripe": {
    "PublishableKey": "pk_test_...",
    "SecretKey": "sk_test_..."
  }
}
```

### Verify Database Created

After running migrations, check for the `houseapp.db` file in the HouseApp.API folder:
```bash
ls -la houseapp.db
```

### Troubleshooting

**If migration fails:**
```bash
# Ensure you're in the HouseApp.API directory
cd HouseApp.API

# Check if EF tools are installed
dotnet ef --version

# If not installed or old version, update
dotnet tool update --global dotnet-ef

# Try migration again
dotnet ef database update
```

**If database already exists but has schema issues:**
```bash
# Drop and recreate (WARNING: This deletes all data)
rm houseapp.db
dotnet ef database update
```

### First Time Setup

After the database is created, you can:
1. Run the MAUI app (HouseApp project)
2. Register a new account (Student or Landlord)
3. Login with your registered credentials
4. Start using the application!

## API Endpoints

### Authentication
- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - Login user

### Profile
- `GET /api/profile/{userId}` - Get user profile
- `PUT /api/profile/{userId}` - Update user profile

### Houses
- `GET /api/houses` - Get all houses
- `GET /api/houses/{id}` - Get house by ID
- `POST /api/houses` - Create house (Landlord only)
- `PUT /api/houses/{id}` - Update house (Landlord only)
- `DELETE /api/houses/{id}` - Delete house (Landlord only)
- `POST /api/houses/{houseId}/tenants/{studentId}` - Add tenant
- `DELETE /api/houses/{houseId}/tenants/{studentId}` - Remove tenant
- `GET /api/houses/{houseId}/messages` - Get house messages

### Payments
- `GET /api/payments/student/{studentId}` - Get student payments
- `GET /api/payments/house/{houseId}` - Get house payments
- `POST /api/payments/create-payment-intent` - Create Stripe payment intent
- `POST /api/payments/{paymentId}/confirm` - Confirm payment

### SignalR Hub
- `/hubs/chat` - Real-time chat hub
  - `JoinHouse(houseId)` - Join house chat room
  - `LeaveHouse(houseId)` - Leave house chat room
  - `SendMessage(messageDto)` - Send message
  - `MarkMessageAsRead(messageId)` - Mark message as read

## Database Schema

### Users
- Id, Email, PasswordHash, UserType, FirstName, LastName, Age, ProfilePictureUrl, DateOfBirth, PhoneNumber, CreatedDate

### Houses
- Id, Name, Address, LandlordId, MonthlyRent, UtilitiesCost, WaterBillCost, CreatedDate, MaxOccupants

### HouseTenants
- Id, HouseId, StudentId, JoinedDate, IsActive

### Payments
- Id, StudentId, HouseId, Amount, PaymentType, Status, StripePaymentIntentId, DueDate, PaidDate, CreatedDate

### Messages
- Id, HouseId, SenderId, SenderName, MessageText, Timestamp, IsRead

## Development

### Run in development mode
```bash
dotnet watch run
```

### Create new migration
```bash
dotnet ef migrations add MigrationName
```

### Apply migrations
```bash
dotnet ef database update
```

## Security Features

- JWT token authentication with 7-day expiration
- BCrypt password hashing with salt
- Role-based authorization (Student/Landlord)
- Secure Stripe payment processing
- CORS configuration for local development

## Notes

- Default CORS allows `http://localhost:3000` and `http://localhost:5173`
- SQLite database file: `houseapp.db`
- Update Stripe keys with real test keys before testing payments
- JWT secret key should be at least 32 characters for production

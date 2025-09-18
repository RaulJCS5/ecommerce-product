# EcommerceProduct API

A modern, secure RESTful API built with ASP.NET Core 9.0 for managing e-commerce products, customers, orders, and reviews. This API is part of a microservices architecture designed for scalable e-commerce platforms.

## Purpose

The EcommerceProduct API provides comprehensive functionality for:

- **Product Management**: Create, read, update, and delete products with categories
- **Customer Management**: Handle customer information and profiles
- **Order Processing**: Manage orders and order items
- **Review System**: Enable product reviews and ratings
- **Authentication**: Secure JWT-based authentication system

## Architecture

- **Framework**: ASP.NET Core 9.0
- **Database**: SQLite with Entity Framework Core
- **Authentication**: JWT Bearer tokens
- **Logging**: Serilog for structured logging
- **Mapping**: AutoMapper for object-to-object mapping
- **API Documentation**: OpenAPI/Swagger integration

## Quick Start

### Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/)

### 1. Clone and Setup

```bash
git clone <repository-url>
cd EcommerceProduct/EcommerceProduct.API
```

### 2. Database Migration

Run the following commands in the Package Manager Console or terminal:

```bash
# Create initial migration
dotnet ef migrations add EcommerceProductDBInitialMigration

# Apply migration to create database
dotnet ef database update
```

### 3. Configure JWT Authentication

#### Generate a Secret Key

```bash
# Navigate to project directory
cd EcommerceProduct.API

# Generate a new JWT signing key
dotnet user-jwts key --reset
```

This command will output a base64-encoded secret key. Copy this key to your `appsettings.Development.json`:

```json
{
  "Authentication": {
    "SecretForKey": "YOUR_GENERATED_SECRET_KEY_HERE",
    "Issuer": "https://localhost:7032",
    "Audience": "ecommerceproductapi"
  }
}
```

### 4. Create a JWT Token

``` bash
# Generate a new signing key if not already done
dotnet user-jwts key --issuer https://localhost:7032
```

```bash
# Create a new JWT token for testing
dotnet user-jwts create --issuer https://localhost:7032 --audience ecommerceproductapi
```

This will generate a JWT token that you can use for authentication. Copy the token for API testing.

Note:

- To generate a new token with claims, use the `--claims` option.

``` bash
dotnet user-jwts create --issuer https://localhost:7032 --audience ecommerceproductapi --claims "city=Altares"
```

### 5. Run the Application

```bash
# Start the API
dotnet run
```

The API will be available at:

- **HTTPS**: `https://localhost:7032`
- **HTTP**: `http://localhost:5253`

## Authentication

This API uses JWT Bearer token authentication. All endpoints (except authentication endpoints) require a valid JWT token.

### User Registration & Authentication

This API supports two authentication methods:

#### Method 1: User Registration & Login (Recommended)

1. **Register a New User**:

```bash
POST https://localhost:7032/api/authentication/register
Content-Type: application/json

{
  "username": "your_username",
  "email": "your@email.com", 
  "password": "SecurePassword123!",
  "firstName": "Your",
  "lastName": "Name",
  "city": "YourCity"
}
```

2. **Login to Get Token**:

```bash
POST https://localhost:7032/api/authentication/login
Content-Type: application/json

{
  "username": "your_username",
  "password": "SecurePassword123!"
}
```

#### Method 2: Development Tokens (For Testing)

```bash
# Create a development JWT token
dotnet user-jwts create --issuer https://localhost:7032 --audience ecommerceproductapi
```

### How to Authenticate

1. **Get a Token**: Register & login or use the dotnet command above
2. **Include in Requests**: Add the token to the `Authorization` header:

```http
Authorization: Bearer YOUR_JWT_TOKEN_HERE
```

### Example API Call

```http
GET https://localhost:7032/api/products
Authorization: YOUR_JWT_TOKEN_HERE
Accept: application/json
```

## API Endpoints

### Authentication

- `POST /api/authentication/register` - Register new user
- `POST /api/authentication/login` - User login (returns token + user info)
- `POST /api/authentication/authenticate` - User Authentication (POST request to obtain JWT token)

### Customers

- `GET /api/customers` - Get all customers
- `GET /api/customers/{id}` - Get customer by ID
- `POST /api/customers` - Create new customer
- `PUT /api/customers/{id}` - Update customer
- `DELETE /api/customers/{id}` - Delete customer

### Products

- `GET /api/products` - Get all products (with pagination)
- `GET /api/products/{id}` - Get product by ID
- `POST /api/products` - Create new product
- `PUT /api/products/{id}` - Update product
- `PATCH /api/products/{id}` - Partial update product
- `DELETE /api/products/{id}` - Delete product

### Product Categories

- `GET /api/productcategories` - Get all categories
- `GET /api/productcategories/{id}` - Get category by ID

### Product Reviews

- `GET /api/products/{productId}/reviews` - Get reviews for product
- `POST /api/products/{productId}/reviews` - Add review to product
- `GET /api/products/{productId}/reviews/{reviewId}` - Get specific review

### Orders

- `GET /api/orders` - Get all orders
- `GET /api/orders/{id}` - Get order by ID
- `POST /api/orders` - Create new order

## Testing the API

### Using the Included HTTP File

Open `EcommerceProduct.API.http` in VS Code and use the REST Client extension:

1. Update the `@jwtToken` variable with your generated token
2. Click "Send Request" on any endpoint

## Development Commands

```bash
# Generate new JWT signing key
dotnet user-jwts key --reset

# Create new JWT token
dotnet user-jwts create --issuer https://localhost:7032 --audience ecommerceproductapi

# List current tokens
dotnet user-jwts list

# Clear all tokens
dotnet user-jwts clear

# Stop running API (if needed)
taskkill /F /IM EcommerceProduct.API.exe

# Run with specific environment
dotnet run --environment Development
```

## Project Structure

```
EcommerceProduct.API/
├── Controllers/          # API controllers
├── DbContexts/          # Entity Framework contexts
├── Entities/            # Domain models
├── Models/              # DTOs and view models
├── Profiles/            # AutoMapper profiles
├── Services/            # Business logic services
├── Migrations/          # Database migrations
├── Properties/          # Launch settings
├── appsettings.json     # Configuration
└── Program.cs           # Application entry point
```

## Configuration

### Database Connection

Update `appsettings.Development.json` for your database:

```json
{
  "ConnectionStrings": {
    "ProductDBConnectionString": "Data Source=EcommerceProduct.db"
  }
}
```

### Authentication Settings

```json
{
  "Authentication": {
    "SecretForKey": "Your-Secret-Key-Here",
    "Issuer": "https://localhost:7032",
    "Audience": "ecommerceproductapi"
  }
}
```

## Troubleshooting

### Common Issues

1. **401 Unauthorized**: Ensure your JWT token is valid and not expired
2. **Database Issues**: Run `dotnet ef database update` to apply migrations
3. **Port Conflicts**: Check if ports 7032/5253 are available
4. **SSL Errors**: Use `https://localhost:7032` for secure connections

### JWT Token Issues

If you encounter authentication problems:

```bash
# Reset everything and start fresh
dotnet user-jwts clear
dotnet user-jwts key --reset
dotnet user-jwts create --issuer https://localhost:7032 --audience ecommerceproductapi
```

Then update your `appsettings.Development.json` with the new secret key.

# Ecommerce Product API

- This project is part of a microservices architecture for an e-commerce platform.

## Migration

- First Migration command:
	- Open Package Manager Console and run the following command to create the initial migration:
		``` bash
			add-migration EcommerceProductDBInitialMigration
		```
- Second command:
	- After creating the migration, apply it to the database using the following command:
		``` bash
			update-database
		```

## Useful Commands

``` bash
# Generate a new JWT signing key
dotnet user-jwts key
# Stop the running API if it's already running
taskkill /F /IM EcommerceProduct.API.exe
# Clear all existing tokens
dotnet user-jwts clear
# List current tokens
dotnet user-jwts list
# Show all details of the JWT tokens
dotnet user-jwts print --show-all
# Reset JWT signing key
dotnet user-jwts key --reset
# Create new JWT token
dotnet user-jwts create --issuer https://localhost:7032 --audience productapi
# List current tokens
dotnet user-jwts list
```

# EF Core Migration Instructions

## Overview
This document provides instructions for creating and applying the initial database migration for the Tourism Platform.

## Prerequisites
- .NET 8.0 SDK installed
- SQL Server or LocalDB installed
- Entity Framework Core Tools installed

## Steps to Create and Apply Migration

### 1. Install EF Core Tools (if not already installed)
```bash
dotnet tool install --global dotnet-ef
```

### 2. Create Initial Migration
Run the following command from the project root directory:
```bash
dotnet ef migrations add InitialCreate
```

This will create a migration file in the `Data/Migrations` folder that contains all the database schema definitions.

### 3. Apply Migration to Database
Run the following command to create the database and apply the migration:
```bash
dotnet ef database update
```

This will:
- Create the TourismPlatformDb database (as specified in appsettings.json)
- Create all tables with proper relationships and constraints
- Set up indexes for unique constraints

## Database Configuration

The connection string is configured in `appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=TourismPlatformDb;Trusted_Connection=true;"
}
```

You can modify this to use:
- SQL Server Express
- Azure SQL Database
- Other SQL Server instances

## Tables Created

The migration will create the following tables:

### Core Tables
- Users
- Districts
- AttractionCategories
- Attractions
- AttractionImages

### Commerce Tables
- Foods
- Hotels
- HotelRoomTypes
- TicketOrders
- HotelOrders
- VerifyCodes

### Community Tables
- Comments
- Ratings
- TravelDiaries
- Favorites

### Admin Tables
- TravelRoutes
- Announcements

### Identity Tables (from ASP.NET Core Identity)
- AspNetUsers
- AspNetRoles
- AspNetUserRoles
- AspNetUserClaims
- AspNetRoleClaims
- AspNetUserLogins
- AspNetUserTokens

## Relationships and Constraints

All relationships are properly configured with:
- Foreign key constraints
- Cascade delete where appropriate
- Restrict delete for referential integrity
- Unique indexes on Email and VerifyCode
- Composite unique index on (UserId, AttractionId) for Favorites

## Verification

After applying the migration, you can verify the database was created correctly by:
1. Opening SQL Server Management Studio
2. Connecting to (localdb)\mssqllocaldb
3. Checking that TourismPlatformDb database exists
4. Verifying all tables are present with correct schema

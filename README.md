# Event Management System - Backend

A secure RESTful API built with ASP.NET Core for managing events and registrations, following Clean Architecture principles.

## Features

- **JWT Authentication**: Secure login with role-based access
- **Role-Based Access Control**:
  - `EventCreator`: Create events and view registrations
  - `EventParticipant`: View events and register
- **Event & Registration Management**: Full CRUD operations
- **SQLite Database**: File-based for easy development
- **Swagger Documentation**: Interactive API docs at `/swagger`

---

## Quick Start

### Install & Run

```bash
dotnet restore
dotnet ef database update
dotnet run
```

### Access API

- **API**: https://localhost:7260  
- **Swagger**: https://localhost:7260/swagger  

### Default Admin Login

- **Email**: `admin@docuware.com`  
- **Password**: `AdminPassword123!`  


## API Endpoints

### Authentication (/api/auth)
- `POST /register` - Create new user account
- `POST /login` - Login and get JWT token

### Events (/api/events)
- `GET /` - Get all events
- `POST /` - Create new event (**Requires: EventCreator role**)
- `GET /{id}` - Get event by ID

### Registrations (/api/events/{id}/registrations)
- `GET /` - Get event registrations (**Requires: EventCreator role**)
- `POST /` - Register for an event

---

## Authentication

1. Register or use the seeded admin account
2. Login to get JWT token
3. Include token in requests:  

   ```
   Authorization: Bearer <token>
   ```

---

## Project Structure

```
Domain/         # Domain models & interfaces
Application/    # Use cases & DTOs  
Infrastructure/ # Database & JWT implementation
Presentation/   # API controllers
```

---

## Database

- SQLite file: `EventRegistration.db`
- Auto-created with seeded admin user on first run by calling DbInitialiser
- Uses EF Core Code-First migrations

---

## Built With

- ASP.NET Core 9.0
- Entity Framework Core
- ASP.NET Core Identity
- JWT Bearer Authentication
- SQLite

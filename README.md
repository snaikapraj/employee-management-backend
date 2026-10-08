# Employee Management System Backend (.NET 10 & PostgreSQL)

A production-ready RESTful Web API built with **.NET 10**, **ASP.NET Core Web API**, **Entity Framework Core 10**, and **PostgreSQL**. The application follows Clean Architecture principles, featuring robust input validation, centralized exception handling, dependency injection, Swagger documentation, development database seeding, unit tests, integration testing guidelines with Testcontainers, and Docker support.

---

## Architecture & Project Structure

The project follows a clean, maintainable, and pragmatic architecture appropriate for CRUD services without unnecessary boilerplate:

```text
EmployeeManagement/
│
├── Controllers/
│   └── EmployeesController.cs         # API REST endpoints & HTTP response handling
│
├── Data/
│   ├── ApplicationDbContext.cs        # EF Core DbContext & PostgreSQL Fluent API mapping
│   └── DbInitializer.cs               # Development seed data initializer
│
├── DTOs/
│   ├── EmployeeDto.cs                 # DTO returned in API responses
│   ├── CreateEmployeeRequest.cs       # Request payload for creating employees
│   ├── UpdateEmployeeRequest.cs       # Request payload for updating employees
│   └── ErrorResponse.cs               # Standard error response structure
│
├── Entities/
│   └── Employee.cs                    # Domain entity representing PostgreSQL Employees table
│
├── Exceptions/
│   ├── DuplicateEmailException.cs     # Thrown when duplicate email constraint is violated
│   └── NotFoundException.cs           # Thrown when a requested resource is missing
│
├── Middleware/
│   └── ExceptionHandlingMiddleware.cs # Global exception middleware for formatted JSON errors
│
├── Migrations/                        # EF Core PostgreSQL database migrations
│   ├── 20261005023002_InitialCreate.cs
│   └── ApplicationDbContextModelSnapshot.cs
│
├── Repositories/                      # Data access repository layer
│   ├── Interfaces/
│   │   └── IEmployeeRepository.cs     # Repository abstraction
│   └── EmployeeRepository.cs          # Repository implementation wrapping EF Core
│
├── Services/
│   ├── Interfaces/
│   │   └── IEmployeeService.cs        # Business logic interface
│   └── EmployeeService.cs             # Business logic implementation
│
├── Validators/
│   ├── CreateEmployeeRequestValidator.cs  # FluentValidation for POST payload
│   └── UpdateEmployeeRequestValidator.cs  # FluentValidation for PUT payload
│
├── Program.cs                         # Application startup, DI container, & pipeline config
├── appsettings.json                   # Production/Default configuration
├── appsettings.Development.json       # Development configuration
└── EmployeeManagement.csproj          # C# 13 & .NET 10 project definition
```

---

## 1. Technology Stack

* **Framework:** .NET 10 (`net10.0`)
* **API:** ASP.NET Core Web API
* **Language:** C# 13 with Nullable Reference Types enabled
* **ORM:** Entity Framework Core 10 (`Microsoft.EntityFrameworkCore`)
* **Database Provider:** `Npgsql.EntityFrameworkCore.PostgreSQL` (v10.0.3)
* **Validation:** FluentValidation (`FluentValidation.DependencyInjectionExtensions`)
* **API Documentation:** Swagger / OpenAPI via Swashbuckle
* **Testing:** xUnit, FluentAssertions, Moq, EF Core InMemory Provider, Testcontainers for PostgreSQL
* **Containerization:** Docker & Docker Compose

---

## 2. PostgreSQL Installation & Database Setup

### Option A: Running via Docker (Recommended)

1. Start PostgreSQL using Docker Compose:
   ```bash
   docker compose up -d
   ```
2. Verify container is running:
   ```bash
   docker ps
   ```

### Option B: Local PostgreSQL Installation

1. **Install PostgreSQL:**
   * **Windows:** Download and install PostgreSQL from [postgresql.org](https://www.postgresql.org/download/windows/).
   * **macOS:** `brew install postgresql`
   * **Linux (Ubuntu/Debian):** `sudo apt install postgresql postgresql-contrib`

2. **Verify PostgreSQL is running:**
   ```bash
   pg_isready -h localhost -p 5432
   ```

3. **Create Database and User:**
   Open `psql` shell:
   ```bash
   psql -U postgres
   ```
   Execute SQL statements:
   ```sql
   CREATE DATABASE "EmployeeManagementDb";
   -- Optional: Create dedicated user
   CREATE USER app_user WITH PASSWORD 'secure_password';
   GRANT ALL PRIVILEGES ON DATABASE "EmployeeManagementDb" TO app_user;
   ```

---

## 3. Configuration & Connection Strings

The database connection string is configured in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=EmployeeManagementDb;Username=postgres;Password=postgres"
  }
}
```

### Environment Variable & User Secrets Overrides

To avoid committing database credentials:

* **Using User Secrets (Local Dev):**
  ```bash
  dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=EmployeeManagementDb;Username=postgres;Password=YOUR_SECRET_PASSWORD" --project EmployeeManagement
  ```
* **Using Environment Variables:**
  ```bash
  # Linux/macOS
  export ConnectionStrings__DefaultConnection="Host=localhost;Port=5432;Database=EmployeeManagementDb;Username=postgres;Password=YOUR_SECRET_PASSWORD"

  # Windows PowerShell
  $env:ConnectionStrings__DefaultConnection="Host=localhost;Port=5432;Database=EmployeeManagementDb;Username=postgres;Password=YOUR_SECRET_PASSWORD"
  ```

---

## 4. Entity Framework Core Configuration

The `Employee` entity is mapped in `ApplicationDbContext.cs` using EF Core Fluent API:

```csharp
modelBuilder.Entity<Employee>(entity =>
{
    entity.ToTable("Employees");
    entity.HasKey(e => e.EmployeeId);
    entity.Property(e => e.EmployeeId).ValueGeneratedOnAdd();
    entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
    entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
    entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
    entity.HasIndex(e => e.Email).IsUnique(); // Unique database constraint
    entity.Property(e => e.Phone).IsRequired().HasMaxLength(20);
    entity.Property(e => e.Department).IsRequired().HasMaxLength(100);
    entity.Property(e => e.Designation).IsRequired().HasMaxLength(150);
    entity.Property(e => e.Salary).IsRequired().HasColumnType("numeric(18,2)");
    entity.Property(e => e.JoiningDate).IsRequired().HasColumnType("date");
    entity.Property(e => e.IsActive).IsRequired().HasDefaultValue(true);
});
```

---

## 5. Database Migrations

### Install EF Core CLI Tool (if not installed)

```bash
dotnet tool install --global dotnet-ef
```

### Create Migration

```bash
dotnet ef migrations add InitialCreate --project EmployeeManagement/EmployeeManagement.csproj
```

### Apply Migration to PostgreSQL

```bash
dotnet ef database update --project EmployeeManagement/EmployeeManagement.csproj
```

### Verification via `psql`

```bash
psql -U postgres -d EmployeeManagementDb -c "\dt"
psql -U postgres -d EmployeeManagementDb -c "\d \"Employees\""
```

---

## 6. Running the Application

1. **Restore dependencies & Build:**
   ```bash
   dotnet restore
   dotnet build
   ```

2. **Run the API:**
   ```bash
   dotnet run --project EmployeeManagement/EmployeeManagement.csproj
   ```

3. **Access Swagger UI:**
   Open your web browser and navigate to:
   * **Swagger:** `https://localhost:7057/swagger` (or `http://localhost:5057/swagger`)

---

## 7. REST API Endpoints Overview

| Method | Endpoint             | Description                | Success Code | Error Codes |
| ------ | -------------------- | -------------------------- | ------------ | ----------- |
| GET    | `/api/employees`     | Retrieve all employees     | 200 OK       | 500         |
| GET    | `/api/employees/{id}` | Retrieve employee by ID    | 200 OK       | 404, 500    |
| POST   | `/api/employees`     | Create a new employee      | 201 Created  | 400, 409, 500|
| PUT    | `/api/employees/{id}` | Update employee details    | 200 OK       | 400, 404, 409, 500|
| DELETE | `/api/employees/{id}` | Delete employee record     | 204 No Content | 404, 500  |

---

## 8. Sample cURL Test Requests

### 1. Create Employee (POST)
```bash
curl -X 'POST' \
  'https://localhost:7057/api/employees' \
  -H 'Content-Type: application/json' \
  -d '{
  "firstName": "Aarav",
  "lastName": "Sharma",
  "email": "aarav@example.com",
  "phone": "9876543210",
  "department": "Engineering",
  "designation": "Software Engineer",
  "salary": 50000,
  "joiningDate": "2025-01-15",
  "isActive": true
}'
```

### 2. Get All Employees (GET)
```bash
curl -X 'GET' 'https://localhost:7057/api/employees'
```

### 3. Get Employee By ID (GET)
```bash
curl -X 'GET' 'https://localhost:7057/api/employees/1'
```

### 4. Update Employee (PUT)
```bash
curl -X 'PUT' \
  'https://localhost:7057/api/employees/1' \
  -H 'Content-Type: application/json' \
  -d '{
  "firstName": "Aarav",
  "lastName": "Sharma",
  "email": "aarav.sharma@example.com",
  "phone": "9876543210",
  "department": "Engineering",
  "designation": "Senior Software Engineer",
  "salary": 75000,
  "joiningDate": "2025-01-15",
  "isActive": true
}'
```

### 5. Delete Employee (DELETE)
```bash
curl -X 'DELETE' 'https://localhost:7057/api/employees/1'
```

### 6. Duplicate Email Test (Conflict 409)
```bash
curl -X 'POST' \
  'https://localhost:7057/api/employees' \
  -H 'Content-Type: application/json' \
  -d '{
  "firstName": "Duplicate",
  "lastName": "User",
  "email": "aarav@example.com",
  "phone": "9876543299",
  "department": "QA",
  "designation": "Tester",
  "salary": 45000,
  "joiningDate": "2025-02-01",
  "isActive": true
}'
```

### 7. Invalid Salary Test (Bad Request 400)
```bash
curl -X 'POST' \
  'https://localhost:7057/api/employees' \
  -H 'Content-Type: application/json' \
  -d '{
  "firstName": "Invalid",
  "lastName": "Salary",
  "email": "invalid.salary@example.com",
  "phone": "9876543210",
  "department": "Sales",
  "designation": "Executive",
  "salary": 0,
  "joiningDate": "2025-01-15",
  "isActive": true
}'
```

---

## 9. Unit Testing

Unit tests are written using **xUnit**, **Moq**, **FluentAssertions**, and EF Core InMemory database provider.

Run tests using CLI:

```bash
dotnet test
```

### Test Coverage:
* `EmployeeServiceTests`: Verify CRUD, duplicate email prevention, and not-found scenarios.
* `EmployeeValidatorTests`: Verify salary, email, name, and required field validation rules.
* `EmployeesControllerTests`: Verify HTTP status codes (200, 201, 204, 400, 404, 409).

---

## 10. Integration Testing with Testcontainers

For realistic PostgreSQL database testing without using in-memory providers:

1. Add `Testcontainers.PostgreSql` package to your test project:
   ```bash
   dotnet add EmployeeManagement.Tests package Testcontainers.PostgreSql
   ```
2. Setup a WebApplicationFactory fixture with Testcontainers:
   ```csharp
   public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
   {
       private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
           .WithImage("postgres:16-alpine")
           .WithDatabase("EmployeeManagementTestDb")
           .WithUsername("postgres")
           .WithPassword("postgres")
           .Build();

       public async Task InitializeAsync() => await _dbContainer.StartAsync();

       protected override void ConfigureWebHost(IWebHostBuilder builder)
       {
           builder.ConfigureServices(services =>
           {
               var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
               if (descriptor != null) services.Remove(descriptor);

               services.AddDbContext<ApplicationDbContext>(options =>
                   options.UseNpgsql(_dbContainer.GetConnectionString()));
           });
       }

       public new async Task DisposeAsync() => await _dbContainer.DisposeAsync();
   }
   ```

---

## 11. Troubleshooting Common PostgreSQL Issues

1. **Connection Refused (`Npgsql.NpgsqlException: Connection refused`):**
   * Ensure PostgreSQL server or Docker container is running (`docker ps` or `pg_isready`).
   * Verify host (`localhost` or `127.0.0.1`) and port (`5432`).

2. **Authentication Failed (`28P01: password authentication failed`):**
   * Check `Username` and `Password` values in `appsettings.json` or environment variables.

3. **Database `EmployeeManagementDb` Does Not Exist (`3D000: database does not exist`):**
   * Run `dotnet ef database update` or `docker compose up -d` to allow auto-creation.

4. **SSL / Transport Errors:**
   * If connecting to a local non-SSL PostgreSQL instance, append `;SSL Mode=Prefer;Trust Server Certificate=true;` to the connection string.

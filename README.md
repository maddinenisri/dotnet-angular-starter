# Mainframe Modernization - Person API

A modern full-stack application demonstrating mainframe to cloud migration using .NET 9, Angular 20, and SQL Server in Docker.

## 🏗️ Architecture

**Clean Architecture** with 4 layers:
- **PersonApi.Domain**: Entities and domain models
- **PersonApi.Infrastructure**: EF Core, DbContext, Repositories
- **PersonApi.Application**: DTOs, Services, Business Logic
- **PersonApi.API**: Controllers, Middleware, API endpoints
- **Web (Angular 20)**: Modern frontend with standalone components

## 🚀 Tech Stack

- **.NET 9.0** - Backend API
- **Entity Framework Core 9** - ORM with SQL Server
- **Angular 20** - Frontend SPA
- **SQL Server (Azure SQL Edge)** - Database (ARM64 compatible for M-series Mac)
- **Docker & Docker Compose** - Containerization
- **Swagger/OpenAPI** - API Documentation
- **xUnit, Moq, FluentAssertions** - Testing

## 📋 Prerequisites

- .NET 9 SDK
- Node.js 18+ & npm
- Angular CLI 20+
- Docker Desktop
- Make (optional, for Makefile commands)

## 🛠️ Quick Start

### 1. Clone and Setup

```bash
cd mainframe-app
make setup
```

### 2. Start All Services with Docker

```bash
make docker-up
```

This command will:
- Build and start SQL Server (Azure SQL Edge)
- Build and start .NET 9 API
- Run EF Core migrations
- Start Angular 20 app (when available)

**Access Points:**
- API: http://localhost:5000
- Swagger UI: http://localhost:5000
- Angular App: http://localhost:4200
- SQL Server: localhost:1433 (sa/YourStrong@Password123)

### 3. Stop Services

```bash
make docker-down
```

## 📖 Available Make Commands

```bash
make help          # Show all available commands
make setup         # Install dependencies
make build         # Build .NET solution
make test          # Run all tests
make docker-up     # Start all services
make docker-down   # Stop all services
make migrate       # Run EF Core migrations
make run-api       # Run API locally (without Docker)
make run-web       # Run Angular app locally
make clean         # Clean build artifacts
make db-reset      # Reset database (WARNING: Deletes data!)
```

## 🗂️ Project Structure

```
mainframe-app/
├── src/
│   ├── PersonApi.API/              # Web API layer
│   ├── PersonApi.Application/       # Business logic layer
│   ├── PersonApi.Domain/            # Domain entities
│   └── PersonApi.Infrastructure/    # Data access layer
├── tests/
│   ├── PersonApi.UnitTests/         # Unit tests
│   └── PersonApi.IntegrationTests/  # Integration tests
├── web/
│   └── person-app/                  # Angular 20 frontend
├── docker/
│   └── sql/                         # SQL initialization scripts
├── docker-compose.yml               # Docker orchestration
├── Makefile                         # Build automation
└── README.md                        # This file
```

## 🔌 API Endpoints

### Persons API

- `GET /api/persons` - Get all persons (paginated)
  - Query params: `pageNumber`, `pageSize`
- `GET /api/persons/{id}` - Get person by ID
- `POST /api/persons` - Create new person
- `PUT /api/persons/{id}` - Update person
- `DELETE /api/persons/{id}` - Delete person
- `GET /api/persons/search?name={name}` - Search by name

### Health & Documentation

- `GET /health` - Health check endpoint
- `GET /` - Swagger UI (Development only)

## 🗃️ Person Entity

```json
{
  "id": 1,
  "name": "John Doe",
  "age": 30,
  "dateOfBirth": "1994-01-15T00:00:00Z",
  "skills": ["C#", ".NET", "Angular"],
  "createdAt": "2025-10-10T10:00:00Z",
  "updatedAt": null
}
```

## 🧪 Running Tests

### All Tests
```bash
make test
```

### Unit Tests Only
```bash
dotnet test tests/PersonApi.UnitTests
```

### Integration Tests Only
```bash
dotnet test tests/PersonApi.IntegrationTests
```

## 🐳 Docker Configuration

### Services

1. **db** (SQL Server - Azure SQL Edge)
   - Port: 1433
   - User: sa
   - Password: YourStrong@Password123
   - ARM64 native support for Apple Silicon

2. **api** (.NET 9 API)
   - Port: 5000
   - Auto-runs migrations on startup
   - Health check enabled

3. **web** (Angular 20)
   - Port: 4200
   - Hot-reload enabled

## 🔧 Development Workflow

### Running Locally (Without Docker)

#### 1. Start SQL Server Only
```bash
docker-compose up db -d
```

#### 2. Run Migrations
```bash
make migrate
```

#### 3. Run API
```bash
make run-api
```

#### 4. Run Angular App (in another terminal)
```bash
make run-web
```

## 🗄️ Database Management

### View Migrations
```bash
dotnet ef migrations list --project src/PersonApi.Infrastructure --startup-project src/PersonApi.API
```

### Add New Migration
```bash
dotnet ef migrations add MigrationName --project src/PersonApi.Infrastructure --startup-project src/PersonApi.API
```

### Reset Database
```bash
make db-reset
```

## 🔐 Security Notes

**⚠️ For Development Only:**
- The SA password is hardcoded for development
- For production, use:
  - Azure Key Vault
  - Kubernetes Secrets
  - Environment-specific configuration

## 📚 Coding Standards

This project follows the coding standards defined in `/Users/srini/Downloads/dotnet-api-guide (1).md`:

- Clean Architecture with separation of concerns
- Async/await throughout
- Repository pattern with generics
- Dependency Injection
- Global exception handling middleware
- Comprehensive logging
- Health checks
- DTOs for data transfer
- XML documentation for Swagger

## 🌐 CORS Configuration

The API is configured to allow requests from:
- `http://localhost:4200` (Angular dev server)

To add more origins, update `Program.cs`:
```csharp
policy.WithOrigins("http://localhost:4200", "https://your-domain.com")
```

## 🐛 Troubleshooting

### SQL Server Connection Issues (Apple Silicon)
- The project uses `azure-sql-edge` for ARM64 compatibility
- If issues persist, allocate more memory to Docker (8+ GB recommended)

### Port Already in Use
```bash
# Find process using port
sudo lsof -i :5000
# Kill process
kill -9 <PID>
```

### EF Core Tools Not Found
```bash
dotnet tool install --global dotnet-ef
export PATH="$PATH:~/.dotnet/tools"
```

### Angular Build Issues
```bash
cd web/person-app
rm -rf node_modules package-lock.json
npm install
```

## 📊 Testing the API

### Using Swagger UI
Navigate to http://localhost:5000 and use the interactive UI

### Using curl
```bash
# Get all persons
curl http://localhost:5000/api/persons

# Create person
curl -X POST http://localhost:5000/api/persons \
  -H "Content-Type: application/json" \
  -d '{
    "name": "John Doe",
    "age": 30,
    "dateOfBirth": "1994-01-15",
    "skills": ["C#", ".NET", "Docker"]
  }'

# Get person by ID
curl http://localhost:5000/api/persons/1
```

## 🚀 Next Steps

1. ✅ Backend API implemented
2. ✅ Docker configuration complete
3. ✅ Database migrations created
4. 🔄 Angular frontend (in progress)
5. 📝 Unit & integration tests
6. 🔐 Add authentication (JWT)
7. 📊 Add logging (Serilog)
8. 🚀 Deploy to cloud (Azure/AWS)

## 📝 License

This project is for demonstration purposes as part of mainframe modernization efforts.

## 👥 Support

For issues or questions, please refer to the coding standards guide or create an issue in the repository.

---

**Built with ❤️ using .NET 9, Angular 20, and modern cloud-native practices**

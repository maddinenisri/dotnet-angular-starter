.PHONY: help setup build test docker-up docker-down migrate clean

# Default target
help:
	@echo "Available targets:"
	@echo "  make setup        - Install all dependencies"
	@echo "  make build        - Build .NET solution"
	@echo "  make test         - Run all tests"
	@echo "  make docker-up    - Start all services with Docker"
	@echo "  make docker-down  - Stop all Docker services"
	@echo "  make migrate      - Run EF Core migrations"
	@echo "  make seed         - Seed database with sample data"
	@echo "  make clean        - Clean build artifacts"
	@echo "  make run-api      - Run API locally (without Docker)"
	@echo "  make run-web      - Run Angular app locally"

# Setup all dependencies
setup:
	@echo "Installing .NET tools..."
	dotnet tool install --global dotnet-ef 2>/dev/null || true
	@echo "Restoring .NET packages..."
	dotnet restore
	@echo "Installing Angular dependencies..."
	cd web/person-app && npm install || echo "Angular app not yet created"
	@echo "Setup complete!"

# Build .NET solution
build:
	@echo "Building .NET solution..."
	dotnet build

# Run all tests
test:
	@echo "Running unit tests..."
	dotnet test tests/PersonApi.UnitTests
	@echo "Running integration tests..."
	dotnet test tests/PersonApi.IntegrationTests

# Start Docker services
docker-up:
	@echo "Starting Docker services..."
	docker-compose up --build -d
	@echo "Waiting for SQL Server to be ready..."
	sleep 20
	@echo "Running migrations..."
	export PATH="$$PATH:/Users/srini/.dotnet/tools" && dotnet ef database update --project src/PersonApi.Infrastructure --startup-project src/PersonApi.API
	@echo "All services started!"
	@echo "API: http://localhost:5000"
	@echo "Swagger: http://localhost:5000"
	@echo "Angular: http://localhost:4200"

# Stop Docker services
docker-down:
	@echo "Stopping Docker services..."
	docker-compose down

# Run EF Core migrations
migrate:
	@echo "Running EF Core migrations..."
	export PATH="$$PATH:/Users/srini/.dotnet/tools" && dotnet ef database update --project src/PersonApi.Infrastructure --startup-project src/PersonApi.API
	@echo "Migrations applied successfully!"

# Seed database
seed:
	@echo "Seeding database..."
	@echo "Database seeding will occur automatically on first run"

# Clean build artifacts
clean:
	@echo "Cleaning build artifacts..."
	dotnet clean
	rm -rf */bin */obj
	@echo "Clean complete!"

# Run API locally (without Docker)
run-api:
	@echo "Starting API locally..."
	@echo "Make sure SQL Server is running (docker-compose up db -d)"
	dotnet run --project src/PersonApi.API

# Run Angular app locally
run-web:
	@echo "Starting Angular app..."
	cd web/person-app && npm start || echo "Angular app not yet created"

# Database reset (WARNING: Deletes all data!)
db-reset:
	@echo "Resetting database..."
	export PATH="$$PATH:/Users/srini/.dotnet/tools" && dotnet ef database drop --force --project src/PersonApi.Infrastructure --startup-project src/PersonApi.API
	export PATH="$$PATH:/Users/srini/.dotnet/tools" && dotnet ef database update --project src/PersonApi.Infrastructure --startup-project src/PersonApi.API
	@echo "Database reset complete!"

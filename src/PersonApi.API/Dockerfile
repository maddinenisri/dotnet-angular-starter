# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["src/PersonApi.API/PersonApi.API.csproj", "src/PersonApi.API/"]
COPY ["src/PersonApi.Application/PersonApi.Application.csproj", "src/PersonApi.Application/"]
COPY ["src/PersonApi.Domain/PersonApi.Domain.csproj", "src/PersonApi.Domain/"]
COPY ["src/PersonApi.Infrastructure/PersonApi.Infrastructure.csproj", "src/PersonApi.Infrastructure/"]
RUN dotnet restore "src/PersonApi.API/PersonApi.API.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/src/PersonApi.API"
RUN dotnet build "PersonApi.API.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "PersonApi.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Create non-root user for security
RUN adduser --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

COPY --from=publish /app/publish .

# Expose port
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Development

# Health check
HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "PersonApi.API.dll"]

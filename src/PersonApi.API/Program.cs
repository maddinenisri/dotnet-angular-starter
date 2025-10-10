using Microsoft.EntityFrameworkCore;
using PersonApi.API.Middleware;
using PersonApi.Application.Interfaces;
using PersonApi.Application.Services;
using PersonApi.Infrastructure.Data;
using PersonApi.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Configure Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// Register Repositories
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IPersonRepository, PersonRepository>();

// Register Services
builder.Services.AddScoped<IPersonService, PersonService>();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .WithExposedHeaders("X-Total-Count", "X-Page-Number", "X-Page-Size");
    });
});

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "Person API",
        Version = "v1",
        Description = "API for managing persons - Mainframe Modernization Demo",
        Contact = new()
        {
            Name = "Person API Support"
        }
    });

    // Enable XML comments for Swagger documentation
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// Add Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>("database");

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Person API V1");
        c.RoutePrefix = string.Empty; // Serve Swagger UI at root
    });
}

// Global Exception Handler Middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseCors("AllowAngularApp");

app.UseAuthorization();

app.MapControllers();

// Health Check Endpoints
app.MapHealthChecks("/health");

// Log application startup
app.Logger.LogInformation("Person API started successfully - Environment: {Environment}", app.Environment.EnvironmentName);
if (app.Environment.IsDevelopment())
{
    app.Logger.LogInformation("Swagger UI available at: http://localhost:5000");
}

await app.RunAsync();

// Make the implicit Program class public for testing
public static partial class Program { }

using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TaskTracker.Api.Data;
using TaskTracker.Api.Middleware;
using TaskTracker.Api.Repositories;
using TaskTracker.Api.Services;

// ─── Cargar variables de entorno desde el .env ───────────────
Env.TraversePath().Load();

var builder = WebApplication.CreateBuilder(args);

// ─── Base de datos (SQL Server via EF Core) ──────────────────
var connectionString =
    $"Server={Environment.GetEnvironmentVariable("DB_SERVER")}," +
    $"{Environment.GetEnvironmentVariable("DB_PORT")};" +
    $"Database={Environment.GetEnvironmentVariable("DB_NAME")};" +
    $"User Id={Environment.GetEnvironmentVariable("DB_USER")};" +
    $"Password={Environment.GetEnvironmentVariable("DB_PASSWORD")};" +
    $"TrustServerCertificate=True;";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// ─── Repositorios ─────────────────────────────────────────────
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ITaskRepository, TaskRepository>();

// ─── Servicios ────────────────────────────────────────────────
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITaskService, TaskService>();

// ─── Autenticación JWT ───────────────────────────────────────
var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET")
    ?? throw new InvalidOperationException("JWT_SECRET no está configurado en el .env");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ValidateIssuer = false,
            ValidateAudience = false,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// ─── CORS ────────────────────────────────────────────────────
var corsOrigin = Environment.GetEnvironmentVariable("CORS_ORIGIN") ?? "http://localhost:3000";
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy.WithOrigins(corsOrigin)
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// ─── Controllers ─────────────────────────────────────────────
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy =
            System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ─── Pipeline de middlewares ──────────────────────────────────
// El manejo de errores debe ir primero para capturar excepciones de toda la pipeline
app.UseMiddleware<ErrorHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

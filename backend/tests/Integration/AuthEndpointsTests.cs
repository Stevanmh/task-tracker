using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TaskTracker.Api.Data;
using TaskTracker.Api.DTOs.Auth;

namespace TaskTracker.Tests.Integration;

/// <summary>
/// Factory personalizada que arranca la app en entorno "Testing".
/// Program.cs detecta ese entorno y usa InMemory en lugar de SQL Server.
/// </summary>
public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Setear entorno a "Testing" ANTES de que la app registre sus servicios.
        // Program.cs detecta esto y usa InMemory en lugar de SQL Server.
        builder.UseEnvironment("Testing");

        // Setear variables de entorno requeridas por la app
        Environment.SetEnvironmentVariable("JWT_SECRET", "integration-test-secret-must-be-32-chars!!");
        Environment.SetEnvironmentVariable("JWT_EXPIRY_HOURS", "1");
        Environment.SetEnvironmentVariable("CORS_ORIGIN", "http://localhost:3000");
    }
}

/// <summary>
/// Pruebas de integración sobre los endpoints de Auth.
/// Se levanta el servidor real en memoria y se hacen requests HTTP reales.
/// </summary>
public class AuthEndpointsTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthEndpointsTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_WithValidData_Returns201Created()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Name = "Test User",
            Email = $"test_{Guid.NewGuid()}@test.com", // email único por test
            Password = "TestPassword123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(body);
        Assert.NotEmpty(body!.Token);
        Assert.Equal(request.Email, body.Email);
    }

    [Fact]
    public async Task Login_WithValidCredentials_Returns200WithToken()
    {
        // Arrange: primero registrar un usuario
        var email = $"login_{Guid.NewGuid()}@test.com";
        var password = "LoginPassword123!";

        await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequest
        {
            Name = "Login Test",
            Email = email,
            Password = password
        });

        // Act: intentar login con esas credenciales
        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = email,
            Password = password
        });

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(body);
        Assert.NotEmpty(body!.Token);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_Returns401()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "noexiste@test.com",
            Password = "WrongPassword"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetTasks_WithoutToken_Returns401Unauthorized()
    {
        // Act: request sin token de autenticación
        var response = await _client.GetAsync("/api/tasks");

        // Assert: el endpoint protegido debe rechazar la request
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}

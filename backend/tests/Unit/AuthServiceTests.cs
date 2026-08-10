using Moq;
using TaskTracker.Api.DTOs.Auth;
using TaskTracker.Api.Models;
using TaskTracker.Api.Repositories;
using TaskTracker.Api.Services;

namespace TaskTracker.Tests.Unit;

/// <summary>
/// Pruebas unitarias para AuthService.
/// Se usa Moq para simular IUserRepository — sin BD real, sin servidor.
/// Se verifica solo la lógica de negocio del servicio de autenticación.
/// </summary>
public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _repoMock;
    private readonly AuthService _service;

    public AuthServiceTests()
    {
        // Configurar el JWT_SECRET en el entorno para que GenerateJwt() funcione
        Environment.SetEnvironmentVariable("JWT_SECRET", "test-secret-key-must-be-at-least-32-chars!!");
        Environment.SetEnvironmentVariable("JWT_EXPIRY_HOURS", "1");

        _repoMock = new Mock<IUserRepository>();
        _service = new AuthService(_repoMock.Object);
    }

    // ─── Register ────────────────────────────────────────────

    [Fact]
    public async Task Register_WithValidData_ReturnsAuthResponseWithToken()
    {
        // Arrange: el email no existe aún en la BD
        _repoMock.Setup(r => r.GetByEmailAsync("nuevo@test.com"))
            .ReturnsAsync((User?)null);

        // CreateAsync devuelve el usuario con ID asignado
        _repoMock.Setup(r => r.CreateAsync(It.IsAny<User>()))
            .ReturnsAsync((User u) => { u.Id = 1; return u; });

        var request = new RegisterRequest
        {
            Name = "Nuevo Usuario",
            Email = "nuevo@test.com",
            Password = "Password123!"
        };

        // Act
        var result = await _service.RegisterAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("nuevo@test.com", result.Email);
        Assert.Equal("Nuevo Usuario", result.Name);
        Assert.NotEmpty(result.Token); // el token debe existir
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ThrowsInvalidOperationException()
    {
        // Arrange: el email YA existe en la BD
        _repoMock.Setup(r => r.GetByEmailAsync("existente@test.com"))
            .ReturnsAsync(new User { Id = 1, Email = "existente@test.com", Name = "Existente" });

        var request = new RegisterRequest
        {
            Name = "Otro",
            Email = "existente@test.com",
            Password = "Password123!"
        };

        // Act & Assert: debe lanzar excepción de negocio
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.RegisterAsync(request));
    }

    [Fact]
    public async Task Register_PasswordNotStoredInPlainText()
    {
        // Arrange
        _repoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((User?)null);

        User? capturedUser = null;
        _repoMock.Setup(r => r.CreateAsync(It.IsAny<User>()))
            .ReturnsAsync((User u) =>
            {
                capturedUser = u;
                u.Id = 1;
                return u;
            });

        // Act
        await _service.RegisterAsync(new RegisterRequest
        {
            Name = "Test",
            Email = "test@test.com",
            Password = "MiContraseña123!"
        });

        // Assert: el hash almacenado NO es la contraseña en texto plano
        Assert.NotNull(capturedUser);
        Assert.NotEqual("MiContraseña123!", capturedUser!.PasswordHash);
        Assert.True(BCrypt.Net.BCrypt.Verify("MiContraseña123!", capturedUser.PasswordHash));
    }

    // ─── Login ───────────────────────────────────────────────

    [Fact]
    public async Task Login_WithCorrectCredentials_ReturnsAuthResponseWithToken()
    {
        // Arrange: usuario existente con contraseña hasheada
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword("CorrectPassword!");
        _repoMock.Setup(r => r.GetByEmailAsync("usuario@test.com"))
            .ReturnsAsync(new User
            {
                Id = 1,
                Name = "Usuario",
                Email = "usuario@test.com",
                PasswordHash = hashedPassword
            });

        // Act
        var result = await _service.LoginAsync(new LoginRequest
        {
            Email = "usuario@test.com",
            Password = "CorrectPassword!"
        });

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.NotEmpty(result.Token);
    }

    [Fact]
    public async Task Login_WithWrongPassword_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword("CorrectPassword!");
        _repoMock.Setup(r => r.GetByEmailAsync("usuario@test.com"))
            .ReturnsAsync(new User
            {
                Id = 1,
                Email = "usuario@test.com",
                PasswordHash = hashedPassword
            });

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _service.LoginAsync(new LoginRequest
            {
                Email = "usuario@test.com",
                Password = "ContraseñaIncorrecta"
            }));
    }

    [Fact]
    public async Task Login_WithNonExistentEmail_ThrowsUnauthorizedAccessException()
    {
        // Arrange: el email no existe en la BD
        _repoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((User?)null);

        // Act & Assert: no debe revelar si el usuario existe o no
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _service.LoginAsync(new LoginRequest
            {
                Email = "noexiste@test.com",
                Password = "cualquierCosa"
            }));
    }
}

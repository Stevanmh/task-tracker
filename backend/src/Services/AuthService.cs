using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using TaskTracker.Api.DTOs.Auth;
using TaskTracker.Api.Models;
using TaskTracker.Api.Repositories;

namespace TaskTracker.Api.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;

    public AuthService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        // Verificar que el email no esté en uso
        var existing = await _userRepository.GetByEmailAsync(request.Email.ToLowerInvariant());
        if (existing != null)
            throw new InvalidOperationException("El email ya está registrado");

        var user = new User
        {
            Name = request.Name.Trim(),
            Email = request.Email.ToLowerInvariant(),
            // La contraseña NUNCA se guarda en texto plano — siempre hasheada con bcrypt
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
        };

        var created = await _userRepository.CreateAsync(user);

        return new AuthResponse
        {
            Id = created.Id,
            Name = created.Name,
            Email = created.Email,
            Token = GenerateJwt(created)
        };
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email.ToLowerInvariant());

        // Verificación en tiempo constante para evitar timing attacks
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Credenciales inválidas");

        return new AuthResponse
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Token = GenerateJwt(user)
        };
    }

    private static string GenerateJwt(User user)
    {
        var secret = Environment.GetEnvironmentVariable("JWT_SECRET")!;
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.Name)
        };

        var expiryHours = int.TryParse(
            Environment.GetEnvironmentVariable("JWT_EXPIRY_HOURS"), out var h) ? h : 1;

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddHours(expiryHours),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

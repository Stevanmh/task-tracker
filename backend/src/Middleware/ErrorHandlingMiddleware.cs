namespace TaskTracker.Api.Middleware;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (InvalidOperationException ex)
        {
            // Violación de regla de negocio (ej: email duplicado)
            _logger.LogWarning("Business rule violation: {Message}", ex.Message);
            context.Response.StatusCode = StatusCodes.Status409Conflict;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            // Credenciales inválidas
            _logger.LogWarning("Unauthorized access: {Message}", ex.Message);
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            // Recurso no encontrado
            _logger.LogWarning("Resource not found: {Message}", ex.Message);
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            // Error inesperado — no exponer detalles internos al cliente
            _logger.LogError(ex, "Unhandled exception");
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new { message = "Error interno del servidor" });
        }
    }
}

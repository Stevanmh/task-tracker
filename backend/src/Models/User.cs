namespace TaskTracker.Api.Models;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navegación: tareas donde este usuario es el responsable
    public ICollection<TaskItem> AssignedTasks { get; set; } = new List<TaskItem>();

    // Navegación: tareas que este usuario creó
    public ICollection<TaskItem> CreatedTasks { get; set; } = new List<TaskItem>();
}

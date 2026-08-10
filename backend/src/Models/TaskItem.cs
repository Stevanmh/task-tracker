using TaskTracker.Api.Models.Enums;

namespace TaskTracker.Api.Models;

public class TaskItem
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TaskItemStatus Status { get; set; } = TaskItemStatus.Pending;
    public TaskItemPriority Priority { get; set; } = TaskItemPriority.Medium;
    public DateTime? Deadline { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // FK: quién creó la tarea (obligatorio)
    public int CreatedById { get; set; }
    public User CreatedBy { get; set; } = null!;

    // FK: quién está asignado (opcional)
    public int? AssignedToId { get; set; }
    public User? AssignedTo { get; set; }
}

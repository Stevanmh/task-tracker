using System.ComponentModel.DataAnnotations;
using TaskTracker.Api.Models.Enums;

namespace TaskTracker.Api.DTOs.Tasks;

public class CreateTaskRequest
{
    [Required(ErrorMessage = "El título es requerido")]
    [MinLength(1)]
    [MaxLength(200, ErrorMessage = "El título no puede superar 200 caracteres")]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2000, ErrorMessage = "La descripción no puede superar 2000 caracteres")]
    public string? Description { get; set; }

    public TaskItemStatus Status { get; set; } = TaskItemStatus.Pending;
    public TaskItemPriority Priority { get; set; } = TaskItemPriority.Medium;
    public int? AssignedToId { get; set; }
    public DateTime? Deadline { get; set; }
}

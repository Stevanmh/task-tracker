using System.ComponentModel.DataAnnotations;
using TaskTracker.Api.Models.Enums;

namespace TaskTracker.Api.DTOs.Tasks;

public class UpdateTaskRequest
{
    [Required(ErrorMessage = "El título es requerido")]
    [MinLength(1)]
    [MaxLength(200, ErrorMessage = "El título no puede superar 200 caracteres")]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    public TaskItemStatus Status { get; set; }
    public TaskItemPriority Priority { get; set; }
    public int? AssignedToId { get; set; }
    public DateTime? Deadline { get; set; }
}

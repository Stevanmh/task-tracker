using TaskTracker.Api.DTOs.Users;
using TaskTracker.Api.Models.Enums;

namespace TaskTracker.Api.DTOs.Tasks;

public class TaskResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TaskItemStatus Status { get; set; }
    public TaskItemPriority Priority { get; set; }
    public DateTime? Deadline { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public UserResponse CreatedBy { get; set; } = null!;
    public UserResponse? AssignedTo { get; set; }
}

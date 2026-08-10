using TaskTracker.Api.Models;
using TaskTracker.Api.Models.Enums;

namespace TaskTracker.Api.Repositories;

public interface ITaskRepository
{
    Task<List<TaskItem>> GetAllAsync(TaskItemStatus? status, TaskItemPriority? priority, int? assignedToId);
    Task<TaskItem?> GetByIdAsync(int id);
    Task<TaskItem> CreateAsync(TaskItem task);
    Task<TaskItem> UpdateAsync(TaskItem task);
    Task DeleteAsync(TaskItem task);
    Task<Dictionary<string, int>> GetStatusCountsAsync();
}

using TaskTracker.Api.DTOs.Dashboard;
using TaskTracker.Api.DTOs.Tasks;

namespace TaskTracker.Api.Services;

public interface ITaskService
{
    Task<List<TaskResponse>> GetAllAsync(
        TaskTracker.Api.Models.Enums.TaskItemStatus? status,
        TaskTracker.Api.Models.Enums.TaskItemPriority? priority,
        int? assignedToId);
    Task<TaskResponse?> GetByIdAsync(int id);
    Task<TaskResponse> CreateAsync(CreateTaskRequest request, int createdById);
    Task<TaskResponse> UpdateAsync(int id, UpdateTaskRequest request);
    Task DeleteAsync(int id);
    Task<DashboardResponse> GetDashboardAsync();
}

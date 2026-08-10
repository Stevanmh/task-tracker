using TaskTracker.Api.DTOs.Dashboard;
using TaskTracker.Api.DTOs.Tasks;
using TaskTracker.Api.DTOs.Users;
using TaskTracker.Api.Models;
using TaskTracker.Api.Models.Enums;
using TaskTracker.Api.Repositories;

namespace TaskTracker.Api.Services;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _taskRepository;

    public TaskService(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }

    public async Task<List<TaskResponse>> GetAllAsync(
        TaskItemStatus? status, TaskItemPriority? priority, int? assignedToId)
    {
        var tasks = await _taskRepository.GetAllAsync(status, priority, assignedToId);
        return tasks.Select(MapToResponse).ToList();
    }

    public async Task<TaskResponse?> GetByIdAsync(int id)
    {
        var task = await _taskRepository.GetByIdAsync(id);
        return task == null ? null : MapToResponse(task);
    }

    public async Task<TaskResponse> CreateAsync(CreateTaskRequest request, int createdById)
    {
        var task = new TaskItem
        {
            Title = request.Title.Trim(),
            Description = request.Description?.Trim(),
            Status = request.Status,
            Priority = request.Priority,
            AssignedToId = request.AssignedToId,
            Deadline = request.Deadline,
            CreatedById = createdById
        };

        var created = await _taskRepository.CreateAsync(task);
        // Recargamos con relaciones para la respuesta completa
        var withRelations = await _taskRepository.GetByIdAsync(created.Id);
        return MapToResponse(withRelations!);
    }

    public async Task<TaskResponse> UpdateAsync(int id, UpdateTaskRequest request)
    {
        var task = await _taskRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Tarea con id {id} no encontrada");

        task.Title = request.Title.Trim();
        task.Description = request.Description?.Trim();
        task.Status = request.Status;
        task.Priority = request.Priority;
        task.AssignedToId = request.AssignedToId;
        task.Deadline = request.Deadline;

        await _taskRepository.UpdateAsync(task);
        var withRelations = await _taskRepository.GetByIdAsync(id);
        return MapToResponse(withRelations!);
    }

    public async Task DeleteAsync(int id)
    {
        var task = await _taskRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Tarea con id {id} no encontrada");
        await _taskRepository.DeleteAsync(task);
    }

    public async Task<DashboardResponse> GetDashboardAsync()
    {
        var counts = await _taskRepository.GetStatusCountsAsync();
        return new DashboardResponse
        {
            TotalTasks = counts.Values.Sum(),
            PendingTasks = counts.GetValueOrDefault("Pending"),
            InProgressTasks = counts.GetValueOrDefault("InProgress"),
            DoneTasks = counts.GetValueOrDefault("Done")
        };
    }

    private static TaskResponse MapToResponse(TaskItem task) => new()
    {
        Id = task.Id,
        Title = task.Title,
        Description = task.Description,
        Status = task.Status,
        Priority = task.Priority,
        Deadline = task.Deadline,
        CreatedAt = task.CreatedAt,
        UpdatedAt = task.UpdatedAt,
        CreatedBy = new UserResponse
        {
            Id = task.CreatedBy.Id,
            Name = task.CreatedBy.Name,
            Email = task.CreatedBy.Email
        },
        AssignedTo = task.AssignedTo == null ? null : new UserResponse
        {
            Id = task.AssignedTo.Id,
            Name = task.AssignedTo.Name,
            Email = task.AssignedTo.Email
        }
    };
}

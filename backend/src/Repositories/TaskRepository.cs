using Microsoft.EntityFrameworkCore;
using TaskTracker.Api.Data;
using TaskTracker.Api.Models;
using TaskTracker.Api.Models.Enums;

namespace TaskTracker.Api.Repositories;

public class TaskRepository : ITaskRepository
{
    private readonly AppDbContext _context;

    public TaskRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<TaskItem>> GetAllAsync(TaskItemStatus? status, TaskItemPriority? priority, int? assignedToId)
    {
        var query = _context.Tasks
            .Include(t => t.CreatedBy)
            .Include(t => t.AssignedTo)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(t => t.Status == status.Value);

        if (priority.HasValue)
            query = query.Where(t => t.Priority == priority.Value);

        if (assignedToId.HasValue)
            query = query.Where(t => t.AssignedToId == assignedToId.Value);

        return await query.OrderByDescending(t => t.CreatedAt).ToListAsync();
    }

    public async Task<TaskItem?> GetByIdAsync(int id)
        => await _context.Tasks
            .Include(t => t.CreatedBy)
            .Include(t => t.AssignedTo)
            .FirstOrDefaultAsync(t => t.Id == id);

    public async Task<TaskItem> CreateAsync(TaskItem task)
    {
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();
        return task;
    }

    public async Task<TaskItem> UpdateAsync(TaskItem task)
    {
        task.UpdatedAt = DateTime.UtcNow;
        _context.Tasks.Update(task);
        await _context.SaveChangesAsync();
        return task;
    }

    public async Task DeleteAsync(TaskItem task)
    {
        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync();
    }

    public async Task<Dictionary<string, int>> GetStatusCountsAsync()
    {
        return await _context.Tasks
            .GroupBy(t => t.Status)
            .Select(g => new { Status = g.Key.ToString(), Count = g.Count() })
            .ToDictionaryAsync(x => x.Status, x => x.Count);
    }
}

using Moq;
using TaskTracker.Api.DTOs.Tasks;
using TaskTracker.Api.Models;
using TaskTracker.Api.Models.Enums;
using TaskTracker.Api.Repositories;
using TaskTracker.Api.Services;

namespace TaskTracker.Tests.Unit;

/// <summary>
/// Pruebas unitarias para TaskService.
/// Se usa Moq para simular ITaskRepository — sin BD real, sin servidor.
/// Se verifica la lógica de negocio del módulo de tareas: creación,
/// consulta, actualización, eliminación y métricas del dashboard.
/// </summary>
public class TaskServiceTests
{
    private readonly Mock<ITaskRepository> _repoMock;
    private readonly TaskService _service;

    public TaskServiceTests()
    {
        _repoMock = new Mock<ITaskRepository>();
        _service = new TaskService(_repoMock.Object);
    }

    /// <summary>
    /// Helper: crea un TaskItem con relaciones populadas.
    /// MapToResponse accede a CreatedBy — debe estar inicializado.
    /// </summary>
    private static TaskItem MakeTask(
        int id = 1,
        string title = "Test Task",
        TaskItemStatus status = TaskItemStatus.Pending,
        TaskItemPriority priority = TaskItemPriority.Medium) => new()
    {
        Id = id,
        Title = title,
        Status = status,
        Priority = priority,
        CreatedById = 1,
        CreatedBy = new User { Id = 1, Name = "Creator", Email = "creator@test.com" }
    };

    // ─── GetAllAsync ──────────────────────────────────────────

    [Fact]
    public async Task GetAllAsync_ReturnsAllTasks_WhenNoFilters()
    {
        // Arrange
        var tasks = new List<TaskItem> { MakeTask(1, "Tarea A"), MakeTask(2, "Tarea B") };
        _repoMock.Setup(r => r.GetAllAsync(null, null, null)).ReturnsAsync(tasks);

        // Act
        var result = await _service.GetAllAsync(null, null, null);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("Tarea A", result[0].Title);
        Assert.Equal("Tarea B", result[1].Title);
    }

    [Fact]
    public async Task GetAllAsync_PassesFiltersToRepository()
    {
        // Arrange
        _repoMock
            .Setup(r => r.GetAllAsync(TaskItemStatus.Done, TaskItemPriority.High, 5))
            .ReturnsAsync(new List<TaskItem>());

        // Act
        await _service.GetAllAsync(TaskItemStatus.Done, TaskItemPriority.High, 5);

        // Assert: los filtros llegan exactamente al repositorio
        _repoMock.Verify(
            r => r.GetAllAsync(TaskItemStatus.Done, TaskItemPriority.High, 5),
            Times.Once);
    }

    // ─── GetByIdAsync ─────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_ReturnsTask_WhenFound()
    {
        // Arrange
        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(MakeTask(1, "Mi Tarea"));

        // Act
        var result = await _service.GetByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Mi Tarea", result.Title);
        Assert.Equal(1, result.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
    {
        // Arrange
        _repoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((TaskItem?)null);

        // Act
        var result = await _service.GetByIdAsync(99);

        // Assert
        Assert.Null(result);
    }

    // ─── CreateAsync ──────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_TrimsTitle_BeforePersisting()
    {
        // Arrange: título con espacios al inicio y final
        TaskItem? capturedTask = null;

        _repoMock.Setup(r => r.CreateAsync(It.IsAny<TaskItem>()))
            .ReturnsAsync((TaskItem t) =>
            {
                capturedTask = t;
                t.Id = 1;
                return t;
            });

        // GetByIdAsync se llama para recargar con relaciones
        _repoMock.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(MakeTask(1, "Tarea con espacios"));

        // Act
        await _service.CreateAsync(
            new CreateTaskRequest { Title = "  Tarea con espacios  " },
            createdById: 1);

        // Assert: el título persiste sin espacios
        Assert.NotNull(capturedTask);
        Assert.Equal("Tarea con espacios", capturedTask!.Title);
    }

    [Fact]
    public async Task CreateAsync_AssignsCreatedById_Correctly()
    {
        // Arrange
        TaskItem? capturedTask = null;

        _repoMock.Setup(r => r.CreateAsync(It.IsAny<TaskItem>()))
            .ReturnsAsync((TaskItem t) =>
            {
                capturedTask = t;
                t.Id = 1;
                return t;
            });

        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(MakeTask(1));

        // Act — el creador es el usuario con id 42
        await _service.CreateAsync(
            new CreateTaskRequest { Title = "Nueva Tarea" },
            createdById: 42);

        // Assert: la tarea queda asociada al usuario que la creó
        Assert.NotNull(capturedTask);
        Assert.Equal(42, capturedTask!.CreatedById);
    }

    // ─── UpdateAsync ──────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_ThrowsKeyNotFoundException_WhenTaskDoesNotExist()
    {
        // Arrange: la tarea no existe
        _repoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((TaskItem?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _service.UpdateAsync(99, new UpdateTaskRequest { Title = "No importa" }));
    }

    [Fact]
    public async Task UpdateAsync_UpdatesFields_WhenTaskExists()
    {
        // Arrange
        var existing = MakeTask(1, "Título original");
        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existing);
        _repoMock.Setup(r => r.UpdateAsync(It.IsAny<TaskItem>())).ReturnsAsync(existing);

        // Act
        await _service.UpdateAsync(1, new UpdateTaskRequest
        {
            Title = "  Título actualizado  ",
            Status = TaskItemStatus.Done,
            Priority = TaskItemPriority.High
        });

        // Assert: el repositorio recibe la entidad con los valores correctos (título trimado)
        _repoMock.Verify(r => r.UpdateAsync(It.Is<TaskItem>(t =>
            t.Title == "Título actualizado" &&
            t.Status == TaskItemStatus.Done &&
            t.Priority == TaskItemPriority.High)), Times.Once);
    }

    // ─── DeleteAsync ──────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_ThrowsKeyNotFoundException_WhenTaskDoesNotExist()
    {
        // Arrange
        _repoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((TaskItem?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.DeleteAsync(99));
    }

    [Fact]
    public async Task DeleteAsync_CallsRepositoryDelete_WhenTaskExists()
    {
        // Arrange
        var existing = MakeTask(1);
        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existing);
        _repoMock.Setup(r => r.DeleteAsync(existing)).Returns(Task.CompletedTask);

        // Act
        await _service.DeleteAsync(1);

        // Assert: se invocó Delete con la entidad correcta
        _repoMock.Verify(r => r.DeleteAsync(existing), Times.Once);
    }

    // ─── GetDashboardAsync ────────────────────────────────────

    [Fact]
    public async Task GetDashboardAsync_ReturnsCorrectAggregatedCounts()
    {
        // Arrange
        var counts = new Dictionary<string, int>
        {
            { "Pending", 3 },
            { "InProgress", 2 },
            { "Done", 5 }
        };
        _repoMock.Setup(r => r.GetStatusCountsAsync()).ReturnsAsync(counts);

        // Act
        var result = await _service.GetDashboardAsync();

        // Assert: el total es la suma de todos los estados
        Assert.Equal(10, result.TotalTasks);
        Assert.Equal(3, result.PendingTasks);
        Assert.Equal(2, result.InProgressTasks);
        Assert.Equal(5, result.DoneTasks);
    }
}

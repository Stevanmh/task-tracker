using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskTracker.Api.DTOs.Dashboard;
using TaskTracker.Api.Services;

namespace TaskTracker.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly ITaskService _taskService;

    public DashboardController(ITaskService taskService)
    {
        _taskService = taskService;
    }

    /// <summary>Obtener métricas de tareas por estado</summary>
    [HttpGet]
    [ProducesResponseType(typeof(DashboardResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get()
    {
        var dashboard = await _taskService.GetDashboardAsync();
        return Ok(dashboard);
    }
}

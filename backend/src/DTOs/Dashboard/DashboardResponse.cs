namespace TaskTracker.Api.DTOs.Dashboard;

public class DashboardResponse
{
    public int TotalTasks { get; set; }
    public int PendingTasks { get; set; }
    public int InProgressTasks { get; set; }
    public int DoneTasks { get; set; }
}

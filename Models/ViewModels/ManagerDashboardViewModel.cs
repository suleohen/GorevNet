
namespace GorevNet.Models.ViewModels{
    public class ManagerDashboardViewModel{
        public int TotalEmployees { get; set; }
        public int ActiveTasks { get; set; }
        public int PendingTasks { get; set; }
        public int OverdueTasks { get; set; }
        public List<Employee> RecentEmployees { get; set; } = new List<Employee>();
        public int CompletedTasks { get; set; }
        public int OngoingTasks { get; set; }
        public double TaskCompletionRate => TotalTasks > 0 ? (double)CompletedTasks / TotalTasks * 100 : 0;
        public int TotalTasks => ActiveTasks + CompletedTasks;}}
using ISS.Tracker.Core.DTOs;

namespace ISS.Tracker.Web.Models.ViewModels;

public class DashboardViewModel
{
    public IssPositionDto? IssPosition { get; set; }
    public int PeopleInSpace { get; set; }
    public List<LaunchDto> UpcomingLaunches { get; set; } = new();
}

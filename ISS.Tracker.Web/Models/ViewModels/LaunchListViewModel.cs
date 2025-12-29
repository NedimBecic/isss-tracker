using ISS.Tracker.Core.DTOs;

namespace ISS.Tracker.Web.Models.ViewModels;

public class LaunchListViewModel
{
    public List<LaunchDto> Launches { get; set; } = new();
}

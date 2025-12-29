using ISS.Tracker.Core.DTOs;
using ISS.Tracker.Core.Entities;

namespace ISS.Tracker.Web.Models.ViewModels;

public class AnalyticsViewModel
{
    public LaunchAnalyticsDto CurrentAnalytics { get; set; } = new();
    public List<LaunchStatistics> HistoricalStats { get; set; } = new();
    public LaunchStatistics? YesterdayStats { get; set; }

    public int TotalLaunchesChange => YesterdayStats != null
        ? CurrentAnalytics.TotalLaunches - YesterdayStats.TotalLaunches
        : 0;

    public int UpcomingLaunchesChange => YesterdayStats != null
        ? CurrentAnalytics.UpcomingLaunches - YesterdayStats.UpcomingLaunches
        : 0;
}

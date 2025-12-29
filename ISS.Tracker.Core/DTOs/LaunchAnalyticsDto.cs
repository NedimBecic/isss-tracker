namespace ISS.Tracker.Core.DTOs;

public class LaunchAnalyticsDto
{
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

    public int TotalLaunches { get; set; }
    public int UpcomingLaunches { get; set; }
    public int SuccessfulLaunches { get; set; }
    public int FailedLaunches { get; set; }
    public int TbdLaunches { get; set; }

    public List<ProviderStatDto> LaunchesByProvider { get; set; } = new();
    public List<MonthlyStatDto> LaunchesByMonth { get; set; } = new();

    public int LaunchesNext7Days { get; set; }
    public int LaunchesPrevious7Days { get; set; }

    public string MostActiveProvider { get; set; } = string.Empty;
    public int MostActiveProviderCount { get; set; }

    public double AverageLaunchesPerMonth { get; set; }
}

public class ProviderStatDto
{
    public string Provider { get; set; } = string.Empty;
    public int Count { get; set; }
    public double Percentage { get; set; }
}

public class MonthlyStatDto
{
    public string Month { get; set; } = string.Empty;
    public int Year { get; set; }
    public int Count { get; set; }
}

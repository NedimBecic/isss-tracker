using System.ComponentModel.DataAnnotations;

namespace ISS.Tracker.Core.Entities;

public class LaunchStatistics
{
    public int Id { get; set; }

    [Required]
    public DateTime Date { get; set; }

    public int TotalLaunches { get; set; }

    public int UpcomingLaunches { get; set; }

    public int SuccessfulLaunches { get; set; }

    public int FailedLaunches { get; set; }

    public int TbdLaunches { get; set; }

    [MaxLength(2000)]
    public string? LaunchesByProviderJson { get; set; }

    [MaxLength(2000)]
    public string? LaunchesByMonthJson { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

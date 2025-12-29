using System.ComponentModel.DataAnnotations;

namespace ISS.Tracker.Core.Entities;

public class Launch
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string LaunchLibraryId { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string Name { get; set; } = string.Empty;

    public DateTime? LaunchDate { get; set; }

    public LaunchStatus Status { get; set; }

    [MaxLength(200)]
    public string? RocketName { get; set; }

    [MaxLength(200)]
    public string? LaunchProvider { get; set; }

    [MaxLength(5000)]
    public string? MissionDescription { get; set; }

    [MaxLength(1000)]
    public string? ImageUrl { get; set; }

    [MaxLength(1000)]
    public string? VideoUrl { get; set; }

    [MaxLength(500)]
    public string? LaunchSite { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

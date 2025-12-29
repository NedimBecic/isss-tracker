using ISS.Tracker.Core.Entities;

namespace ISS.Tracker.Core.DTOs;

public record LaunchDto(
    int Id,
    string LaunchLibraryId,
    string Name,
    DateTime? LaunchDate,
    LaunchStatus Status,
    string? RocketName,
    string? LaunchProvider,
    string? MissionDescription,
    string? ImageUrl,
    string? VideoUrl,
    string? LaunchSite,
    string TimeUntilLaunch, // Computed: "T-3 days", "T-2 hours", etc.
    bool IsFavorite = false
);

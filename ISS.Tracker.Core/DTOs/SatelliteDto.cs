using ISS.Tracker.Core.Entities;

namespace ISS.Tracker.Core.DTOs;

public record SatelliteDto(
    int Id,
    int NoradId,
    string Name,
    SatelliteType Type,
    DateTime? LaunchDate,
    string? Description,
    bool IsFavorite = false
);

namespace ISS.Tracker.Core.DTOs;

public record FlyoverDto(
    DateTime RiseTime,
    DateTime SetTime,
    int DurationSeconds,
    double MaxElevationDegrees
);

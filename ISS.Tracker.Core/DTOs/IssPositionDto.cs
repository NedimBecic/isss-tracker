namespace ISS.Tracker.Core.DTOs;

public record IssPositionDto(
    double Latitude,
    double Longitude,
    double Altitude,
    double Velocity,
    DateTime Timestamp,
    string Visibility // "daylight" or "eclipsed"
);

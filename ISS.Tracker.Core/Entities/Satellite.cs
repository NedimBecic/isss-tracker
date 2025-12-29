using System.ComponentModel.DataAnnotations;

namespace ISS.Tracker.Core.Entities;

public class Satellite
{
    public int Id { get; set; }

    [Required]
    public int NoradId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public SatelliteType Type { get; set; }

    public DateTime? LaunchDate { get; set; }

    [MaxLength(2000)]
    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

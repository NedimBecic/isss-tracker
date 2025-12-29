using ISS.Tracker.Core.DTOs;

namespace ISS.Tracker.Web.Models.ViewModels;

public class SatelliteTrackViewModel
{
    public IssPositionDto? CurrentPosition { get; set; }
    public List<FlyoverDto> Flyovers { get; set; } = new();
}

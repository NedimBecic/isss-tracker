namespace ISS.Tracker.Web.Models.ViewModels;

public class LocationRequest
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public int Passes { get; set; } = 5;
}

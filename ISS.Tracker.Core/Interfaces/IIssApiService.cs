using ISS.Tracker.Core.DTOs;

namespace ISS.Tracker.Core.Interfaces;

public interface IIssApiService
{
    Task<IssPositionDto?> GetCurrentPositionAsync();
    Task<List<FlyoverDto>> GetUpcomingFlyoversAsync(double latitude, double longitude, int passes = 5);
    Task<int> GetNumberOfPeopleInSpaceAsync();
}

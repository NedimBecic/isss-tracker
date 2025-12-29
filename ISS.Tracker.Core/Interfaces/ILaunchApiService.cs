using ISS.Tracker.Core.DTOs;

namespace ISS.Tracker.Core.Interfaces;

public interface ILaunchApiService
{
    Task<List<LaunchDto>> GetUpcomingLaunchesAsync(int limit = 10);
    Task<LaunchDto?> GetLaunchDetailsAsync(string launchLibraryId);
    Task SyncLaunchesToDatabaseAsync();
}

using ISS.Tracker.Core.DTOs;
using ISS.Tracker.Core.Entities;

namespace ISS.Tracker.Core.Interfaces;

public interface ILaunchAnalyticsService
{
    Task<LaunchAnalyticsDto> GetCurrentAnalyticsAsync();
    Task<LaunchStatistics> GenerateAndSaveStatisticsAsync();
    Task<LaunchStatistics?> GetStatisticsForDateAsync(DateTime date);
    Task<List<LaunchStatistics>> GetStatisticsHistoryAsync(int days = 7);
}

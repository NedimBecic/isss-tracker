using System.Text.Json;
using ISS.Tracker.Core.DTOs;
using ISS.Tracker.Core.Entities;
using ISS.Tracker.Core.Interfaces;
using ISS.Tracker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace ISS.Tracker.Infrastructure.Services;

public class LaunchAnalyticsService : ILaunchAnalyticsService
{
    private readonly ApplicationDbContext _context;
    private readonly IMemoryCache _cache;
    private const string AnalyticsCacheKey = "launch_analytics";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(30);

    public LaunchAnalyticsService(ApplicationDbContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<LaunchAnalyticsDto> GetCurrentAnalyticsAsync()
    {
        if (_cache.TryGetValue(AnalyticsCacheKey, out LaunchAnalyticsDto? cachedAnalytics) && cachedAnalytics != null)
        {
            return cachedAnalytics;
        }

        var analytics = await CalculateAnalyticsAsync();
        _cache.Set(AnalyticsCacheKey, analytics, CacheDuration);
        return analytics;
    }

    private async Task<LaunchAnalyticsDto> CalculateAnalyticsAsync()
    {
        var launches = await _context.Launches.ToListAsync();
        var now = DateTime.UtcNow;

        var analytics = new LaunchAnalyticsDto
        {
            GeneratedAt = now,
            TotalLaunches = launches.Count,
            UpcomingLaunches = launches.Count(l => IsUpcoming(l.Status)),
            SuccessfulLaunches = launches.Count(l => l.Status == LaunchStatus.Success),
            FailedLaunches = launches.Count(l => l.Status == LaunchStatus.Failed || l.Status == LaunchStatus.PartialFailure),
            TbdLaunches = launches.Count(l => l.Status == LaunchStatus.ToBeDetermined)
        };

        var providerGroups = launches
            .Where(l => !string.IsNullOrEmpty(l.LaunchProvider))
            .GroupBy(l => l.LaunchProvider!)
            .OrderByDescending(g => g.Count())
            .Take(10)
            .ToList();

        analytics.LaunchesByProvider = providerGroups.Select(g => new ProviderStatDto
        {
            Provider = g.Key,
            Count = g.Count(),
            Percentage = launches.Count > 0 ? Math.Round((double)g.Count() / launches.Count * 100, 1) : 0
        }).ToList();

        if (analytics.LaunchesByProvider.Any())
        {
            var topProvider = analytics.LaunchesByProvider.First();
            analytics.MostActiveProvider = topProvider.Provider;
            analytics.MostActiveProviderCount = topProvider.Count;
        }

        var monthlyGroups = launches
            .Where(l => l.LaunchDate.HasValue)
            .GroupBy(l => new { l.LaunchDate!.Value.Year, l.LaunchDate!.Value.Month })
            .OrderByDescending(g => g.Key.Year)
            .ThenByDescending(g => g.Key.Month)
            .Take(12)
            .ToList();

        analytics.LaunchesByMonth = monthlyGroups.Select(g => new MonthlyStatDto
        {
            Month = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM"),
            Year = g.Key.Year,
            Count = g.Count()
        }).ToList();

        if (monthlyGroups.Any())
        {
            analytics.AverageLaunchesPerMonth = Math.Round(monthlyGroups.Average(g => g.Count()), 1);
        }

        var next7Days = now.AddDays(7);
        var prev7Days = now.AddDays(-7);

        analytics.LaunchesNext7Days = launches.Count(l =>
            l.LaunchDate.HasValue && l.LaunchDate.Value >= now && l.LaunchDate.Value <= next7Days);

        analytics.LaunchesPrevious7Days = launches.Count(l =>
            l.LaunchDate.HasValue && l.LaunchDate.Value >= prev7Days && l.LaunchDate.Value < now);

        return analytics;
    }

    private bool IsUpcoming(LaunchStatus status)
    {
        return status == LaunchStatus.Scheduled || status == LaunchStatus.ToBeDetermined;
    }

    public async Task<LaunchStatistics> GenerateAndSaveStatisticsAsync()
    {
        var analytics = await CalculateAnalyticsAsync();
        var today = DateTime.UtcNow.Date;

        var existingStats = await _context.Set<LaunchStatistics>()
            .FirstOrDefaultAsync(s => s.Date.Date == today);

        if (existingStats != null)
        {
            existingStats.TotalLaunches = analytics.TotalLaunches;
            existingStats.UpcomingLaunches = analytics.UpcomingLaunches;
            existingStats.SuccessfulLaunches = analytics.SuccessfulLaunches;
            existingStats.FailedLaunches = analytics.FailedLaunches;
            existingStats.TbdLaunches = analytics.TbdLaunches;
            existingStats.LaunchesByProviderJson = JsonSerializer.Serialize(analytics.LaunchesByProvider);
            existingStats.LaunchesByMonthJson = JsonSerializer.Serialize(analytics.LaunchesByMonth);
        }
        else
        {
            existingStats = new LaunchStatistics
            {
                Date = today,
                TotalLaunches = analytics.TotalLaunches,
                UpcomingLaunches = analytics.UpcomingLaunches,
                SuccessfulLaunches = analytics.SuccessfulLaunches,
                FailedLaunches = analytics.FailedLaunches,
                TbdLaunches = analytics.TbdLaunches,
                LaunchesByProviderJson = JsonSerializer.Serialize(analytics.LaunchesByProvider),
                LaunchesByMonthJson = JsonSerializer.Serialize(analytics.LaunchesByMonth)
            };
            _context.Set<LaunchStatistics>().Add(existingStats);
        }

        await _context.SaveChangesAsync();
        return existingStats;
    }

    public async Task<LaunchStatistics?> GetStatisticsForDateAsync(DateTime date)
    {
        return await _context.Set<LaunchStatistics>()
            .FirstOrDefaultAsync(s => s.Date.Date == date.Date);
    }

    public async Task<List<LaunchStatistics>> GetStatisticsHistoryAsync(int days = 7)
    {
        var fromDate = DateTime.UtcNow.Date.AddDays(-days);
        return await _context.Set<LaunchStatistics>()
            .Where(s => s.Date >= fromDate)
            .OrderByDescending(s => s.Date)
            .ToListAsync();
    }
}

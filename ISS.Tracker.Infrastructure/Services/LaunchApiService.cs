using System.Text.Json;
using ISS.Tracker.Core.DTOs;
using ISS.Tracker.Core.Entities;
using ISS.Tracker.Core.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ISS.Tracker.Infrastructure.Services;

public class LaunchApiService : ILaunchApiService
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly ILaunchRepository _launchRepository;
    private readonly ILogger<LaunchApiService> _logger;
    private readonly string _launchLibraryBaseUrl;
    private readonly int _launchesCacheHours;

    private const string LaunchesCacheKey = "upcoming_launches";

    public LaunchApiService(
        HttpClient httpClient,
        IMemoryCache cache,
        ILaunchRepository launchRepository,
        IConfiguration configuration,
        ILogger<LaunchApiService> logger)
    {
        _httpClient = httpClient;
        _cache = cache;
        _launchRepository = launchRepository;
        _logger = logger;
        _launchLibraryBaseUrl = configuration["ApiSettings:LaunchLibrary"] ?? "https://ll.thespacedevs.com/2.2.0";
        _launchesCacheHours = int.Parse(configuration["CacheSettings:LaunchesCacheHours"] ?? "12");
    }

    public async Task<List<LaunchDto>> GetUpcomingLaunchesAsync(int limit = 10)
    {
        var cacheKey = $"{LaunchesCacheKey}_{limit}";

        if (_cache.TryGetValue(cacheKey, out List<LaunchDto>? cachedLaunches))
        {
            return cachedLaunches ?? new List<LaunchDto>();
        }
        
        var dbLaunches = await _launchRepository.GetUpcomingAsync(limit);
        var launchesList = dbLaunches.ToList();

        if (launchesList.Count == 0)
        {
            await SyncLaunchesToDatabaseAsync();
            dbLaunches = await _launchRepository.GetUpcomingAsync(limit);
            launchesList = dbLaunches.ToList();
        }

        var launchDtos = launchesList.Select(MapToDto).ToList();
        _cache.Set(cacheKey, launchDtos, TimeSpan.FromHours(_launchesCacheHours));

        return launchDtos;
    }

    public async Task<LaunchDto?> GetLaunchDetailsAsync(string launchLibraryId)
    {
        var launch = await _launchRepository.GetByLaunchLibraryIdAsync(launchLibraryId);

        if (launch == null)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_launchLibraryBaseUrl}/launch/{launchLibraryId}/");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var apiLaunch = ParseLaunchFromJson(json);
                    if (apiLaunch != null)
                    {
                        await _launchRepository.AddAsync(apiLaunch);
                        await _launchRepository.SaveChangesAsync();
                        return MapToDto(apiLaunch);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching launch details for ID: {LaunchId}", launchLibraryId);
            }

            return null;
        }

        return MapToDto(launch);
    }

    public async Task SyncLaunchesToDatabaseAsync()
    {
        try
        {
            _logger.LogInformation("Starting launch sync from Launch Library API");

            var response = await _httpClient.GetAsync($"{_launchLibraryBaseUrl}/launch/upcoming/?limit=50");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;

            if (!root.TryGetProperty("results", out var results))
            {
                _logger.LogWarning("No results found in launch API response");
                return;
            }

            foreach (var launchElement in results.EnumerateArray())
            {
                var launch = ParseLaunchElement(launchElement);
                if (launch == null) continue;

                var existingLaunch = await _launchRepository.GetByLaunchLibraryIdAsync(launch.LaunchLibraryId);
                if (existingLaunch != null)
                {
                    existingLaunch.Name = launch.Name;
                    existingLaunch.LaunchDate = launch.LaunchDate;
                    existingLaunch.Status = launch.Status;
                    existingLaunch.RocketName = launch.RocketName;
                    existingLaunch.LaunchProvider = launch.LaunchProvider;
                    existingLaunch.MissionDescription = launch.MissionDescription;
                    existingLaunch.ImageUrl = launch.ImageUrl;
                    existingLaunch.VideoUrl = launch.VideoUrl;
                    existingLaunch.LaunchSite = launch.LaunchSite;
                    existingLaunch.UpdatedAt = DateTime.UtcNow;
                    _launchRepository.Update(existingLaunch);
                }
                else
                {
                    await _launchRepository.AddAsync(launch);
                }
            }

            await _launchRepository.SaveChangesAsync();
            _logger.LogInformation("Launch sync completed successfully");
            
            _cache.Remove($"{LaunchesCacheKey}_10");
            _cache.Remove($"{LaunchesCacheKey}_50");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing launches from API");
        }
    }

    private Launch? ParseLaunchFromJson(string json)
    {
        try
        {
            using var document = JsonDocument.Parse(json);
            return ParseLaunchElement(document.RootElement);
        }
        catch
        {
            return null;
        }
    }

    private Launch? ParseLaunchElement(JsonElement element)
    {
        try
        {
            var id = element.GetProperty("id").GetString();
            if (string.IsNullOrEmpty(id)) return null;

            var launch = new Launch
            {
                LaunchLibraryId = id,
                Name = element.GetProperty("name").GetString() ?? "Unknown Mission",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            
            if (element.TryGetProperty("net", out var netElement) && netElement.ValueKind != JsonValueKind.Null)
            {
                if (DateTime.TryParse(netElement.GetString(), out var launchDate))
                {
                    launch.LaunchDate = DateTime.SpecifyKind(launchDate.ToUniversalTime(), DateTimeKind.Utc);
                }
            }
            
            if (element.TryGetProperty("status", out var statusElement))
            {
                if (statusElement.TryGetProperty("id", out var statusId))
                {
                    launch.Status = MapStatus(statusId.GetInt32());
                }
            }
            
            if (element.TryGetProperty("rocket", out var rocketElement))
            {
                if (rocketElement.TryGetProperty("configuration", out var configElement))
                {
                    if (configElement.TryGetProperty("full_name", out var fullName))
                    {
                        launch.RocketName = fullName.GetString();
                    }
                    else if (configElement.TryGetProperty("name", out var name))
                    {
                        launch.RocketName = name.GetString();
                    }
                }
            }
            
            if (element.TryGetProperty("launch_service_provider", out var providerElement))
            {
                if (providerElement.TryGetProperty("name", out var providerName))
                {
                    launch.LaunchProvider = providerName.GetString();
                }
            }
            
            if (element.TryGetProperty("mission", out var missionElement) && missionElement.ValueKind != JsonValueKind.Null)
            {
                if (missionElement.TryGetProperty("description", out var description))
                {
                    launch.MissionDescription = description.GetString();
                }
            }
            
            if (element.TryGetProperty("image", out var imageElement) && imageElement.ValueKind != JsonValueKind.Null)
            {
                launch.ImageUrl = imageElement.GetString();
            }
            
            if (element.TryGetProperty("vidURLs", out var vidUrlsElement) && vidUrlsElement.ValueKind == JsonValueKind.Array)
            {
                var firstVideo = vidUrlsElement.EnumerateArray().FirstOrDefault();
                if (firstVideo.ValueKind != JsonValueKind.Undefined)
                {
                    if (firstVideo.TryGetProperty("url", out var videoUrl))
                    {
                        launch.VideoUrl = videoUrl.GetString();
                    }
                }
            }
            
            if (element.TryGetProperty("pad", out var padElement))
            {
                if (padElement.TryGetProperty("name", out var padName))
                {
                    var site = padName.GetString();

                    if (padElement.TryGetProperty("location", out var locationElement))
                    {
                        if (locationElement.TryGetProperty("name", out var locationName))
                        {
                            site = $"{site}, {locationName.GetString()}";
                        }
                    }

                    launch.LaunchSite = site;
                }
            }

            return launch;
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static LaunchStatus MapStatus(int statusId)
    {
        return statusId switch
        {
            1 => LaunchStatus.Scheduled,
            2 => LaunchStatus.ToBeDetermined,
            3 => LaunchStatus.Success,
            4 => LaunchStatus.Failed,
            5 => LaunchStatus.Scheduled,
            6 => LaunchStatus.InFlight,
            7 => LaunchStatus.PartialFailure,
            8 => LaunchStatus.ToBeDetermined,
            _ => LaunchStatus.ToBeDetermined
        };
    }

    private static LaunchDto MapToDto(Launch launch)
    {
        return new LaunchDto(
            Id: launch.Id,
            LaunchLibraryId: launch.LaunchLibraryId,
            Name: launch.Name,
            LaunchDate: launch.LaunchDate,
            Status: launch.Status,
            RocketName: launch.RocketName,
            LaunchProvider: launch.LaunchProvider,
            MissionDescription: launch.MissionDescription,
            ImageUrl: launch.ImageUrl,
            VideoUrl: launch.VideoUrl,
            LaunchSite: launch.LaunchSite,
            TimeUntilLaunch: CalculateTimeUntilLaunch(launch.LaunchDate)
        );
    }

    private static string CalculateTimeUntilLaunch(DateTime? launchDate)
    {
        if (launchDate == null)
        {
            return "TBD";
        }

        var timeUntil = launchDate.Value - DateTime.UtcNow;

        if (timeUntil.TotalSeconds < 0)
        {
            return "Launched";
        }

        if (timeUntil.TotalDays >= 1)
        {
            var days = (int)timeUntil.TotalDays;
            return $"T-{days} day{(days != 1 ? "s" : "")}";
        }

        if (timeUntil.TotalHours >= 1)
        {
            var hours = (int)timeUntil.TotalHours;
            return $"T-{hours} hour{(hours != 1 ? "s" : "")}";
        }

        var minutes = (int)timeUntil.TotalMinutes;
        return $"T-{minutes} min{(minutes != 1 ? "s" : "")}";
    }
}

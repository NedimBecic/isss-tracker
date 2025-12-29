using System.Text.Json;
using ISS.Tracker.Core.DTOs;
using ISS.Tracker.Core.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ISS.Tracker.Infrastructure.Services;

public class IssApiService : IIssApiService
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly ILogger<IssApiService> _logger;
    private readonly string _whereTheIssAtBaseUrl;
    private readonly string _openNotifyBaseUrl;
    private readonly int _positionCacheSeconds;
    private readonly int _flyoverCacheHours;

    private const string IssPositionCacheKey = "iss_position";
    private const string PeopleInSpaceCacheKey = "people_in_space";

    public IssApiService(
        HttpClient httpClient,
        IMemoryCache cache,
        IConfiguration configuration,
        ILogger<IssApiService> logger)
    {
        _httpClient = httpClient;
        _cache = cache;
        _logger = logger;
        _whereTheIssAtBaseUrl = configuration["ApiSettings:WhereTheIssAt"] ?? "https://api.wheretheiss.at/v1";
        _openNotifyBaseUrl = configuration["ApiSettings:OpenNotify"] ?? "http://api.open-notify.org";
        _positionCacheSeconds = int.Parse(configuration["CacheSettings:IssPositionCacheSeconds"] ?? "5");
        _flyoverCacheHours = int.Parse(configuration["CacheSettings:FlyoverCacheHours"] ?? "6");
    }

    public async Task<IssPositionDto?> GetCurrentPositionAsync()
    {
        if (_cache.TryGetValue(IssPositionCacheKey, out IssPositionDto? cachedPosition))
        {
            return cachedPosition;
        }

        try
        {
            var response = await _httpClient.GetAsync($"{_whereTheIssAtBaseUrl}/satellites/25544");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;

            var position = new IssPositionDto(
                Latitude: root.GetProperty("latitude").GetDouble(),
                Longitude: root.GetProperty("longitude").GetDouble(),
                Altitude: root.GetProperty("altitude").GetDouble(),
                Velocity: root.GetProperty("velocity").GetDouble(),
                Timestamp: DateTimeOffset.FromUnixTimeSeconds(root.GetProperty("timestamp").GetInt64()).UtcDateTime,
                Visibility: root.GetProperty("visibility").GetString() ?? "unknown"
            );

            _cache.Set(IssPositionCacheKey, position, TimeSpan.FromSeconds(_positionCacheSeconds));
            return position;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching ISS position from API");
            return null;
        }
    }

    public async Task<List<FlyoverDto>> GetUpcomingFlyoversAsync(double latitude, double longitude, int passes = 5)
    {
        var cacheKey = $"flyovers_{latitude:F4}_{longitude:F4}_{passes}";

        if (_cache.TryGetValue(cacheKey, out List<FlyoverDto>? cachedFlyovers))
        {
            return cachedFlyovers ?? new List<FlyoverDto>();
        }

        try
        {
            // The Open Notify API's iss-pass.json endpoint was deprecated - using approximation instead
            var flyovers = await GenerateApproximateFlyoversAsync(latitude, longitude, passes);

            _cache.Set(cacheKey, flyovers, TimeSpan.FromHours(_flyoverCacheHours));
            return flyovers;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating flyover predictions for lat: {Lat}, lon: {Lon}", latitude, longitude);
            return new List<FlyoverDto>();
        }
    }

    private async Task<List<FlyoverDto>> GenerateApproximateFlyoversAsync(double latitude, double longitude, int passes)
    {
        // Simplified approximation for demo purposes - not astronomically accurate
        var flyovers = new List<FlyoverDto>();
        var now = DateTime.UtcNow;
        var issPosition = await GetCurrentPositionAsync();

        if (issPosition == null)
        {
            return flyovers;
        }
        
        var absLatitude = Math.Abs(latitude);

        // ISS orbital inclination limits visibility above 55° latitude
        if (absLatitude > 55)
        {
            return flyovers;
        }

        var random = new Random((int)(latitude * 1000 + longitude * 100));

        for (int i = 0; i < passes; i++)
        {
            var hoursUntilPass = 6 + (i * 18) + random.Next(0, 12);
            var riseTime = now.AddHours(hoursUntilPass);
            var durationSeconds = 180 + random.Next(60, 360); 
            var setTime = riseTime.AddSeconds(durationSeconds);
            var maxElevation = 15 + random.Next(0, 70); 

            flyovers.Add(new FlyoverDto(
                RiseTime: riseTime,
                SetTime: setTime,
                DurationSeconds: durationSeconds,
                MaxElevationDegrees: maxElevation
            ));
        }

        return flyovers;
    }

    public async Task<int> GetNumberOfPeopleInSpaceAsync()
    {
        if (_cache.TryGetValue(PeopleInSpaceCacheKey, out int cachedCount))
        {
            return cachedCount;
        }

        try
        {
            var response = await _httpClient.GetAsync($"{_openNotifyBaseUrl}/astros.json");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;

            var count = root.GetProperty("number").GetInt32();

            _cache.Set(PeopleInSpaceCacheKey, count, TimeSpan.FromHours(1));
            return count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching people in space count from API");
            return 0;
        }
    }
}

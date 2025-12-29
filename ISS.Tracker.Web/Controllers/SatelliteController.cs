using ISS.Tracker.Core.Interfaces;
using ISS.Tracker.Web.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace ISS.Tracker.Web.Controllers;

public class SatelliteController : Controller
{
    private readonly ILogger<SatelliteController> _logger;
    private readonly IIssApiService _issApiService;

    public SatelliteController(
        ILogger<SatelliteController> logger,
        IIssApiService issApiService)
    {
        _logger = logger;
        _issApiService = issApiService;
    }

    public IActionResult Track()
    {
        var viewModel = new SatelliteTrackViewModel();
        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> GetIssPosition()
    {
        try
        {
            var position = await _issApiService.GetCurrentPositionAsync();
            if (position == null)
            {
                return StatusCode(503, new { error = "Unable to fetch ISS position" });
            }

            return Json(position);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching ISS position");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> GetFlyovers([FromBody] LocationRequest request)
    {
        try
        {
            if (request == null)
            {
                return BadRequest(new { error = "Invalid request" });
            }

            var flyovers = await _issApiService.GetUpcomingFlyoversAsync(
                request.Latitude,
                request.Longitude,
                request.Passes > 0 ? request.Passes : 5
            );

            return Json(flyovers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching flyovers for lat: {Lat}, lon: {Lon}",
                request?.Latitude, request?.Longitude);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }
}

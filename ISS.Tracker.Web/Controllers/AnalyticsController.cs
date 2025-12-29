using ISS.Tracker.Core.Interfaces;
using ISS.Tracker.Web.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace ISS.Tracker.Web.Controllers;

public class AnalyticsController : Controller
{
    private readonly ILogger<AnalyticsController> _logger;
    private readonly ILaunchAnalyticsService _analyticsService;

    public AnalyticsController(
        ILogger<AnalyticsController> logger,
        ILaunchAnalyticsService analyticsService)
    {
        _logger = logger;
        _analyticsService = analyticsService;
    }

    public async Task<IActionResult> Index()
    {
        var viewModel = new AnalyticsViewModel();

        try
        {
            viewModel.CurrentAnalytics = await _analyticsService.GetCurrentAnalyticsAsync();
            viewModel.HistoricalStats = await _analyticsService.GetStatisticsHistoryAsync(7);
            viewModel.YesterdayStats = await _analyticsService.GetStatisticsForDateAsync(DateTime.UtcNow.Date.AddDays(-1));

            await _analyticsService.GenerateAndSaveStatisticsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching analytics data");
        }

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> GetAnalyticsJson()
    {
        try
        {
            var analytics = await _analyticsService.GetCurrentAnalyticsAsync();
            return Json(analytics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching analytics JSON");
            return StatusCode(500, new { error = "Failed to fetch analytics" });
        }
    }
}

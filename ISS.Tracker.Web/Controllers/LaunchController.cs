using ISS.Tracker.Core.Interfaces;
using ISS.Tracker.Web.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace ISS.Tracker.Web.Controllers;

public class LaunchController : Controller
{
    private readonly ILogger<LaunchController> _logger;
    private readonly ILaunchApiService _launchApiService;

    public LaunchController(
        ILogger<LaunchController> logger,
        ILaunchApiService launchApiService)
    {
        _logger = logger;
        _launchApiService = launchApiService;
    }

    public async Task<IActionResult> Index()
    {
        var viewModel = new LaunchListViewModel();

        try
        {
            viewModel.Launches = await _launchApiService.GetUpcomingLaunchesAsync(50);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching launches");
        }

        return View(viewModel);
    }

    public async Task<IActionResult> Details(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return NotFound();
        }

        try
        {
            var launch = await _launchApiService.GetLaunchDetailsAsync(id);

            if (launch == null)
            {
                return NotFound();
            }

            return View(launch);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching launch details for ID: {Id}", id);
            return NotFound();
        }
    }
}

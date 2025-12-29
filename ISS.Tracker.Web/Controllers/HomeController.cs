using System.Diagnostics;
using ISS.Tracker.Core.Interfaces;
using ISS.Tracker.Web.Models;
using ISS.Tracker.Web.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace ISS.Tracker.Web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IIssApiService _issApiService;
    private readonly ILaunchApiService _launchApiService;

    public HomeController(
        ILogger<HomeController> logger,
        IIssApiService issApiService,
        ILaunchApiService launchApiService)
    {
        _logger = logger;
        _issApiService = issApiService;
        _launchApiService = launchApiService;
    }

    public async Task<IActionResult> Index()
    {
        var viewModel = new DashboardViewModel();

        try
        {
            var positionTask = _issApiService.GetCurrentPositionAsync();
            var peopleTask = _issApiService.GetNumberOfPeopleInSpaceAsync();
            var launchesTask = _launchApiService.GetUpcomingLaunchesAsync(5);

            await Task.WhenAll(positionTask, peopleTask, launchesTask);

            viewModel.IssPosition = await positionTask;
            viewModel.PeopleInSpace = await peopleTask;
            viewModel.UpcomingLaunches = await launchesTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching dashboard data");
        }

        return View(viewModel);
    }

    public IActionResult About()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

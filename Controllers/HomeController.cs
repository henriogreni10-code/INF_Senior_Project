using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using INF_SP.Data;
using INF_SP.Models;

namespace INF_SP.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    /// <summary>
    /// Test endpoint to verify Supabase database connection
    /// Access via: /Home/TestConnection
    /// </summary>
    public async Task<IActionResult> TestConnection()
    {
        try
        {
            // Try to connect to the database
            var canConnect = await _context.Database.CanConnectAsync();
            
            if (canConnect)
            {
                // Get database info
                var connection = _context.Database.GetDbConnection();
                var userCount = await _context.Users.CountAsync();
                var eventCount = await _context.Events.CountAsync();
                
                return Json(new
                {
                    success = true,
                    message = "Successfully connected to Supabase!",
                    database = new
                    {
                        server = connection.DataSource,
                        databaseName = connection.Database,
                        state = connection.State.ToString()
                    },
                    statistics = new
                    {
                        users = userCount,
                        events = eventCount
                    }
                });
            }
            
            return Json(new
            {
                success = false,
                message = "Cannot connect to database"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database connection test failed");
            return Json(new
            {
                success = false,
                message = "Connection failed",
                error = ex.Message,
                innerError = ex.InnerException?.Message
            });
        }
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

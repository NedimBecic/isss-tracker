using ISS.Tracker.Core.Interfaces;
using ISS.Tracker.Infrastructure.Data;
using ISS.Tracker.Infrastructure.Data.Repositories;
using ISS.Tracker.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddMemoryCache();

if (builder.Environment.IsDevelopment())
{
    var dbPath = Path.Combine(builder.Environment.ContentRootPath, "iss_tracker.db");
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlite($"Data Source={dbPath}"));
}
else
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
    {
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                               ?? Environment.GetEnvironmentVariable("DATABASE_URL")
                               ?? Environment.GetEnvironmentVariable("DATABASE_PRIVATE_URL")
                               ?? BuildConnectionStringFromEnv();

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException(
                "Database connection string not found. Set DATABASE_URL or PGHOST/PGUSER/PGPASSWORD/PGDATABASE environment variables.");
        }

        options.UseNpgsql(ParsePostgresUri(connectionString));
    });
}

static string? BuildConnectionStringFromEnv()
{
    var host = Environment.GetEnvironmentVariable("PGHOST");
    var port = Environment.GetEnvironmentVariable("PGPORT") ?? "5432";
    var user = Environment.GetEnvironmentVariable("PGUSER");
    var password = Environment.GetEnvironmentVariable("PGPASSWORD");
    var database = Environment.GetEnvironmentVariable("PGDATABASE");

    if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(user) || string.IsNullOrEmpty(database))
        return null;

    return $"Host={host};Port={port};Username={user};Password={password};Database={database};SSL Mode=Require;Trust Server Certificate=true";
}

builder.Services.AddScoped<ISatelliteRepository, SatelliteRepository>();
builder.Services.AddScoped<ILaunchRepository, LaunchRepository>();
builder.Services.AddHttpClient<IIssApiService, IssApiService>();
builder.Services.AddHttpClient<ILaunchApiService, LaunchApiService>();
builder.Services.AddScoped<ILaunchAnalyticsService, LaunchAnalyticsService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    if (app.Environment.IsDevelopment())
        db.Database.EnsureCreated();
    else
        db.Database.Migrate();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

if (app.Environment.IsDevelopment())
{
    app.Run();
}
else
{
    var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
    app.Run($"http://0.0.0.0:{port}");
}

static string ParsePostgresUri(string uriString)
{
    if (string.IsNullOrEmpty(uriString))
        throw new InvalidOperationException("Connection string is null or empty");
    
    if (uriString.Contains("Host=") || uriString.Contains("Server="))
        return uriString;

    if (!uriString.StartsWith("postgres://") && !uriString.StartsWith("postgresql://"))
        throw new InvalidOperationException($"Unrecognized connection string format. Expected postgres:// or postgresql:// URI, or ADO.NET format. Got: {uriString.Substring(0, Math.Min(20, uriString.Length))}...");

    var uri = new Uri(uriString);
    var userInfo = uri.UserInfo.Split(':');

    if (userInfo.Length < 2)
        throw new InvalidOperationException("Connection string URI missing username or password");

    return $"Host={uri.Host};Port={uri.Port};Username={userInfo[0]};Password={userInfo[1]};Database={uri.AbsolutePath.TrimStart('/')};SSL Mode=Require;Trust Server Certificate=true";
}
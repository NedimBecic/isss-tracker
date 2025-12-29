# ISS Tracker

A modern web application that tracks the International Space Station (ISS) in real-time, displays upcoming space launches, and predicts when the ISS will fly over a user's location.

## Features

- **Real-Time ISS Tracking**: Watch the ISS orbit Earth with position updates every 5 seconds
- **Interactive Map**: Leaflet.js-powered map showing the ISS location with auto-follow capability
- **People in Space**: Live count of astronauts currently in orbit
- **Flyover Predictions**: Get predictions for when the ISS will pass over your location using browser geolocation
- **Launch Calendar**: Browse upcoming rocket launches from around the world
- **Launch Details**: View mission details, countdowns, and watch live streams
- **Launch Analytics**: Statistical analysis of launch data with daily snapshots, provider breakdowns, and trend comparisons

## Tech Stack

- **Framework**: ASP.NET Core 8.0 MVC
- **Language**: C# 12
- **Database**: SQLite (development), PostgreSQL (production)
- **ORM**: Entity Framework Core 8.0
- **Caching**: In-Memory Cache (IMemoryCache)
- **Frontend**: Razor Views, Bootstrap 5, Leaflet.js, jQuery

## Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

## Local Development

1. **Clone the repository**
   ```bash
   git clone https://github.com/yourusername/iss-tracker.git
   cd iss-tracker
   ```

2. **Run the application**
   ```bash
   cd ISS.Tracker.Web
   dotnet run
   ```

3. **Open in browser**
   Navigate to `http://localhost:5000`

The app automatically creates a SQLite database (`iss_tracker.db`) on first run.  
For the deployed version, the app uses PostreSQL.

## Project Structure

```
ISS.Tracker/
├── ISS.Tracker.Core/              # Domain/Business Logic Layer
│   ├── Entities/                  # Domain entities (database models)
│   ├── DTOs/                      # Data Transfer Objects
│   └── Interfaces/                # Service and repository interfaces
│
├── ISS.Tracker.Infrastructure/    # Data Access & External Services
│   ├── Data/                      # EF Core DbContext and repositories
│   │   ├── ApplicationDbContext.cs
│   │   └── Repositories/
│   └── Services/                  # External API service implementations
│
└── ISS.Tracker.Web/               # ASP.NET Core MVC Web Application
    ├── Controllers/               # MVC Controllers
    ├── Views/                     # Razor Views
    ├── Models/ViewModels/         # View-specific models
    └── wwwroot/                   # Static files (CSS, JS, images)
```

## API Configuration

The application uses the following free APIs (no API keys required):

| API | Purpose | Base URL |
|-----|---------|----------|
| Where the ISS at? | Real-time ISS position | https://api.wheretheiss.at/v1 |
| Open Notify | People in space count | http://api.open-notify.org |
| Launch Library 2 | Rocket launch data | https://ll.thespacedevs.com/2.2.0 |


## Architecture 
Three distinct layers:
- **Core**: Contains domain entities, DTOs, and interfaces with no external dependencies
- **Infrastructure**: Implements data access and external API services
- **Web**: Handles HTTP requests, views, and dependency injection  

All data access is abstracted through repository interfaces, making the codebase testable and maintainable.

## Database Schema

The application uses 3 database entities:
- **Satellite**: Tracked satellites (ISS is seeded by default)
- **Launch**: Upcoming rocket launches (synced from Launch Library API)
- **LaunchStatistics**: Daily snapshots of launch statistics for historical analysis

**Note**: ISS position data and flyover predictions are fetched from external APIs in real-time and not persisted in the database.

## License

This project is open source and available under the [MIT License](LICENSE).

## Acknowledgments

- [Where the ISS at?](https://wheretheiss.at/) for real-time ISS tracking data
- [Open Notify](http://open-notify.org/) for people in space data
- [The Space Devs](https://thespacedevs.com/) for comprehensive launch data
- [Leaflet.js](https://leafletjs.com/) for interactive maps
- [Bootstrap](https://getbootstrap.com/) for responsive design

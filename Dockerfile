FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["ISS.Tracker.sln", "./"]
COPY ["ISS.Tracker.Core/*.csproj", "ISS.Tracker.Core/"]
COPY ["ISS.Tracker.Infrastructure/*.csproj", "ISS.Tracker.Infrastructure/"]
COPY ["ISS.Tracker.Web/*.csproj", "ISS.Tracker.Web/"]
RUN dotnet restore

COPY . .
WORKDIR "/src/ISS.Tracker.Web"
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "ISS.Tracker.Web.dll"]
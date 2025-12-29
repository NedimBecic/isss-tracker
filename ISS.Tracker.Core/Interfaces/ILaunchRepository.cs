using ISS.Tracker.Core.Entities;

namespace ISS.Tracker.Core.Interfaces;

public interface ILaunchRepository : IRepository<Launch>
{
    Task<IEnumerable<Launch>> GetUpcomingAsync(int limit = 10);
    Task<Launch?> GetByLaunchLibraryIdAsync(string launchLibraryId);
}

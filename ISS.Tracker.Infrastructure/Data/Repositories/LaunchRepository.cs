using ISS.Tracker.Core.Entities;
using ISS.Tracker.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ISS.Tracker.Infrastructure.Data.Repositories;

public class LaunchRepository : Repository<Launch>, ILaunchRepository
{
    public LaunchRepository(ApplicationDbContext context) : base(context) {}

    public async Task<IEnumerable<Launch>> GetUpcomingAsync(int limit = 10)
    {
        return await _dbSet
            .Where(l => l.LaunchDate == null || l.LaunchDate > DateTime.UtcNow)
            .OrderBy(l => l.LaunchDate)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<Launch?> GetByLaunchLibraryIdAsync(string launchLibraryId)
    {
        return await _dbSet.FirstOrDefaultAsync(l => l.LaunchLibraryId == launchLibraryId);
    }
}

using ISS.Tracker.Core.Entities;
using ISS.Tracker.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ISS.Tracker.Infrastructure.Data.Repositories;

public class SatelliteRepository : Repository<Satellite>, ISatelliteRepository
{
    public SatelliteRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Satellite?> GetByNoradIdAsync(int noradId)
    {
        return await _dbSet.FirstOrDefaultAsync(s => s.NoradId == noradId);
    }
}

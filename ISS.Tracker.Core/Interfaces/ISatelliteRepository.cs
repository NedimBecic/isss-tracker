using ISS.Tracker.Core.Entities;

namespace ISS.Tracker.Core.Interfaces;

public interface ISatelliteRepository : IRepository<Satellite>
{
    Task<Satellite?> GetByNoradIdAsync(int noradId);
}

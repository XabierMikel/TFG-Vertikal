using System.Collections.Generic;
using System.Threading.Tasks;
using Vertikal.Core.Models;

namespace Vertikal.Core.Interfaces
{
    public interface IFirebaseSummitService
    {
        Task<List<Summits>> GetSummitListAsync();
        Task<List<Summits>> GetNearbySummitsAsync(double latitude, double longitude, double radiusMeters);

    }
}
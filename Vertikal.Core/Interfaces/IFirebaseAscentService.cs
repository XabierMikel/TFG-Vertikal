using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vertikal.Core.Models;

namespace Vertikal.Core.Interfaces
{
    public interface IFirebaseAscentService
    {
        Task RegisterAscentAsync(Ascent ascent);
        Task<bool> AscentExistsTodayAsync(string userId, string summitId, DateTime date);
        Task<List<Ascent>> GetAscentsByUserIdAsync(string userId);
       
        Task<List<Ascent>> GetAscentsByUserAndDateRangeAsync(string userId, DateTime startDate, DateTime endDate);
    }
}
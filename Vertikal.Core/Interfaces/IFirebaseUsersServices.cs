using System.Threading.Tasks;
using Vertikal.Core.Models;

namespace Vertikal.Core.Interfaces
{
    public interface IFirebaseUsersServices
    {
        Task SaveUserToFirestoreAsync(string uid, string email);
        Task<Users?> GetUserByUidAsync(string uid);
        Task UpdateUserNameAsync(string uid, string name);
    }
}
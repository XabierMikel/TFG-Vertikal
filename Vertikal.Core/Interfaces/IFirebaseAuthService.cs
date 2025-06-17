using System.Threading.Tasks;

namespace Vertikal.Core.Interfaces
{
    public interface IFirebaseAuthService
    {
        Task<string> LoginOrRegisterUserAsync(string email, string password);
        Task ResetPasswordAsync(string email);
        Task LogoutAsync();
        Task<string> GetValidIdTokenAsync();
    }
}
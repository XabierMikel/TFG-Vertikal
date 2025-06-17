using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vertikal.Core.Interfaces
{
    public interface IAuthClient
    {
        Task<IAuthResult> SignInWithEmailAndPasswordAsync(string email, string password);
        Task<IAuthResult> CreateUserWithEmailAndPasswordAsync(string email, string password);
        Task ResetEmailPasswordAsync(string email);
    }

    public interface IAuthResult
    {
        string Uid { get; }
        string RefreshToken { get; }
        Task<string> GetIdTokenAsync();
    }
}

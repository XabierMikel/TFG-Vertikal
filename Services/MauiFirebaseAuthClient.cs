using Firebase.Auth;
using Firebase.Auth.Providers;
using Vertikal.Core.Interfaces;
using Vertikal.Core.Models;

public class MauiFirebaseAuthClient : IAuthClient
{
    private readonly FirebaseAuthClient _client;

    public MauiFirebaseAuthClient(FirebaseConfig config)
    {
        var authConfig = new FirebaseAuthConfig
        {
            ApiKey = config.ApiKey,
            AuthDomain = config.AuthDomain,
            Providers = new FirebaseAuthProvider[]
            {
                new EmailProvider()
            }
        };
        _client = new FirebaseAuthClient(authConfig);
    }

    public async Task<IAuthResult> SignInWithEmailAndPasswordAsync(string email, string password)
    {
        var result = await _client.SignInWithEmailAndPasswordAsync(email, password);
        return new MauiAuthResult(result.User);
    }

    public async Task<IAuthResult> CreateUserWithEmailAndPasswordAsync(string email, string password)
    {
        var result = await _client.CreateUserWithEmailAndPasswordAsync(email, password);
        return new MauiAuthResult(result.User);
    }

    public Task ResetEmailPasswordAsync(string email)
        => _client.ResetEmailPasswordAsync(email);
}

public class MauiAuthResult : IAuthResult
{
    private readonly User _user;
    public MauiAuthResult(User user) => _user = user;
    public string Uid => _user.Uid;
    public string RefreshToken => _user.Credential?.RefreshToken ?? "";
    public Task<string> GetIdTokenAsync() => _user.GetIdTokenAsync();
}
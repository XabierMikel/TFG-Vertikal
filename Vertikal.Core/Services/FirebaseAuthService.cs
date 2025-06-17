using Firebase.Auth;
using System.Text.Json;
using Vertikal.Core.Interfaces;


namespace Vertikal.Core.Services
{
    public class FirebaseAuthService: IFirebaseAuthService
    {
        private readonly IAuthClient _authClient;
        private readonly IApiClient _apiClient;
        private readonly ISecureStorageService _secureStorage;
        private readonly string _firebaseApiKey;


        public FirebaseAuthService(IApiClient apiClient, IAuthClient authClient, ISecureStorageService secureStorage, string firebaseApiKey)
        {
            _apiClient = apiClient;
            _authClient = authClient;
            _secureStorage = secureStorage;
            _firebaseApiKey = firebaseApiKey;
        }

        // Guardar tokens y UID en SecureStorage
        private async Task SaveTokensAsync(string idToken, string refreshToken, string uid)
        {
            await _secureStorage.SetAsync("FirebaseToken", idToken);
            await _secureStorage.SetAsync("FirebaseRefreshToken", refreshToken);
            await _secureStorage.SetAsync("FirebaseUid", uid);
        }

        // Login o registro si no existe el usuario
        public async Task<string> LoginOrRegisterUserAsync(string email, string password)
        {
            try
            {
                var result = await _authClient.SignInWithEmailAndPasswordAsync(email, password);
                await SaveTokensAsync(
                    await result.GetIdTokenAsync(),
                    result.RefreshToken,
                    result.Uid);
                return result.Uid;
            }
            catch (FirebaseAuthException ex) when (ex.Reason == AuthErrorReason.UnknownEmailAddress)
            {
                var result = await _authClient.CreateUserWithEmailAndPasswordAsync(email, password);
                await SaveTokensAsync(
                    await result.GetIdTokenAsync(),
                    result.RefreshToken,
                    result.Uid);
                return result.Uid;
            }
        }


        // Enviar email para restablecer contraseña
        public async Task ResetPasswordAsync(string email)
        {
            await _authClient.ResetEmailPasswordAsync(email);
        }

        // Cerrar sesión: limpia los tokens almacenados
        public async Task LogoutAsync()
        {
            await _secureStorage.SetAsync("FirebaseToken", "");
            await _secureStorage.SetAsync("FirebaseRefreshToken", "");
            await _secureStorage.SetAsync("FirebaseUid", "");
        }

        // Obtener siempre un token válido
        public async Task<string> GetValidIdTokenAsync()
        {
            var idToken = await _secureStorage.GetAsync("FirebaseToken");
            var refreshToken = await _secureStorage.GetAsync("FirebaseRefreshToken");

            if (string.IsNullOrEmpty(idToken) || string.IsNullOrEmpty(refreshToken))
                throw new InvalidOperationException("Usuario no autenticado.");

            // Siempre refrescamos para asegurarnos de que el token está válido
            var newIdToken = await RefreshIdTokenManuallyAsync(refreshToken);
            return newIdToken;
        }

        // Refresca el token usando el endpoint REST de Firebase
        private async Task<string> RefreshIdTokenManuallyAsync(string refreshToken)
        {      
            var url = $"https://securetoken.googleapis.com/v1/token?key={_firebaseApiKey}";

            var payload = new Dictionary<string, string>
            {
                { "grant_type", "refresh_token" },
                { "refresh_token", refreshToken }
            };

            var content = new FormUrlEncodedContent(payload);

            using var client = new HttpClient();
            var response = await client.PostAsync(url, content);



            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new InvalidOperationException("Error al refrescar el token: " + body);

            using var doc = JsonDocument.Parse(body);
            var newIdToken = doc.RootElement.GetProperty("id_token").GetString();
            var newRefreshToken = doc.RootElement.GetProperty("refresh_token").GetString();

            // Guarda los tokens actualizados
            await SaveTokensAsync(newIdToken, newRefreshToken, await _secureStorage.GetAsync("FirebaseUid"));

            return newIdToken;
        }
    }
}

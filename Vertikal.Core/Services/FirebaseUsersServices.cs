using System.Text;
using System.Text.Json;
using Vertikal.Core.Models;
using Vertikal.Core.Helpers;
using Vertikal.Core.Interfaces;

namespace Vertikal.Core.Services
{
    public  class FirebaseUsersServices:IFirebaseUsersServices
    {

        private readonly IFirebaseAuthService _authService;
        private readonly IApiClient _apiClient;


        public FirebaseUsersServices(IFirebaseAuthService authService, IApiClient apiClient)
        {
            _authService = authService;
            _apiClient = apiClient;
        }   

        public async Task SaveUserToFirestoreAsync(string uid, string email)
        {
            var user = new Users
            {         
                Email = email,
                RegisterDate = DateTime.Now.ToString("o"),
                Name = "" // El usuario podrá editarlo después
            };

            var json = JsonSerializer.Serialize(new
            {
                fields = new
                {               
                    Email = new { stringValue = user.Email },
                    RegisterDate = new { stringValue = user.RegisterDate },
                    Name = new { stringValue = user.Name }
                }
            });

            var idToken = await _authService.GetValidIdTokenAsync();
            _apiClient.SetBearerToken(idToken);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _apiClient.PostAsync($"{FirebaseUrls.UsersCollection}?documentId={uid}", content);
       
            response.EnsureSuccessStatusCode();
        }
 

        public async Task<Users?> GetUserByUidAsync(string uid)
        {
            var idToken = await _authService.GetValidIdTokenAsync();
            _apiClient.SetBearerToken(idToken);

            var response = await _apiClient.GetAsync($"{FirebaseUrls.UsersCollection}/{uid}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrWhiteSpace(json))
                {
                    // Deserializa el documento Firestore a tu modelo Users
                    var doc = JsonDocument.Parse(json);
                    var fields = doc.RootElement.GetProperty("fields");
                    return new Users
                    {                
                        Email = fields.GetProperty("Email").GetProperty("stringValue").GetString(),
                        Name = fields.GetProperty("Name").GetProperty("stringValue").GetString(),
                        RegisterDate = fields.GetProperty("RegisterDate").GetProperty("stringValue").GetString()
                    };
                }
            }
            return null;
        }

        public async Task UpdateUserNameAsync(string uid, string name)
        {
            var idToken = await _authService.GetValidIdTokenAsync();
            _apiClient.SetBearerToken(idToken);

            // Solo actualizamos el campo Name
            var json = JsonSerializer.Serialize(new
            {
                fields = new
                {
                    Name = new { stringValue = name }
                }
            });

            // PATCH para actualizar solo el campo Name
            var request = new HttpRequestMessage(HttpMethod.Patch, $"{FirebaseUrls.UsersCollection}/{uid}?updateMask.fieldPaths=Name")
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            var response = await _apiClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
        }




    }
}
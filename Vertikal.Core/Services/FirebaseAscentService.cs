using Firebase.Auth;
using System.Text;
using System.Text.Json;
using Vertikal.Core.Helpers;
using Vertikal.Core.Interfaces;
using Vertikal.Core.Models;

namespace Vertikal.Core.Services
{
    public  class FirebaseAscentService: IFirebaseAscentService
    {
        private readonly IFirebaseAuthService _authService;
        private readonly IApiClient _apiClient;

        public FirebaseAscentService(IFirebaseAuthService authService, IApiClient apiClient)
        {
            _authService = authService;
            _apiClient = apiClient;
        }


        public async Task RegisterAscentAsync(Ascent ascent)
        {
            try
            {
                var idToken = await _authService.GetValidIdTokenAsync();
                _apiClient.SetBearerToken(idToken);

                var document = new
                {
                    fields = new
                    {
                        UserId = new { stringValue = ascent.UserId },
                        SummitId = new { stringValue = ascent.SummitId },
                        Date = new { stringValue = DateTime.Now.ToString("o") },  
                        ValidationMethod = new { stringValue = ascent.ValidationMethod }
                    }
                };


                var json = JsonSerializer.Serialize(document);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _apiClient.PostAsync($"{FirebaseUrls.AscentsCollection}", content);         
                response.EnsureSuccessStatusCode();
             }
            catch (HttpRequestException ex)
            {
                throw new Exception("Error de red al registrar el ascenso.", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Ocurrió un error inesperado al registrar el ascenso.", ex);
            }

        }

        public async Task<bool> AscentExistsTodayAsync(string userId, string summitId, DateTime date)
        {
            var ascents = await GetAscentsByUserIdAsync(userId);

            return ascents.Any(a =>
                a.SummitId == summitId &&
                a.Date.Date == date.Date 
            );
        }

        public async Task<List<Ascent>> GetAscentsByUserIdAsync_Prueba(string userId)
        {
            var idToken = await _authService.GetValidIdTokenAsync();

            _apiClient.SetBearerToken(idToken);

            var query = new
            {
                structuredQuery = new
                {
                    from = new[] { new { collectionId = "Ascents" } }
                }
            };

            var json = JsonSerializer.Serialize(query);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // OJO: esta es la URL modificada para acceder a subcolección
            var response = await _apiClient.PostAsync(
                $"{FirebaseUrls.FirestoreBase}/Users/{userId}/Ascents:runQuery",
                content);

            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();
            var result = JsonDocument.Parse(responseJson);

            var ascents = new List<Ascent>();
            foreach (var doc in result.RootElement.EnumerateArray())
            {
                if (!doc.TryGetProperty("document", out var document)) continue;
                if (!document.TryGetProperty("fields", out var fields)) continue;

                string summitId = fields.TryGetProperty("SummitId", out var summitProp) && summitProp.TryGetProperty("stringValue", out var summitVal)
                    ? summitVal.GetString() : null;
                string userIdField = fields.TryGetProperty("UserId", out var userProp) && userProp.TryGetProperty("stringValue", out var userVal)
                    ? userVal.GetString() : null;
                string date = fields.TryGetProperty("Date", out var dateProp) && dateProp.TryGetProperty("stringValue", out var dateVal)
                    ? dateVal.GetString() : null;
                string validationMethod = fields.TryGetProperty("ValidationMethod", out var validationProp) && validationProp.TryGetProperty("stringValue", out var validationVal)
                    ? validationVal.GetString() : null;

                ascents.Add(new Ascent
                {
                    SummitId = summitId,
                    UserId = userIdField,
                    Date = Convert.ToDateTime(date),
                    ValidationMethod = validationMethod
                });
            }

            return ascents;
        }



        public virtual async Task<List<Ascent>> GetAscentsByUserIdAsync(string userId)
        {
            try
            {
                var idToken = await _authService.GetValidIdTokenAsync();
                _apiClient.SetBearerToken(idToken);

                // Construye la consulta estructurada
                var query = new
                {
                    structuredQuery = new
                    {
                        from = new[] { new { collectionId = "Ascents" } },
                        where = new
                        {
                            fieldFilter = new
                            {
                                field = new { fieldPath = "UserId" },
                                op = "EQUAL",
                                value = new { stringValue = userId }
                            }
                        }
                    }
                };

                var json = JsonSerializer.Serialize(query);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _apiClient.PostAsync($"{FirebaseUrls.FirestoreBase}:runQuery", content);
                response.EnsureSuccessStatusCode();
                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonDocument.Parse(responseJson);

                var ascents = new List<Ascent>();
                foreach (var doc in result.RootElement.EnumerateArray())
                {
                    if (!doc.TryGetProperty("document", out var document)) continue;
                    if (!document.TryGetProperty("fields", out var fields)) continue;

                    string summitId = fields.TryGetProperty("SummitId", out var summitProp) && summitProp.TryGetProperty("stringValue", out var summitVal)
                        ? summitVal.GetString() : null;
                    string userIdField = fields.TryGetProperty("UserId", out var userProp) && userProp.TryGetProperty("stringValue", out var userVal)
                        ? userVal.GetString() : null;
                    string date = fields.TryGetProperty("Date", out var dateProp) && dateProp.TryGetProperty("stringValue", out var dateVal)
                        ? dateVal.GetString() : null;
                    string ValidationMethod = fields.TryGetProperty("ValidationMethod", out var validationProp) && validationProp.TryGetProperty("stringValue", out var validationVal)
                        ? validationVal.GetString() : null;

                    ascents.Add(new Ascent
                    {
                        SummitId = summitId,
                        UserId = userIdField,
                        Date = Convert.ToDateTime(date),
                        ValidationMethod = ValidationMethod
                    });
                }
                return ascents;
            }
            catch (HttpRequestException ex)
            {
                throw new Exception("Error de red al conectar con el servidor.", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Ocurrió un error inesperado al obtener los ascensos.", ex);
            }
           
        }

        public async Task<List<Ascent>> GetAllAscentsAndFilterByUserAsync(string userId)
        {
            var idToken = await _authService.GetValidIdTokenAsync();
            _apiClient.SetBearerToken(idToken);

            var response = await _apiClient.GetAsync($"{FirebaseUrls.AscentsCollection}/Ascents");
            response.EnsureSuccessStatusCode();
            var responseJson = await response.Content.ReadAsStringAsync();
            var result = JsonDocument.Parse(responseJson);

            var ascents = new List<Ascent>();
            foreach (var doc in result.RootElement.GetProperty("documents").EnumerateArray())
            {
                var fields = doc.GetProperty("fields");

                string userIdField = fields.GetProperty("UserId").GetProperty("stringValue").GetString();

                if (userIdField != userId) continue;

                string summitId = fields.GetProperty("SummitId").GetProperty("stringValue").GetString();
                string date = fields.GetProperty("Date").GetProperty("stringValue").GetString();
                string validationMethod = fields.GetProperty("ValidationMethod").GetProperty("stringValue").GetString();

                ascents.Add(new Ascent
                {
                    SummitId = summitId,
                    UserId = userIdField,
                    Date = Convert.ToDateTime(date),
                    ValidationMethod = validationMethod
                });
            }
            return ascents;
        }

        public async Task<List<Ascent>> GetAscentsByUserAndDateRangeAsync(string userId, DateTime startDate, DateTime endDate)
        {
            var allAscents = await GetAscentsByUserIdAsync(userId);
            return allAscents
                .Where(a => a.Date.Date >= startDate.Date && a.Date.Date <= endDate.Date)
                .OrderByDescending(a => a.Date)
                .ToList();
        }

    }
}

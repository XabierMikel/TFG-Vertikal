using System.Text.Json;
using Vertikal.Core.Helpers;
using Vertikal.Core.Interfaces;
using Vertikal.Core.Models;

namespace Vertikal.Core.Services
{
    public class FirebaseSummitService:IFirebaseSummitService
    {

        private readonly IFirebaseAuthService _authService;
        private readonly IApiClient _apiClient;

        public FirebaseSummitService(IFirebaseAuthService authService, IApiClient apiClient)
        {
            _authService = authService;
            _apiClient = apiClient;
        }


        public async Task<List<Summits>> GetSummitListAsync()
        {
            try
            {
                var idToken = await _authService.GetValidIdTokenAsync();
                _apiClient.SetBearerToken(idToken);

                var response = await _apiClient.GetAsync(FirebaseUrls.SummitsCollection);

                if (!response.IsSuccessStatusCode)
                {
                    // Lanzar excepción con el mensaje del servidor
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Error al obtener summits: {response.StatusCode} - {errorContent}");
                }

                var json = await response.Content.ReadAsStringAsync();

                var result = JsonDocument.Parse(json);
                var summits = new List<Summits>();
                foreach (var doc in result.RootElement.GetProperty("documents").EnumerateArray())
                {
                    if (!doc.TryGetProperty("fields", out var fields)) continue;

                    string documentId = doc.GetProperty("name").GetString().Split('/').Last();

                    string name = fields.TryGetProperty("Name", out var nameProp) && nameProp.TryGetProperty("stringValue", out var nameVal)
                        ? nameVal.GetString() : null;

                    string description = fields.TryGetProperty("Description", out var descProp) && descProp.TryGetProperty("stringValue", out var descVal)
                        ? descVal.GetString() : null;

                    var altitud = Helpers.Helpers.GetDoubleFromField(fields, "Altitud");
                    var latitude = Helpers.Helpers.GetDoubleFromField(fields, "Latitude");
                    var longitude = Helpers.Helpers.GetDoubleFromField(fields, "Longitude");

                    string provincia = fields.TryGetProperty("Provincia", out var provProp) && provProp.TryGetProperty("stringValue", out var provVal) ? provVal.GetString() : null;

                    summits.Add(new Summits
                    {
                        Id = documentId,
                        Name = name,
                        Description = description,
                        Altitud = altitud,
                        Latitude = latitude,
                        Longitude = longitude,
                        Provincia = provincia
                    });
                }
                return summits;

            }
            catch (HttpRequestException ex)
            {
                // Manejo de errores de red    
                throw new Exception("Error de red al conectar con el servidor.", ex);
            }
            catch (Exception ex)
            {
                // Manejo de otros errores
                throw new Exception("Ocurrió un error inesperado al obtener los summits.", ex);
            }
        }

        public async Task<List<Summits>> GetNearbySummitsAsync(double userLat, double userLon, double delta)
        {

            try
            {
                var idToken = await _authService.GetValidIdTokenAsync();
                _apiClient.SetBearerToken(idToken);

                // Construir la consulta REST para Firestore con filtros de latitud y longitud
                string url = $"{FirebaseUrls.SummitsCollection}:runQuery";
                var query = new
                {
                    structuredQuery = new
                    {
                        from = new[] { new { collectionId = "Summits" } },
                        where = new
                        {
                            compositeFilter = new
                            {
                                op = "AND",
                                filters = new object[]
                                {
                    new {
                        fieldFilter = new {
                            field = new { fieldPath = "Latitude" },
                            op = "GREATER_THAN_OR_EQUAL",
                            value = new { doubleValue = userLat - delta }
                        }
                    },
                    new {
                        fieldFilter = new {
                            field = new { fieldPath = "Latitude" },
                            op = "LESS_THAN_OR_EQUAL",
                            value = new { doubleValue = userLat + delta }
                        }
                    },
                    new {
                        fieldFilter = new {
                            field = new { fieldPath = "Longitude" },
                            op = "GREATER_THAN_OR_EQUAL",
                            value = new { doubleValue = userLon - delta }
                        }
                    },
                    new {
                        fieldFilter = new {
                            field = new { fieldPath = "Longitude" },
                            op = "LESS_THAN_OR_EQUAL",
                            value = new { doubleValue = userLon + delta }
                        }
                    }
                                }
                            }
                        }
                    }
                };

                var content = new StringContent(JsonSerializer.Serialize(query));
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

                var response = await _apiClient.PostAsync(url, content);

                if (!response.IsSuccessStatusCode)
                {
                    // Lanzar excepción con el mensaje del servidor
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Error al obtener summits: {response.StatusCode} - {errorContent}");
                }

                var json = await response.Content.ReadAsStringAsync();
                var summits = new List<Summits>();
                using (var doc = JsonDocument.Parse(json))
                {
                    foreach (var item in doc.RootElement.EnumerateArray())
                    {
                        if (!item.TryGetProperty("document", out var document)) continue;
                        if (!document.TryGetProperty("fields", out var fields)) continue;

                        string documentId = document.GetProperty("name").GetString().Split('/').Last();

                        string name = fields.TryGetProperty("Name", out var nameProp) && nameProp.TryGetProperty("stringValue", out var nameVal)
                            ? nameVal.GetString() : null;

                        string description = fields.TryGetProperty("Description", out var descProp) && descProp.TryGetProperty("stringValue", out var descVal)
                            ? descVal.GetString() : null;

                        var altitud = Helpers.Helpers.GetDoubleFromField(fields, "Altitud");
                        var latitude = Helpers.Helpers.GetDoubleFromField(fields, "Latitude");
                        var longitude = Helpers.Helpers.GetDoubleFromField(fields, "Longitude");

                        summits.Add(new Summits
                        {
                            Id = documentId,
                            Name = name,
                            Description = description,
                            Altitud = altitud,
                            Latitude = latitude,
                            Longitude = longitude
                        });
                    }
                }
                return summits;
            }


            catch (HttpRequestException ex)
            {
                // Manejo de errores de red    
                throw new Exception("Error de red al conectar con el servidor.", ex);
            }
            catch (Exception ex)
            {
                // Manejo de otros errores
                throw new Exception("Ocurrió un error inesperado al obtener los summits.", ex);
            }
        }
    }
}

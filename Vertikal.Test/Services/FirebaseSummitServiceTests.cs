using Moq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Vertikal.Core.Interfaces;
using Vertikal.Core.Models;
using Vertikal.Core.Services;
using Xunit;

namespace Vertikal.Test.Services
{
    public class FirebaseSummitServiceTests
    {
        private readonly Mock<IFirebaseAuthService> _mockAuthService;
        private readonly Mock<IApiClient> _mockApiClient;
        private readonly FirebaseSummitService _service;

        public FirebaseSummitServiceTests()
        {
            _mockAuthService = new Mock<IFirebaseAuthService>();
            _mockApiClient = new Mock<IApiClient>();
            _service = new FirebaseSummitService(_mockAuthService.Object, _mockApiClient.Object);
        }

        [Fact]
        public async Task GetSummitListAsync_ReturnsSummits_Gorbea()
        {
            // Arrange
            var jsonResponse = """
            {
              "documents": [
                {
                  "name": "projects/test/databases/(default)/documents/Summits/gorbea123",
                  "fields": {
                    "Name": { "stringValue": "Gorbea" },
                    "Description": { "stringValue": "Monte más alto de Bizkaia y Álava" },
                    "Altitud": { "stringValue": "1482" },
                    "Latitude": { "stringValue": "43,066" },
                    "Longitude": { "stringValue": "-2,75" },
                    "Provincia": { "stringValue": "Bizkaia" }
                  }
                }
              ]
            }
            """.Trim();

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
            };

            _mockAuthService.Setup(x => x.GetValidIdTokenAsync()).ReturnsAsync("fake-token");
            _mockApiClient.Setup(x => x.GetAsync(It.IsAny<string>())).ReturnsAsync(response);

            // Act
            var result = await _service.GetSummitListAsync();

            // Assert
            Assert.Single(result);
            Assert.Equal("Gorbea", result[0].Name);
            Assert.Equal("Bizkaia", result[0].Provincia);
            Assert.Equal(1482, result[0].Altitud);
        }

        [Fact]
        public async Task GetNearbySummitsAsync_ReturnsPagasarri_WhenWithinRange()
        {
            // Arrange
            var jsonResponse = """
            [
              {
                "document": {
                  "name": "projects/test/databases/(default)/documents/Summits/pagasarri456",
                  "fields": {
                    "Name": { "stringValue": "Pagasarri" },
                    "Description": { "stringValue": "Monte cercano a Bilbao" },
                    "Altitud": { "stringValue": "673" },
                    "Latitude": { "stringValue": "43,23" },
                    "Longitude": { "stringValue": "-2,94" }
                  }
                }
              }
            ]
            """.Trim();

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
            };

            _mockAuthService.Setup(x => x.GetValidIdTokenAsync()).ReturnsAsync("token");
            _mockApiClient.Setup(x => x.PostAsync(It.IsAny<string>(), It.IsAny<HttpContent>()))
                .ReturnsAsync(response);

            double userLat = 43.22, userLon = -2.93, delta = 0.05;

            // Act
            var result = await _service.GetNearbySummitsAsync(userLat, userLon, delta);

            // Assert
            Assert.Single(result);
            Assert.Equal("Pagasarri", result[0].Name);
            Assert.Equal(673, result[0].Altitud);
        }

        [Fact]
        public async Task GetSummitListAsync_ThrowsException_OnMalformedJson()
        {
            var jsonResponse = "{ this is not valid json ";
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
            };
            _mockAuthService.Setup(x => x.GetValidIdTokenAsync()).ReturnsAsync("fake-token");
            _mockApiClient.Setup(x => x.GetAsync(It.IsAny<string>())).ReturnsAsync(response);

            var ex = await Assert.ThrowsAsync<Exception>(() => _service.GetSummitListAsync());
            Assert.Contains("Ocurrió un error inesperado al obtener los summits", ex.Message);
        }

        [Fact]
        public async Task GetSummitListAsync_ThrowsException_OnNetworkError()
        {
            _mockAuthService.Setup(x => x.GetValidIdTokenAsync()).ReturnsAsync("fake-token");
            _mockApiClient.Setup(x => x.GetAsync(It.IsAny<string>())).ThrowsAsync(new HttpRequestException("Network error"));

            var ex = await Assert.ThrowsAsync<Exception>(() => _service.GetSummitListAsync());
            Assert.Contains("Error de red al conectar con el servidor", ex.Message);
        }

        [Fact]
        public async Task GetSummitListAsync_ThrowsException_OnApiError()
        {
            var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent("Error", Encoding.UTF8, "application/json")
            };
            _mockAuthService.Setup(x => x.GetValidIdTokenAsync()).ReturnsAsync("fake-token");
            _mockApiClient.Setup(x => x.GetAsync(It.IsAny<string>())).ReturnsAsync(response);

            var ex = await Assert.ThrowsAsync<Exception>(() => _service.GetSummitListAsync());
            Assert.Contains("Ocurrió un error inesperado al obtener los summits", ex.Message);
        }

        [Fact]
        public async Task GetNearbySummitsAsync_ThrowsException_OnApiError()
        {
            var response = new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new StringContent("Server error", Encoding.UTF8, "application/json")
            };
            _mockAuthService.Setup(x => x.GetValidIdTokenAsync()).ReturnsAsync("token");
            _mockApiClient.Setup(x => x.PostAsync(It.IsAny<string>(), It.IsAny<HttpContent>()))
                .ReturnsAsync(response);

            var ex = await Assert.ThrowsAsync<Exception>(() => _service.GetNearbySummitsAsync(0, 0, 0));
            Assert.Contains("Ocurrió un error inesperado al obtener los summits", ex.Message);
        }

        [Fact]
        public async Task GetSummitListAsync_ReturnsEmptyList_WhenNoDocuments()
        {
            var jsonResponse = """
             { "documents": [] }
            """;
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
            };
            _mockAuthService.Setup(x => x.GetValidIdTokenAsync()).ReturnsAsync("fake-token");
            _mockApiClient.Setup(x => x.GetAsync(It.IsAny<string>())).ReturnsAsync(response);

            var result = await _service.GetSummitListAsync();
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetNearbySummitsAsync_ThrowsException_OnMalformedJson()
        {
            var jsonResponse = "[ this is not valid json ";
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
            };
            _mockAuthService.Setup(x => x.GetValidIdTokenAsync()).ReturnsAsync("token");
            _mockApiClient.Setup(x => x.PostAsync(It.IsAny<string>(), It.IsAny<HttpContent>()))
                .ReturnsAsync(response);

            var ex = await Assert.ThrowsAsync<Exception>(() => _service.GetNearbySummitsAsync(0, 0, 0));
            Assert.Contains("Ocurrió un error inesperado al obtener los summits", ex.Message);
        }

        [Fact]
        public async Task GetNearbySummitsAsync_ThrowsException_OnNetworkError()
        {
            _mockAuthService.Setup(x => x.GetValidIdTokenAsync()).ReturnsAsync("token");
            _mockApiClient.Setup(x => x.PostAsync(It.IsAny<string>(), It.IsAny<HttpContent>()))
                .ThrowsAsync(new HttpRequestException("Network error"));

            var ex = await Assert.ThrowsAsync<Exception>(() => _service.GetNearbySummitsAsync(0, 0, 0));
            Assert.Contains("Error de red al conectar con el servidor", ex.Message);
        }
    }
}

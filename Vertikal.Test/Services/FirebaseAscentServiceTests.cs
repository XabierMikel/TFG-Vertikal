using Moq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Vertikal.Core.Interfaces;
using Vertikal.Core.Models;
using Vertikal.Core.Services;
using Xunit;

namespace Vertikal.Test.Services
{
    // Clase derivada para testear sin acceder a la red
    internal class FirebaseAscentServiceTestable : FirebaseAscentService
    {
        private readonly List<Ascent> _ascentsToReturn;

        public FirebaseAscentServiceTestable(IFirebaseAuthService authService, IApiClient apiClient, List<Ascent> ascentsToReturn)
            : base(authService, apiClient)
        {
            _ascentsToReturn = ascentsToReturn;
        }

        public override Task<List<Ascent>> GetAscentsByUserIdAsync(string userId)
        {
            return Task.FromResult(_ascentsToReturn);
        }
    }

    public class FirebaseAscentServiceTests
    {
        private readonly Mock<IFirebaseAuthService> _mockAuthService;
        private readonly Mock<IApiClient> _mockApiClient;
        private readonly FirebaseAscentService _service;

        public FirebaseAscentServiceTests()
        {
            _mockAuthService = new Mock<IFirebaseAuthService>();
            _mockApiClient = new Mock<IApiClient>();
            _service = new FirebaseAscentService(_mockAuthService.Object, _mockApiClient.Object);
        }

        [Fact]
        public async Task AscentExistsTodayAsync_ReturnsTrue_IfAscentExists()
        {
            var userId = "user1";
            var summitId = "summit1";
            var today = DateTime.Today;

            var ascents = new List<Ascent>
            {
                new Ascent { UserId = userId, SummitId = summitId, Date = today }
            };

            var service = new FirebaseAscentServiceTestable(_mockAuthService.Object, _mockApiClient.Object, ascents);

            var result = await service.AscentExistsTodayAsync(userId, summitId, today);

            Assert.True(result);
        }

        [Fact]
        public async Task AscentExistsTodayAsync_ReturnsFalse_IfNoAscentExists()
        {
            var userId = "user1";
            var summitId = "summit1";
            var today = DateTime.Today;

            var ascents = new List<Ascent>
            {
                // Un ascenso en otra fecha o a otro summit
                new Ascent { UserId = userId, SummitId = summitId, Date = today.AddDays(-1) },
                new Ascent { UserId = userId, SummitId = "otroSummit", Date = today }
            };

            var service = new FirebaseAscentServiceTestable(_mockAuthService.Object, _mockApiClient.Object, ascents);

            var result = await service.AscentExistsTodayAsync(userId, summitId, today);

            Assert.False(result);


        }

        [Fact]
        public async Task GetAscentsByUserAndDateRangeAsync_FiltersCorrectly()
        {
            // Arrange
            var userId = "user1";
            var ascents = new List<Ascent>
        {
            new Ascent { UserId = userId, SummitId = "s1", Date = new DateTime(2025, 01, 01) },
            new Ascent { UserId = userId, SummitId = "s2", Date = new DateTime(2025, 03, 01) },
            new Ascent { UserId = userId, SummitId = "s3", Date = new DateTime(2025, 05, 01) },
        };

            var startDate = new DateTime(2025, 02, 01);
            var endDate = new DateTime(2025, 04, 01);

            var service = new FirebaseAscentServiceTestable(_mockAuthService.Object, _mockApiClient.Object, ascents);

            // Act
            var filtered = await service.GetAscentsByUserAndDateRangeAsync(userId, startDate, endDate);

            // Assert
            Assert.Single(filtered);
            Assert.Equal("s2", filtered[0].SummitId);
        }

        [Fact]
        public async Task RegisterAscentAsync_CallsApiWithCorrectData()
        {
            // Arrange
            var ascent = new Ascent
            {
                UserId = "user123",
                SummitId = "summit456",
                ValidationMethod = "GPS"
            };

            var idToken = "mock-token";
            _mockAuthService.Setup(x => x.GetValidIdTokenAsync()).ReturnsAsync(idToken);

            _mockApiClient.Setup(x => x.PostAsync(It.IsAny<string>(), It.IsAny<HttpContent>()))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

            // Act
            await _service.RegisterAscentAsync(ascent);

            // Assert
            _mockApiClient.Verify(x => x.SetBearerToken(idToken), Times.Once);
            _mockApiClient.Verify(x =>
                x.PostAsync(It.Is<string>(url => url.Contains("Ascents")),
                    It.Is<HttpContent>(content => ContentContains(content, ascent.UserId, ascent.SummitId))),
                Times.Once);
        }

        [Fact]
        public async Task GetAscentsByUserIdAsync_ReturnsAscentsList()
        {
            // Arrange
            var userId = "user123";
            var fakeToken = "fake-token";
            var fakeJson = @"[
            {
            ""document"": {
                ""fields"": {
                    ""SummitId"": { ""stringValue"": ""summit1"" },
                    ""UserId"": { ""stringValue"": ""user123"" },
                    ""Date"": { ""stringValue"": ""2024-07-01T00:00:00"" },
                    ""ValidationMethod"": { ""stringValue"": ""manual"" }
                   }
                }
             }
             ]";

            var authServiceMock = new Mock<IFirebaseAuthService>();
            authServiceMock.Setup(a => a.GetValidIdTokenAsync()).ReturnsAsync(fakeToken);

            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(fakeJson, Encoding.UTF8, "application/json")
            };

            var apiClientMock = new Mock<IApiClient>();
            apiClientMock.Setup(a => a.SetBearerToken(fakeToken));
            apiClientMock.Setup(a => a.PostAsync(It.IsAny<string>(), It.IsAny<HttpContent>()))
                         .ReturnsAsync(httpResponse);

            var service = new FirebaseAscentService(authServiceMock.Object, apiClientMock.Object);

            // Act
            var result = await service.GetAscentsByUserIdAsync(userId);

            // Assert
            Assert.Single(result);
            Assert.Equal("summit1", result[0].SummitId);
            Assert.Equal("user123", result[0].UserId);
            Assert.Equal("manual", result[0].ValidationMethod);
            Assert.Equal(new DateTime(2024, 7, 1, 0, 0, 0), result[0].Date);
        }


        [Fact]
        public async Task RegisterAscentAsync_ThrowsException_OnApiError()
        {
            var ascent = new Ascent { UserId = "user1", SummitId = "summit1", Date = DateTime.Today };
            _mockAuthService.Setup(x => x.GetValidIdTokenAsync()).ReturnsAsync("token");
            _mockApiClient.Setup(x => x.PostAsync(It.IsAny<string>(), It.IsAny<HttpContent>()))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent("Error")
                });

            await Assert.ThrowsAsync<Exception>(() => _service.RegisterAscentAsync(ascent));
        }

        [Fact]
        public async Task GetAscentsByUserIdAsync_ThrowsException_OnNetworkError()
        {
            var ascent = new Ascent { UserId = "user1", SummitId = "summit1", Date = DateTime.Today };
            _mockAuthService.Setup(x => x.GetValidIdTokenAsync()).ReturnsAsync("token");
            _mockApiClient.Setup(x => x.PostAsync(It.IsAny<string>(), It.IsAny<HttpContent>()))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent("Error")
                });

            var ex = await Assert.ThrowsAsync<Exception>(() => _service.RegisterAscentAsync(ascent));
            Assert.Contains("Error de red al registrar el ascenso", ex.Message);
        }

        private bool ContentContains(HttpContent content, string expectedUserId, string expectedSummitId)
        {
            var json = content.ReadAsStringAsync().Result;
            return json.Contains(expectedUserId) && json.Contains(expectedSummitId);
        }
    }
}
